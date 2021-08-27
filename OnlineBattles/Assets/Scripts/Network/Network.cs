using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public static class Network
{    
    public static event DataHolder.Notification TcpConnectionIsDone;
    public static event DataHolder.TextЕransmissionEnvent EndOfGame;
    public static event DataHolder.TextЕransmissionEnvent WifiServerAnswer;

    public static bool TryRecconect = true;

    private const float TimeForWaitAnswer = 5f;   
    private static bool WaitingForLogin = true;
    private static bool MessageHandlerIsBusy = false;

    public static void MessageHandler()
    {
        if (!MessageHandlerIsBusy)
        {
            MessageHandlerIsBusy = true;
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
                        WaitingForLogin = false;
                        break;

                    case "ping":
                        DataHolder.ClientTCP.SendMessage("ping");
                        break;                   

                    case "time":
                        DataHolder.TimeDifferenceWithServer = DateTime.UtcNow.Ticks - Convert.ToInt64(mes[1]);
                        break;

                    case "denied":
                        CloseTcpConnection();
                        NotificationManager.NM.CloseNotification(); // Выключаем панель ожидания
                        new Notification("Запрос отклонён", Notification.ButtonTypes.SimpleClose);
                        WifiServerAnswer?.Invoke("denied");
                        break;

                    case "accept":
                        NotificationManager.NM.CloseNotification(); // Выключаем панель ожидания
                        WifiServerAnswer?.Invoke("accept");
                        break;                   

                    default:
                        DataHolder.MessageTCPforGame.Add(DataHolder.MessageTCP[0]);
                        break;

                }
                DataHolder.MessageTCP.RemoveAt(0);
            }
            MessageHandlerIsBusy = false;
        }       
    }

    //TODO: Теперь это костыль тк надо привязать к открытым сценам юнити и + тепрь надо самому вставлять во все сцены. те пока пусть будет в NotificationPanels
    public static void ConnectionLifeSupport()
    {
        // Поддержание жизни соединения с сервером.
        if (DataHolder.ClientTCP != null && DataHolder.Connected == true && (DateTime.UtcNow - DataHolder.LastSend).TotalMilliseconds > 3000)
            DataHolder.ClientTCP.SendMessage("Check");           
    }

    /// <summary>
    /// Создание экземпляра ClientUDP и установка UDP "соединения".
    /// </summary>
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
    
    /// <summary>
    /// Установка соединения с сервером (асинхронная): проверка интернета, создание экземпляра TcpConnect и авторизация в системе.
    /// </summary>
    public static void CreateTCP()
    {
        //TODO: Добавить анимацию загрузки, что было понятно, что надо подождать
        var notifType = Notification.NotifTypes.Connection;       
        //NotificationManager.NM.AddNotificationToQueue("Ожидание подключения", notifType);
        new Notification("Ожидание подключения", notifType, Notification.ButtonTypes.Waiting);

        TcpConnectionProcess(notifType);

        if (DataHolder.Connected)
        {
            TcpConnectionIsDone?.Invoke();
            if (DataHolder.ClientTCP != null)
            {
                DataHolder.ClientTCP.CanStartReconnect = true; //TODO: Чёт здесь какая-то фигня
            }
        }
    }

    private static async void TcpConnectionProcess(Notification.NotifTypes type)
    {
        CloseTcpConnection();

        if (DataHolder.GameType == "Multiplayer")
        {
            if (!await Task.Run(() => CheckForInternetConnection()))
            {
                new Notification("Отсутствует подключение к интернету.", type, Notification.ButtonTypes.SimpleClose);
                return;
            }
        }

        // Асинхронность нужна чтоб сначала показать уведомление о начале подключения, а потом уже подключать. 
        await Task.Run(() => DataHolder.ClientTCP = new TCPConnect());

        if (!DataHolder.Connected)
        {
            CloseTcpConnection();
            //NotificationManager.NM.AddNotificationToQueue("Сервер недоступен.", type);
            new Notification("Сервер недоступен.", type, Notification.ButtonTypes.SimpleClose);
            return;
        }

        new Notification("Ожидание ответа сервера", type, Notification.ButtonTypes.Waiting);
        await Task.Run(() => LoginInServerSystem());

        if (!DataHolder.Connected)
        {
            CloseTcpConnection();
            new Notification("Ошибка доступа к серверу.", type, Notification.ButtonTypes.SimpleClose);
            return;
        }
    }

    /// <summary>
    /// Отправка запроса на авторизацию в системе сервера и ожидание подтверждения (получение id и money).
    /// </summary>
    private static void LoginInServerSystem()
    {
        if (DataHolder.GameType == "Multiplayer")
            DataHolder.ClientTCP.SendMessage("login " + DataHolder.KeyCodeName);
        else if (DataHolder.GameType == "WifiClient")
        {
            DataHolder.ClientTCP.SendMessage("name " + DataHolder.NickName);
            return;
        }

        DateTime StartTryConnect = DateTime.Now;

        while (true)
        {
            if (((DateTime.Now - StartTryConnect).TotalSeconds < TimeForWaitAnswer))
            {
                if (!WaitingForLogin)
                    break;
            }
            else
            {
                DataHolder.Connected = false;
                break;
            }
        }
        WaitingForLogin = true;
    }

    /// <summary>
    /// Функция реконнекта. Блокировка кнопок на экране, показ всех нужных уведомлений, проверка сети и запуск цикла запросов на повторное соединение с сервером.
    /// </summary>
    public static void StartReconnect()
    {
        DataHolder.Connected = false;
        new Notification("Разрыв соединения.\r\nПереподключение...", Notification.NotifTypes.Reconnect, Notification.ButtonTypes.Waiting);

        while (TryRecconect)
        {
            TcpConnectionProcess(Notification.NotifTypes.Reconnect);
            if (DataHolder.Connected)
            {
                // При полном успехе
                NotificationManager.NM.CloseStartReconnect();
                DataHolder.ClientTCP.CanStartReconnect = true;
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
            using (client.OpenRead("http://google.com/generate_204"))
                return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// Очистка и удаление ClientTCP и всего TCP соединения.
    /// </summary>
    public static void CloseTcpConnection()
    {       
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
}
