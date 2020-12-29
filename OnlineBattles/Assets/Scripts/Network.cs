using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Network : MonoBehaviour
{
    public GameObject NotifPanel, NotifButton, StopReconnectButton, CancelSearchButton, CloseEndGameButton;
    public GameObject Shield; // Блокирует нажатия на все кнопки, кроме notifPanel

    private const float TimeForWaitAnswer = 3f;
    private bool TryRecconect { get; set; } = true;

    void Awake()
    {
        DataHolder.NetworkScript = this;       
    }

    private void Update()
    {
        DataHolder.ServerTime += Convert.ToInt64(Time.deltaTime * 10000 * 1000);
        if (DataHolder.MessageTCP.Count > 0)
        {
            string[] mes = DataHolder.MessageTCP[0].Split(' ');
            Debug.Log($"net {DataHolder.MessageTCP[0]}");
            switch (mes[0])
            {
                case "win":
                case "lose":
                case "drawn":
                    //TODO: Нужен какой-то мультивыбор скрипта ниже, а не только этот. ВОТ ТУТ И НУЖНО НАСЛЕДОВАНИЕ И ИНКАПСУЛЯЦИЯ.
                    GetComponent<UDPGame>().CloseAll();
                    ShowNotif("Игра завершена\r\n" + mes[0], 4);                    
                    break;

                case "login":
                    return;

                case "ping":
                    DataHolder.ClientTCP.SendMessage("ping");
                    break;

                case "time":
                    DataHolder.ServerTime = Convert.ToInt64(mes[1]);
                    break;

                default:
                    DataHolder.MessageTCPforGame.Add(DataHolder.MessageTCP[0]);
                    break;

            }
            DataHolder.MessageTCP.RemoveAt(0); 
        }

        // Поддержание жизни соединения с сервером.
        if ((DateTime.UtcNow - DataHolder.LastSend).TotalMilliseconds > 3000 && DataHolder.ClientTCP != null)
            DataHolder.ClientTCP.SendMessage("Check");
    }

    /// <summary>
    /// Создание экземпляра ClientUDP и установка UDP "соединения".
    /// </summary>
    public void CreateUDP()
    {
        DataHolder.ClientUDP = new UDPConnect();
    }

    /// <summary>
    /// Установка соединения с сервером (асинхронная): проверка интернета, создание экземпляра TcpConnect и авторизация в системе.
    /// </summary>
    public async void CreateTCP()
    {
        //TODO: Добавить анимацию загрузки, что было понятно, что надо подождать
        ShowNotif("Ожидание подключения", 0);

        if (!await Task.Run(() => CheckForInternetConnection()))
        {
            ShowNotif("Отсутствует подключение к интернету.", 1);           
            return;
        }

        // Асинхронность нужна чтоб сначала показать уведомление о начале подключения, а потом уже подключать. 
        await Task.Run(() => DataHolder.ClientTCP = new TcpConnect());
        DataHolder.ClientTCP.GetMessage += SOS;

        if (!DataHolder.Connected)
        {
            CleanTcpConnection();
            ShowNotif("Сервер не доступен. Попробуйте позже.", 1);
            return;
        }

        await Task.Run(() => LoginInServerSystem());

        if (!DataHolder.Connected)
        {
            CleanTcpConnection();
            ShowNotif("Ошибка доступа к серверу.", 1);
            return;
        }
        else
        {
            GetComponent<MainMenuScr>().GoToMultiplayerMenu();
            DataHolder.ClientTCP.CanStartReconnect = true;
        }
    }

    /// <summary>
    /// Отправка запроса на авторизацию в системе сервера и ожидание подтверждения (получение id и money).
    /// </summary>
    private void LoginInServerSystem()
    {
        DataHolder.ClientTCP.SendMessage("login " + DataHolder.KeyCodeName);

        DateTime StartTryConnect = DateTime.Now;
        //TODO: При этом, пока телефон думает, пусть в углу будет гифка загрузки, чтоб пользователь понимал, что что-то происходит
        while (true)
        {
            if (((DateTime.Now - StartTryConnect).TotalSeconds < TimeForWaitAnswer))
            {
                // Получаем id и деньги от сервера
                if (DataHolder.MessageTCP.Count > 0)
                {
                    string[] mes = DataHolder.MessageTCP[0].Split(' ');
                    if (mes[0] == "login")
                    {
                        try
                        {
                            DataHolder.MyServerID = Convert.ToInt32(mes[1]);
                            DataHolder.Money = Convert.ToInt32(mes[2]);                           
                        }
                        catch { DataHolder.Connected = false; }

                        DataHolder.MessageTCP.RemoveAt(0);
                        break;
                    }
                }
            }
            else
            {
                DataHolder.Connected = false;
                break;
            }
        }
    }

    /// <summary>
    /// Функция реконнекта. Блокировка кнопок на экране, показ всех нужных уведомлений, проверка сети и запуск цикла запросов на повторное соединение с сервером.
    /// </summary>
    public async void StartReconnect()
    {
        DataHolder.Connected = false;
        CleanTcpConnection();
        ShowNotif("Разрыв соединения.\r\nПереподключение...", 2);

        while (TryRecconect)
        {
            if (!await Task.Run(() => CheckForInternetConnection()))
            {
                ShowNotif("Разрыв соединения.\r\nОтсутствует подключение к интернету.\r\nОжидание...", 2);
                continue;
            }
            else ShowNotif("Разрыв соединения.\r\nПодключение к серверу...", 2);

            await Task.Run(() => DataHolder.ClientTCP = new TcpConnect());
            DataHolder.ClientTCP.GetMessage += SOS;

            if (!DataHolder.Connected)
            {
                CleanTcpConnection();
                continue;
            }

            ShowNotif("Разрыв соединения.\r\nОжидание ответа сервера", 2);
            await Task.Run(() => LoginInServerSystem());

            if (!DataHolder.Connected)
            {
                CleanTcpConnection();
                continue;
            }
               
            // При полном успехе
            NotificatonMultyButton(10);
            DataHolder.ClientTCP.CanStartReconnect = true;
        }
        TryRecconect = true;
    }

    /// <summary>
    /// Функция закрытия всех типов уведомлений NotifPanel с последующей обработкой.
    /// </summary>
    /// <param name="num">Тип уведомления.</param>
    public void NotificatonMultyButton(int num)
    {       
        switch (num)
        {
            case 1: // ExitSimpleNotif       
                NotifButton.SetActive(false);               
                break;

            case 2: // StopReconnect                
                CleanTcpConnection();
                TryRecconect = false;
                StopReconnectButton.SetActive(false);
                SceneManager.LoadScene("mainMenu"); // Ну если не хочешь reconnect во время игры, то не играй))
                //TODO: Ну тогда надо ещё корректно завершить игру и закрыть UDP соединение.
                break;

            case 3: // CancelGameSearch
                DataHolder.ClientTCP.SendMessage("CancelSearch");
                CancelSearchButton.SetActive(false);
                break;

            case 4: // ExitPresentGame
                CloseEndGameButton.SetActive(false);
                SceneManager.LoadScene("mainMenu");
                break;

            case 10: // Правильный выход из StartReconnect
                TryRecconect = false;
                StopReconnectButton.SetActive(false);
                break;
        }
        NotifPanel.SetActive(false);
        Shield.SetActive(false);
    }

    /// <summary>
    /// Выводит на экран уведомление и отключает все остальные кнопки.
    /// </summary>
    /// <param name="notif">Текст уведомления.</param>
    /// <param name="caseNotif">Выбор типа кнопки и самого уведомления на окне.</param>
    public void ShowNotif(string notif, int caseNotif) //TODO: А если будет несколько уведомлений по очереди, надо сделать очередь.
    {
        Shield.SetActive(true); //TODO: При переходе между сценами связь между ссылками временно теряется и вылетает ошибка

        NotifButton.SetActive(false);
        StopReconnectButton.SetActive(false);
        CancelSearchButton.SetActive(false);
        CloseEndGameButton.SetActive(false);
             
        if (caseNotif == 1)
            NotifButton.SetActive(true);
        else if (caseNotif == 2)
            StopReconnectButton.SetActive(true);
        else if (caseNotif == 3)
            CancelSearchButton.SetActive(true);
        else if (caseNotif == 4)
            CloseEndGameButton.SetActive(true);

        NotifPanel.transform.Find("Text").GetComponent<Text>().text = notif;
        NotifPanel.SetActive(true);
    }

    /// <summary>
    /// Проверка соединения с интернетом, путём открытия гугловской страницы.
    /// </summary>
    /// <returns>True, если выход в интернет есть.</returns>
    private bool CheckForInternetConnection()
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
    private void CleanTcpConnection()
    {
        if (DataHolder.ClientTCP != null)
        {
            DataHolder.ClientTCP.GetMessage -= SOS;
            DataHolder.ClientTCP.CloseClient();
            DataHolder.ClientTCP = null;
        }
    }

    /// <summary>
    /// Получение времени из интернета
    /// Взял отсюда https://qna.habr.com/q/491141
    /// </summary>
    /// <returns></returns>
    public static DateTime GetNetworkTime()
    {
        const string ntpServer = "pool.ntp.org";
        // NTP message size - 16 bytes of the digest (RFC 2030)
        var ntpData = new byte[48];

        //Setting the Leap Indicator, Version Number and Mode values
        ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

        var addresses = Dns.GetHostEntry(ntpServer).AddressList;

        //The UDP port number assigned to NTP is 123
        var ipEndPoint = new IPEndPoint(addresses[0], 123);
        //NTP uses UDP
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Connect(ipEndPoint);

            //Stops code hang if NTP is blocked
            socket.ReceiveTimeout = 3000;

            socket.Send(ntpData);
            socket.Receive(ntpData);
        }

        //Offset to get to the "Transmit Timestamp" field (time at which the reply 
        //departed the server for the client, in 64-bit timestamp format."
        const byte serverReplyTime = 40;

        //Get the seconds part
        ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

        //Get the seconds fraction
        ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

        //Convert From big-endian to little-endian
        intPart = SwapEndianness(intPart);
        fractPart = SwapEndianness(fractPart);

        var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

        //**UTC** time
        var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

        return networkDateTime;
        //return networkDateTime.ToLocalTime();
    }

    private static uint SwapEndianness(ulong x)
    {
        return (uint)(((x & 0x000000ff) << 24) +
                       ((x & 0x0000ff00) << 8) +
                       ((x & 0x00ff0000) >> 8) +
                       ((x & 0xff000000) >> 24));
    }

    public void SOS()
    {
        Debug.Log("sosi");
    }
}
