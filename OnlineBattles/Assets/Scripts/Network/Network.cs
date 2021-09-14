using System;
using System.Net;
using System.Threading.Tasks;

public static class Network
{    
    public static event DataHolder.Notification TcpConnectionIsDone;
    public static event DataHolder.TextЕransmissionEnvent EndOfGame;
    public static event DataHolder.TextЕransmissionEnvent WifiServerAnswer;

    public static bool TryRecconect = true;   
    public static bool ConnectionInProgress = false;

    private const float TimeForWaitAnswer = 10f;   
    private static bool _loginSuccessful = false;
    private static bool _messageHandlerIsBusy = false;
    private static bool _earlyTerminationOfConnection = false;
    private static bool _requestDenied = false;

    public static void MessageHandler()
    {
        if (!_messageHandlerIsBusy)
        {
            _messageHandlerIsBusy = true;
            while (DataHolder.MessageTCP.Count > 0)
            {
                string[] mes = DataHolder.MessageTCP[0].Split(' ');
                switch (mes[0])
                {
                    case "win":
                    case "lose":
                    case "drawn":
                        EndOfGame?.Invoke(mes[0]);
                        break;

                    case "login":
                        DataHolder.MyIDInServerSystem = Convert.ToInt32(mes[1]);
                        DataHolder.Money = Convert.ToInt32(mes[2]);
                        _loginSuccessful = true;
                        break;

                    case "ping":
                        DataHolder.ClientTCP.SendMessage("ping");
                        break;                   

                    case "time":
                        DataHolder.TimeDifferenceWithServer = DateTime.UtcNow.Ticks - Convert.ToInt64(mes[1]);
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

                    default:
                        DataHolder.MessageTCPforGame.Add(DataHolder.MessageTCP[0]);
                        break;

                }
                DataHolder.MessageTCP.RemoveAt(0);
            }
            _messageHandlerIsBusy = false;
        }       
    }

    //TODO: Теперь это костыль тк надо привязать к открытым сценам юнити и + тепрь надо самому вставлять во все сцены. те пока пусть будет в NotificationPanels
    public static void ConnectionLifeSupport()
    {
        // Поддержание жизни соединения с сервером.
        if (DataHolder.ClientTCP != null && DataHolder.ClientTCP.ConnectionIsReady == true && (DateTime.UtcNow - DataHolder.LastSend).TotalMilliseconds > 3000)
            DataHolder.ClientTCP.SendMessage("Check");           
    }

    public static void CreateUDP()
    {
        CloseUdpConnection();
        DataHolder.ClientUDP = new UDPConnect();
    }

    public static void CreateWifiServerSearcher(string type)
    {
        CloseWifiServerSearcher();
        DataHolder.ServerSearcher = new WifiServer_Searcher(type);
    }
    
    public static async void CreateTCP()
    {     
        new Notification("Ожидание подключения", Notification.NotifTypes.Connection, Notification.ButtonTypes.Waiting);
        await Task.Run(() => TcpConnectionProcess(Notification.NotifTypes.Connection));

        if (ConnectionInProgress)
        {
            ConnectionInProgress = false;
            DataHolder.ClientTCP.ConnectionIsReady = true;

            if (DataHolder.GameType == DataHolder.GameTypes.Multiplayer)
                TcpConnectionIsDone?.Invoke();
        }
    }

    private static void TcpConnectionProcess(Notification.NotifTypes type)
    {
        CloseTcpConnection();
        ConnectionInProgress = true;

        if (DataHolder.GameType == DataHolder.GameTypes.Multiplayer)
        {
            if (!CheckForInternetConnection())
            {
                new Notification("Отсутствует подключение к интернету.", type, Notification.ButtonTypes.SimpleClose);
                return;
            }
        }

        if (CheckForEarlyTerminationOfConnection()) return;

        DataHolder.ClientTCP = new TCPConnect();

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
        if (DataHolder.GameType == DataHolder.GameTypes.Multiplayer)
            DataHolder.ClientTCP.SendMessage("login " + DataHolder.KeyCodeName);
        else if (DataHolder.GameType == DataHolder.GameTypes.WifiClient)
            DataHolder.ClientTCP.SendMessage("name " + DataHolder.NickName);

        DateTime StartTryConnect = DateTime.Now;

        while (true)
        {
            if (_earlyTerminationOfConnection) return; // Пользователь отменил коннект или Wifi запрос отклонён

            if (_loginSuccessful) return; // Авторизация прошла успешно

            if (DataHolder.GameType == DataHolder.GameTypes.Multiplayer && ((DateTime.Now - StartTryConnect).TotalSeconds > TimeForWaitAnswer)) // Превышен лимит ожижания
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
                DataHolder.ClientTCP.ConnectionIsReady = true;
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
            if (!_requestDenied && DataHolder.ClientTCP != null)
                DataHolder.ClientTCP.SendMessage("Cancel"); // Отправить хосту сообщение, что ты больше не ищешь игру.

            CloseTcpConnection();
            _earlyTerminationOfConnection = false;
            ConnectionInProgress = false;           
            _requestDenied = false;
            return true;
        }
        else return false;           
    }

    public static void CloseTcpConnection()
    {
        _loginSuccessful = false;
        if (DataHolder.ClientTCP != null)
        {
            DataHolder.ClientTCP.CloseClient();
            DataHolder.ClientTCP = null;
        }
    }

    public static void CloseUdpConnection()
    {
        if (DataHolder.ClientUDP != null)
        {
            DataHolder.ClientUDP.CloseAll();
            DataHolder.ClientUDP = null;
        }
    }

    public static void CloseWifiServerSearcher()
    {
        if (DataHolder.ServerSearcher != null)
        {
            DataHolder.ServerSearcher.CloseAll();
            DataHolder.ServerSearcher = null;
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
}
