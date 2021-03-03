using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static class WifiServer_Host
{
    public const float UpdateRate = 0.05f; // Отправка UDP инфы каждые UpdateRate мс 

    public static event DataHolder.GameNotification ShowGameNotification;
    public static event DataHolder.Notification CleanHostingUI;
    public static event DataHolder.Notification AcceptOpponent;
    public static event DataHolder.Notification OpponentLeaveTheGame;

    public static string OpponentStatus = null;
    public static bool OpponentIsReady = false;

    public static Opponent_Info _opponent { get; private set; } = null;
       
    private static TcpListener _listener = null;
    private static NetworkStream _streamGame = null;
    private static Thread receiveThread;
    private static bool _searching, _working;

    public static async void StartHosting()
    {
        Network.CloseTcpConnection();
        Network.CloseUdpConnection();
        CloseAll();

        _searching = true;
        _working = true;
        Network.CreateWifiServerSearcher("spamming");
        _listener = new TcpListener(IPAddress.Any, DataHolder.WifiPort);
        _listener.Start();

    StartSearch:
        await Task.Run(() => ListnerClientsTCP());

        if (!_searching)
        {
            CloseAll();
            return;
        }

        await Task.Run(() => WaitForLogin());

        if (!_searching)
        {
            CloseAll();
            return;
        }

        string myDecision = await Task.Run(() => WaitPlayerAnswer());
        
        if (myDecision == "accept")
        {
            Network.CloseWifiServerSearcher();
            SendTcpMessage("accept");

            await Task.Run(() => CheckPing());
            if (!_searching)
            {
                CloseAll();
                return;
            }

            OpponentIsReady = true; //TODO: При дисконнеекте делать false. Переделать бы на подтверждение выбора карты.
            AcceptOpponent?.Invoke();
            receiveThread = new Thread(TcpMessageHandler);
            receiveThread.Start();
            DataHolder.StartMenuView = "WifiHost";
        }
        else if (myDecision == "denied")
        {
            SendTcpMessage("denied");
            ClearOpponentInfo();
            goto StartSearch;
        }
    }

    private static void TcpMessageHandler()
    {
        while (_working)
        {
            GetTcpMessageFromStream();

            if (_opponent.TcpMessages.Count > 0)
            {
                string[] mes = _opponent.TcpMessages[0].Split(' ');
                switch (mes[0])
                {

                    case "leave":
                        OpponentLeaveTheGame?.Invoke();
                        break;

                    case "exit":
                        //TODO: Обработать и в играх, и в главном меню
                        break;

                    case "Check":

                        break;

                    default:

                        break;

                }
                _opponent.TcpMessages.RemoveAt(0);
            }
            CheckDisconnect();
        }
    }

    #region ConnectionSteps

    private static void ListnerClientsTCP()
    {
        while (_searching)
        {
            if (_listener.Pending())
            {
                _opponent = new Opponent_Info(_listener.AcceptTcpClient(), DateTime.UtcNow);
                break;
            }
        }
    }

    private static void WaitForLogin()
    {
        while (_searching)
        {
            GetTcpMessageFromStream();
            if (_opponent.TcpMessages.Count > 0)
            {
                string[] mes = _opponent.TcpMessages[0].Split(' ');
                if (mes[0] == "name")
                {
                    _opponent.PlayerName = mes[1];
                    ShowGameNotification?.Invoke("Подключился игрок:\r\n" + _opponent.PlayerName, 5);
                    _opponent.TcpMessages.RemoveAt(0);
                    break;
                }
                _opponent.TcpMessages.RemoveAt(0);
            }
        }
    }

    private static string WaitPlayerAnswer()
    {
        while (true)
        {
            if (OpponentStatus != null)
            {
                string answer = null;
                if (OpponentStatus == "accept")
                    answer = "accept";
                else if (OpponentStatus == "denied")
                    answer = "denied";

                OpponentStatus = null;
                return answer;
            }
        }
    }

    public static void CheckPing()
    {
        SendTcpMessage("ping");
        long StartPingTimeInTicks = DateTime.UtcNow.Ticks;
        while (_searching)
        {
            GetTcpMessageFromStream();
            if (_opponent.TcpMessages.Count > 0)
            {
                string[] mes = _opponent.TcpMessages[0].Split(' ');
                if (mes[0] == "ping")
                {
                    _opponent.Ping = DateTime.UtcNow.Ticks - StartPingTimeInTicks;
                    SendTcpMessage($"time {DateTime.UtcNow.Ticks + (_opponent.Ping / 2)}");
                    _opponent.TcpMessages.RemoveAt(0);
                    return;
                }
                _opponent.TcpMessages.RemoveAt(0);
            }
        }       
    }
    
    #endregion

    public static void SendTcpMessage(string mes)
    {
        mes += "|";
        byte[] Buffer = Encoding.UTF8.GetBytes((mes).ToCharArray());
        try
        {
            _opponent.Client.GetStream().Write(Buffer, 0, Buffer.Length);
        }
        catch { } //TODO: Тут могут быть какие-то проблемы типо реконнекта как на клиенте?
    }
     
    private static void GetTcpMessageFromStream()
    {
        List<byte> buffer = new List<byte>();
        try
        {
            _streamGame = _opponent.Client.GetStream();
            if (_streamGame.DataAvailable)
            {
                while (_streamGame.DataAvailable)
                {
                    int ReadByte = _streamGame.ReadByte();
                    if (ReadByte != -1)
                    {
                        buffer.Add((byte)ReadByte);
                    }
                }               
            }
            else return;
        }
        catch { return; }

        string[] message = (Encoding.UTF8.GetString(buffer.ToArray())).Split('|');
        List<string> messList = new List<string>(message);
        messList.Remove("");
        for (int i = 0; i < messList.Count; i++)
        {
            _opponent.TcpMessages.Add(messList[i]);
            _opponent.LastReciveTime = DateTime.UtcNow;
        }
    }
    
    public static void EndOfGame(string opponentStatus)
    {
        SendTcpMessage(opponentStatus);
        if (opponentStatus == "drawn")
        {
            ShowGameNotification?.Invoke("Игра завершена\r\ndrawn", 4);
        }
        else if (opponentStatus == "lose")
        {
            ShowGameNotification?.Invoke("Игра завершена\r\nwin", 4);
        }
        else if (opponentStatus == "win")
        {
            ShowGameNotification?.Invoke("Игра завершена\r\nlose", 4);
        }
    }

    public static void CheckDisconnect()
    {
        if ((DateTime.UtcNow - _opponent.LastReciveTime).TotalSeconds > 5)
        {
            OpponentLeaveTheGame?.Invoke();
            CloseAll();
            DataHolder.StartMenuView = null;
            ShowGameNotification?.Invoke("Игрок покинул игру", 4); //TODO: Настроить и время и действия, а то хз, правильно так или добавить ещё ожидание и дать время на реконнект
        }
    }

    public static void CancelWaiting()
    {
        _searching = false;
        CleanHostingUI?.Invoke();
    }

    private static void CloseAll()
    {
        _searching = false;
        _working = false;
        OpponentIsReady = false;
        Network.CloseWifiServerSearcher();
        ClearOpponentInfo();

        if (_listener != null)
        {
            _listener.Stop();
            _listener = null;
        }

        if (_streamGame != null)
        {
            _streamGame.Close();
            _streamGame = null;
        }

        OpponentStatus = null;
        
        //TODO: Мб Отправить сообщение подключённом игроку на всякий случай
        //TODO: Вернуть в самое начало меню
    }

    private static void ClearOpponentInfo()
    {
        if (_opponent != null)
        {
            _opponent.Client.Close();
            _opponent = null;
        }
    }
}
