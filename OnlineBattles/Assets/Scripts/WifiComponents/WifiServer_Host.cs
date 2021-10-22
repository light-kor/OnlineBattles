using GameEnumerations;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public static class WifiServer_Host
{
    public const float UpdateRate = 0.03125f; // Отправка UDP инфы каждые UpdateRate мс 

    public static event DataHolder.Notification AcceptOpponent;    
    public static event DataHolder.StringArrayMessage NewGameControlMessage;

    public static string OpponentStatus = null;
    public static bool OpponentIsReady { get; private set; } = false; //TODO: При дисконнеекте делать false. Переделать бы на подтверждение выбора карты.
    public static WifiOpponent Opponent { get; private set; } = null;
       
    private static TcpListener _listener = null;    
    private static Thread receiveThread = null;
    private static bool _working, _opponentCancelConnect, _pingReceived, _messageHandlerWorking, _messageHandlerClose;

    public static async void StartHosting()
    {
        Network.CloseTcpConnection();
        Network.CloseUdpConnection();
        CloseConnection();
        
        _working = true;
        Network.CreateWifiServerSearcher("spamming");
        _listener = new TcpListener(IPAddress.Any, DataHolder.WifiPort);
        _listener.Start();

        while (true)
        {
            ClearOpponentInfo(); // Это нужно для каждого повторного круга            
            await Task.Run(() => WaitingForTCPConnections());

            if (_working == false)
            {
                CloseConnection();
                break;
            }
            else if (_opponentCancelConnect == true)
                continue;

            await Task.Run(() => WaitingForLogin());

            if (_working == false)
            {
                CloseConnection();
                break;
            }
            else if (_opponentCancelConnect == true)
                continue;

            await Task.Run(() => WaitingPlayerAnswer());

            if (_working == false)
            {
                CloseConnection();
                break;
            }
            else if (_opponentCancelConnect == true)
                continue;

            if (OpponentStatus == "accept")
            {
                Network.CloseWifiServerSearcher();
                Opponent.SendTcpMessage("accept");

                await Task.Delay(500); // Чтоб всё прогрузилось на клиенте, перед запросом пинга
                await Task.Run(() => CheckPingReceipt());

                if (_working == false)
                {
                    CloseConnection();
                    break;
                }
                else if (_opponentCancelConnect == true)
                    continue;

                OpponentIsReady = true;
                AcceptOpponent?.Invoke();
                StopListening();
                break;
            }
            else if (OpponentStatus == "denied")
            {
                Opponent.SendTcpMessage("denied");                
                continue;
            }
        }
    }

    private static void TcpMessageHandler()
    {
        bool needDisconnect = false;
        _messageHandlerClose = false;

        while (_messageHandlerWorking)
        {
            Opponent.GetMessagesFromStream();
            if (Opponent != null && Opponent.TCPMessagesCount > 0)
            {
                string[] mes = Opponent.UseAndDeleteFirstMessage();

                switch (mes[0])
                {                  
                    case "disconnect":
                        needDisconnect = true; // Альтернатинвый выход, чтоб программа не циклилась
                        goto Closing;

                    case "name":
                        Opponent.PlayerName = mes[1];
                        new Notification("Подключился игрок:\r\n" + Opponent.PlayerName, Notification.NotifTypes.WifiRequest, 0);
                        break;

                    case "ping":
                        _pingReceived = true;
                        break;

                    case "Cancel":
                        _opponentCancelConnect = true;
                        new Notification("Игрок отменил запрос", Notification.NotifTypes.WifiRequest, Notification.ButtonTypes.SimpleClose);                       
                        break;

                    case "Check":
                        break;

                    default:
                        NewGameControlMessage?.Invoke(mes);
                        break;
                }
            }
            CheckDisconnect();
        }

        Closing:
        _messageHandlerClose = true;

        if (needDisconnect)
            Disconnect();
    }

    #region ConnectionSteps

    /// <summary>
    /// Ожидание подключения любого игрока.
    /// </summary>
    private static void WaitingForTCPConnections()
    {
        while (_working)
        {
            if (_listener.Pending())
            {             
                Opponent = new WifiOpponent(_listener.AcceptTcpClient(), DateTime.UtcNow);
                
                _messageHandlerWorking = true;
                receiveThread = new Thread(TcpMessageHandler);
                receiveThread.Start();
                return;
            }
        }
    }

    /// <summary>
    /// Ожидание получения имени оппонента.
    /// </summary>
    private static void WaitingForLogin()
    {
        while (_working)
        {
            if (Opponent.PlayerName != null)
                return;

            if (_opponentCancelConnect == true)
                return;
        }
    }

    /// <summary>
    /// Ожидание решения игрока о подключении оппонента.
    /// </summary>
    private static void WaitingPlayerAnswer()
    {
        while (_working)
        {
            if (OpponentStatus != null)
                return;

            if (_opponentCancelConnect == true)
                return;
        }
    }

    /// <summary>
    /// Получения пинга оппонента.
    /// </summary>
    private static void CheckPingReceipt()
    {
        _pingReceived = false;
        Opponent.SendTcpMessage("ping");
        long StartPingTimeInTicks = DateTime.UtcNow.Ticks;
        while (_working)
        {
            if (_pingReceived == true)
            {
                Opponent.Ping = DateTime.UtcNow.Ticks - StartPingTimeInTicks;
                Opponent.SendTcpMessage($"time {DateTime.UtcNow.Ticks - (Opponent.Ping / 2)}");
                return;
            }

            if (_opponentCancelConnect == true)
                return;
        }       
    }
    
    #endregion      

    private static void CheckDisconnect()
    {
        if (Opponent != null)
            if ((DateTime.UtcNow - Opponent.LastReciveTime).TotalSeconds > 5)
                Disconnect();      
    }

    private static void Disconnect()
    {
        CloseConnection();
        string[] str = { "GiveUp" };
        NewGameControlMessage?.Invoke(str);
        DataHolder.GameType = GameTypes.Null; //TODO: Надо ли?
        new Notification("Игрок отключился", Notification.ButtonTypes.MenuButton); //TODO: Настроить и время и действия, а то хз, правильно так или добавить ещё ожидание и дать время на реконнект
    }

    public static void CancelConnect()
    {
        _working = false;
    }

    public static void CloseConnection()
    {
        _working = false;
        Network.CloseWifiServerSearcher();
        ClearOpponentInfo();
        StopListening();       
    }

    private static void StopListening()
    {
        if (_listener != null)
        {
            _listener.Stop();
            _listener = null;
        }
    }

    private static void ClearOpponentInfo()
    {
        OpponentStatus = null;
        OpponentIsReady = false;
        _opponentCancelConnect = false;

        if (Opponent != null)
        {
            Opponent.Client.Close();
            Opponent.ClearStream();
            Opponent = null;
        }       

        _messageHandlerWorking = false;
        if (receiveThread != null)
        {
            while (_messageHandlerClose == false) { } // Ожидание просто для надёжности
            receiveThread = null;
        }                    
    }
}
