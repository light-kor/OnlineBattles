using System;
using System.Net;
using System.Threading.Tasks;

public static class Network
{    
    public static event DataHolder.Notification TcpConnectionIsDone;
    public static event DataHolder.Notification EndOfGame;
    public static event DataHolder.GameNotification ShowGameNotification;

    private const float TimeForWaitAnswer = 3f;
    public static bool TryRecconect { get; set; } = true;
    private static bool WaitingForLogin = true;
    private static bool MessageHandlerIsBusy = false;

    private static void MessageHandler()
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
                        EndOfGame?.Invoke();
                        ShowGameNotification?.Invoke("Игра завершена\r\n" + mes[0], 4);
                        break;

                    case "login":
                        DataHolder.MyServerID = Convert.ToInt32(mes[1]);
                        DataHolder.Money = Convert.ToInt32(mes[2]);
                        WaitingForLogin = false;
                        break;

                    case "ping":
                        DataHolder.ClientTCP.SendMessage("ping");
                        break;

                    case "time":
                        DataHolder.TimeDifferenceWithServer = Convert.ToInt64(mes[1]) - DateTime.UtcNow.Ticks;
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
        if (DataHolder.ClientTCP != null && (DateTime.UtcNow - DataHolder.LastSend).TotalMilliseconds > 3000)
            DataHolder.ClientTCP.SendMessage("Check");
    }

    /// <summary>
    /// Создание экземпляра ClientUDP и установка UDP "соединения".
    /// </summary>
    public static void CreateUDP()
    {
        if (DataHolder.ClientUDP != null)
        {
            DataHolder.ClientUDP.CloseClient();
            DataHolder.ClientUDP = null;
        }
        DataHolder.ClientUDP = new UDPConnect();
    }

    /// <summary>
    /// Установка соединения с сервером (асинхронная): проверка интернета, создание экземпляра TcpConnect и авторизация в системе.
    /// </summary>
    public static async void CreateTCP()
    {
        //TODO: Добавить анимацию загрузки, что было понятно, что надо подождать
        ShowGameNotification?.Invoke("Ожидание подключения", 0);

        if (!await Task.Run(() => CheckForInternetConnection()))
        {
            ShowGameNotification?.Invoke("Отсутствует подключение к интернету.", 1);           
            return;
        }

        // Асинхронность нужна чтоб сначала показать уведомление о начале подключения, а потом уже подключать. 
        await Task.Run(() => DataHolder.ClientTCP = new TcpConnect());
        DataHolder.ClientTCP.GetMessage += MessageHandler;

        if (!DataHolder.Connected)
        {
            CleanTcpConnection();
            ShowGameNotification?.Invoke("Сервер не доступен. Попробуйте позже.", 1);
            return;
        }

        await Task.Run(() => LoginInServerSystem());

        if (!DataHolder.Connected)
        {
            CleanTcpConnection();
            ShowGameNotification?.Invoke("Ошибка доступа к серверу.", 1);
            return;
        }
        else
        {
            TcpConnectionIsDone?.Invoke();
            DataHolder.ClientTCP._canStartReconnect = true;
        }
    }

    /// <summary>
    /// Отправка запроса на авторизацию в системе сервера и ожидание подтверждения (получение id и money).
    /// </summary>
    private static void LoginInServerSystem()
    {
        DataHolder.ClientTCP.SendMessage("login " + DataHolder.KeyCodeName);

        DateTime StartTryConnect = DateTime.Now;
        //TODO: При этом, пока телефон думает, пусть в углу будет гифка загрузки, чтоб пользователь понимал, что что-то происходит
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
    public static async void StartReconnect()
    {
        DataHolder.Connected = false;
        CleanTcpConnection();
        ShowGameNotification?.Invoke("Разрыв соединения.\r\nПереподключение...", 2);

        while (TryRecconect)
        {
            if (!await Task.Run(() => CheckForInternetConnection()))
            {
                ShowGameNotification?.Invoke("Разрыв соединения.\r\nОтсутствует подключение к интернету.\r\nОжидание...", 2);
                continue;
            }
            else ShowGameNotification?.Invoke("Разрыв соединения.\r\nПодключение к серверу...", 2);

            await Task.Run(() => DataHolder.ClientTCP = new TcpConnect());
            DataHolder.ClientTCP.GetMessage += MessageHandler;

            if (!DataHolder.Connected)
            {
                CleanTcpConnection();
                continue;
            }

            ShowGameNotification?.Invoke("Разрыв соединения.\r\nОжидание ответа сервера", 2);
            await Task.Run(() => LoginInServerSystem());

            if (!DataHolder.Connected)
            {
                CleanTcpConnection();
                continue;
            }

            // При полном успехе
            DataHolder.NotifPanels.NotificatonMultyButton(10);
            DataHolder.ClientTCP._canStartReconnect = true;
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
    private static void CleanTcpConnection()
    {
        if (DataHolder.ClientTCP != null)
        {
            DataHolder.ClientTCP.GetMessage -= MessageHandler;
            DataHolder.ClientTCP.CloseClient();
            DataHolder.ClientTCP = null;
        }
    }

    public static void StopReconnecting()
    {
        CleanTcpConnection();
        TryRecconect = false;
    }

    //TODO: Очистка всего, связанного  с соединениями, и тем, что создано в этом скрипте
    private static void Clear()
    {

    }
}
