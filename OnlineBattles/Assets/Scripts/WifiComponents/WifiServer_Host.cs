using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class WifiServer_Host
{
    public const float UpdateRate = 0.03125f; // Отправка UDP инфы каждые UpdateRate мс 

    public static event DataHolder.Notification CleanHostingUI;
    public static event DataHolder.Notification AcceptOpponent;
    public static event DataHolder.Notification OpponentGaveUp;

    public static string OpponentStatus = null;
    public static bool OpponentIsReady { get; private set; } = false;
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

            await Task.Delay(500); // Чтоб всё прогрузилось на клиенте, перед запросом пинга
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

                    case "GiveUp":
                        OpponentGaveUp?.Invoke();
                        break;

                    case "disconnect":
                        Disconnect();
                        return;

                    case "Check":
                        break;

                    default:
                        _opponent.MessageTCPforGame.Add(_opponent.TcpMessages[0]);
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
                _streamGame = _opponent.Client.GetStream();
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
                    NotificationPanels.NP.AddNotificationToQueue("Подключился игрок:\r\n" + _opponent.PlayerName, 5);
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

    private static void CheckPing()
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
                    SendTcpMessage($"time {DateTime.UtcNow.Ticks - (_opponent.Ping / 2)}");
                    _opponent.TcpMessages.RemoveAt(0);
                    return;
                }
                _opponent.TcpMessages.RemoveAt(0);
            }
        }       
    }
    
    #endregion

    public static void SendTcpMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        byte[] sizeInByte = BitConverter.GetBytes(buffer.Length);
        try
        {
            _opponent.Client.GetStream().Write(sizeInByte, 0, sizeInByte.Length);
            _opponent.Client.GetStream().Write(buffer, 0, buffer.Length);
        }
        catch { } //TODO: Тут могут быть какие-то проблемы типо реконнекта как на клиенте?
    }

    private static void GetTcpMessageFromStream() //TODO: Мб сделать в отдельном потоке
    {
        List<byte> buffer = new List<byte>();
        try
        {
            while (buffer.Count < 4)
                CheckStream(buffer);

            int mesCount = BitConverter.ToInt32(buffer.ToArray(), 0);
            buffer.Clear();

            while (buffer.Count < mesCount)
                CheckStream(buffer);
        }
        catch { return; }

        string message = Encoding.UTF8.GetString(buffer.ToArray());
        _opponent.TcpMessages.Add(message);
        _opponent.LastReciveTime = DateTime.UtcNow;
    }

    private static void CheckStream(List<byte> Buffer)
    {
        if (_streamGame.DataAvailable)
        {
            int ReadByte = _streamGame.ReadByte();
            if (ReadByte > -1)
            {
                Buffer.Add((byte)ReadByte);
            }
        }
    }

    private static void CheckDisconnect()
    {
        if ((DateTime.UtcNow - _opponent.LastReciveTime).TotalSeconds > 5)
        {
            Disconnect();
        }
    }

    private static void Disconnect()
    {
        OpponentGaveUp?.Invoke();
        CloseAll();
        DataHolder.StartMenuView = null;
        NotificationPanels.NP.AddNotificationToQueue("Игрок отключился", 4); //TODO: Настроить и время и действия, а то хз, правильно так или добавить ещё ожидание и дать время на реконнект
    }


    public static void CancelWaiting()
    {
        _searching = false;
        CleanHostingUI?.Invoke();
    }

    public static void CloseAll()
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
