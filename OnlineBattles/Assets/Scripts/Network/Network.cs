using GameEnumerations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine.Events;

public static class Network
{    
    public static event UnityAction TcpConnectionIsDone;
    public static event UnityAction<string> WifiServerAnswer;
    public static event UnityAction<string[]> NewGameControlMessage;

    public static bool TryRecconect = true;   
    public static bool ConnectionInProgress = false;
         
    public static TCPConnect ClientTCP { get; private set; } = null;
    public static UDPConnect ClientUDP { get; private set; } = null;
    public static long TimeDifferenceWithServer { get; private set; } = 0;

    public static List<byte[]> BigMessagesTCP = new List<byte[]>();
    public static List<byte[]> MessagesUDP = new List<byte[]>();
    private static List<string[]> _messagesTCP = new List<string[]>();
    private static WifiServer_Searcher _serverSearcher = null;
   
    private static bool _loginSuccessful = false;
    private static bool _messageHandlerIsBusy = false;
    private static bool _earlyTerminationOfConnection = false;
    private static bool _requestDenied = false;

    private const float TimeForWaitAnswer = 10f;

    private static void MessageHandler()
    {
        if (!_messageHandlerIsBusy)
        {
            _messageHandlerIsBusy = true;
            while (_messagesTCP.Count > 0)
            {
                string[] mes = DataHolder.UseAndDeleteFirstListMessage(_messagesTCP);
                switch (mes[0])
                {                   
                    case "login":
                        DataHolder.MyIDInServerSystem = Convert.ToInt32(mes[1]);
                        DataHolder.Money = Convert.ToInt32(mes[2]);
                        _loginSuccessful = true;
                        break;

                    case "ping":
                        ClientTCP.SendMessage("ping");
                        break;                   

                    case "time":
                        TimeDifferenceWithServer = DateTime.UtcNow.Ticks - Convert.ToInt64(mes[1]);
                        break;                                        

                    case "denied":
                        _earlyTerminationOfConnection = true;
                        _requestDenied = true;
                        new Notification("Запрос отклонён", Notification.NotifTypes.Connection, Notification.ButtonTypes.SimpleClose);
                        WifiServerAnswer?.Invoke("denied");
                        break;

                    case "accept":
                        _loginSuccessful = true;
                        NotificationManager.NM.CloseNotification(); // Выключаем панель ожидания                       
                        WifiServerAnswer?.Invoke("accept");
                        break;

                    case "disconnect":
                        CloseTcpConnection();
                        DataHolder.GameType = GameTypes.Null;
                        new Notification("Сервер отключён", Notification.ButtonTypes.MenuButton);
                        break;

                    default:
                        NewGameControlMessage?.Invoke(mes);
                        break;
                }
            }
            _messageHandlerIsBusy = false;
        }       
    }

    public static void ConnectionLifeSupport()
    {
        // Поддержание жизни соединения с сервером.
        if (ClientTCP != null && ClientTCP.ConnectionIsReady == true && (DateTime.UtcNow - ClientTCP.LastSend).TotalMilliseconds > 3000)
            ClientTCP.SendMessage("Check");           
    }

    public static void CreateUDP()
    {
        CloseUdpConnection();
        ClientUDP = new UDPConnect();
    }

    public static void AddNewTCPMessage(string message)
    {
        _messagesTCP.Add(message.Split(' '));
        MessageHandler();
    }

    public static void CreateWifiServerSearcher(string type)
    {
        CloseWifiServerSearcher();
        _serverSearcher = new WifiServer_Searcher(type);
    }
    
    public static async void CreateTCP()
    {     
        new Notification("Ожидание подключения", Notification.NotifTypes.Connection, Notification.ButtonTypes.Waiting);
        await Task.Run(() => TcpConnectionProcess(Notification.NotifTypes.Connection));

        if (ConnectionInProgress)
        {
            ConnectionInProgress = false;
            ClientTCP.ConnectionIsReady = true;

            if (DataHolder.GameType == GameTypes.Multiplayer)
                TcpConnectionIsDone?.Invoke();
        }
    }

    private static void TcpConnectionProcess(Notification.NotifTypes type)
    {
        CloseTcpConnection();
        ConnectionInProgress = true;

        if (DataHolder.GameType == GameTypes.Multiplayer)
        {
            if (!CheckForInternetConnection())
            {
                new Notification("Отсутствует подключение к интернету.", type, Notification.ButtonTypes.SimpleClose);
                return;
            }
        }

        if (CheckForEarlyTerminationOfConnection()) return;

        ClientTCP = new TCPConnect();

        if (CheckForEarlyTerminationOfConnection()) return;

        if (!ConnectionInProgress)
        {
            CloseTcpConnection();
            new Notification("Сервер недоступен", type, Notification.ButtonTypes.SimpleClose);
            return;
        }

        new Notification("Ожидание ответа сервера", type, Notification.ButtonTypes.Waiting);
        LoginInServerSystem();

        if (CheckForEarlyTerminationOfConnection()) return;

        if (!ConnectionInProgress)
        {
            CloseTcpConnection();
            new Notification("Ошибка доступа к серверу", type, Notification.ButtonTypes.SimpleClose);
            return;
        }
    }   

    /// <summary>
    /// Отправка запроса на авторизацию в системе сервера и ожидание подтверждения (получение id и money).
    /// </summary>
    private static void LoginInServerSystem()
    {
        if (DataHolder.GameType == GameTypes.Multiplayer)
            ClientTCP.SendMessage("login " + "DataHolder.KeyCodeName");
        else if (DataHolder.GameType == GameTypes.WifiClient)
            ClientTCP.SendMessage("name " + DataHolder.NickName);

        DateTime StartTryConnect = DateTime.Now;

        while (true)
        {
            if (_earlyTerminationOfConnection) return; // Пользователь отменил коннект или Wifi запрос отклонён

            if (_loginSuccessful) return; // Авторизация прошла успешно

            if (DataHolder.GameType == GameTypes.Multiplayer && ((DateTime.Now - StartTryConnect).TotalSeconds > TimeForWaitAnswer)) // Превышен лимит ожижания
            {
                ConnectionInProgress = false;
                break;
            }
        }
    }

    /// <summary>
    /// Функция реконнекта. Блокировка кнопок на экране, показ всех нужных уведомлений, проверка сети и запуск цикла запросов на повторное соединение с сервером.
    /// </summary>
    public static void StartReconnect() //TODO: Сделать async, как CreateTCP, тк теперь TcpConnectionProcess работает по-другому
    {
        new Notification("Разрыв соединения.\r\nПереподключение...", Notification.NotifTypes.Reconnect, Notification.ButtonTypes.Waiting);

        while (TryRecconect)
        {
            TcpConnectionProcess(Notification.NotifTypes.Reconnect);

            if (ConnectionInProgress)
            {
                // При полном успехе
                ConnectionInProgress = false;
                ClientTCP.ConnectionIsReady = true;
                NotificationManager.NM.CloseStartReconnect();
            }
        }
        TryRecconect = true;
    }  

    /// <summary>
    /// Проверка соединения с интернетом, путём открытия гугловской страницы.
    /// </summary>
    /// <returns>True, если выход в интернет есть.</returns>
    private static bool CheckForInternetConnection()
    {
        try
        {
            using (var client = new WebClient())
            using (client.OpenRead("https://google.com/generate_204"))
                return true;
        }
        catch { return false; }
    }    

    private static bool CheckForEarlyTerminationOfConnection()
    {
        if (_earlyTerminationOfConnection)
        {
            if (!_requestDenied && ClientTCP != null)
                ClientTCP.SendMessage("Cancel"); // Отправить хосту сообщение, что ты больше не ищешь игру.

            CloseTcpConnection();
            _earlyTerminationOfConnection = false;
            ConnectionInProgress = false;           
            _requestDenied = false;
            return true;
        }
        else return false;           
    }   

    #region CloseConnection
    public static void CloseTcpConnection()
    {
        _loginSuccessful = false;
        if (ClientTCP != null)
        {
            ClientTCP.CloseClient();
            ClientTCP = null;
        }
    }

    public static void CloseUdpConnection()
    {
        if (ClientUDP != null)
        {
            ClientUDP.CloseAll();
            ClientUDP = null;
        }
    }

    public static void CloseWifiServerSearcher()
    {
        if (_serverSearcher != null)
        {
            _serverSearcher.CloseAll();
            _serverSearcher = null;
        }
    }

    public static void StopReconnecting()
    {
        CloseTcpConnection();
        TryRecconect = false;
    }

    public static void StopConnecting()
    {
        _earlyTerminationOfConnection = true;
    }
    #endregion
}
