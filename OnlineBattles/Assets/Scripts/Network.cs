using System;
using System.Net;
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
        if (DataHolder.MessageTCP.Count > 0)
        {
            string[] mes = DataHolder.MessageTCP[0].Split(' ');
            Debug.Log($"net {DataHolder.MessageTCP[0]}");
            switch (mes[0])
            {
                case "win":
                case "lose":
                case "drawn":
                    //TODO: Нужен какой-то мультивыбор скрипта ниже, а не только этот
                    GetComponent<UDPGame>().CloseAll();
                    ShowNotif("Игра завершена\r\n" + mes[0], 4);                    
                    break;

                case "login":
                    return;

                case "ping":
                    DataHolder.ClientTCP.SendMassage("ping");
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
        //TODO: Добавить стандартные команды от сервера типо закончить и тд
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

        if (DataHolder.Connected == false)
        {
            CleanTcpConnection();
            ShowNotif("Сервер не доступен. Попробуйте позже.", 1);
            return;
        }

        await Task.Run(() => LoginInServerSystem());

        if (DataHolder.Connected == false)
        {
            CleanTcpConnection();
            ShowNotif("Ошибка доступа к серверу.", 1);
            return;
        }
        else GetComponent<MainMenuScr>().GoToMultiplayerMenu();
    }

    /// <summary>
    /// Отправка запроса на авторизацию в системе сервера и ожидание подтверждения (получение id и money).
    /// </summary>
    private void LoginInServerSystem()
    {
        DataHolder.ClientTCP.SendMassage("login " + DataHolder.KeyCodeName);

        DateTime StartTryConnect = DateTime.Now;
        //TODO: При этом, пока телефон думает, пусть в углу будет гифка загрузки, чтоб пользователь понимал, что что-то происходит
        //TODO: Можно вызывать это не бесконечно, а раз в пол секунды
        while (true)
        {
            if (((DateTime.Now - StartTryConnect).TotalSeconds < TimeForWaitAnswer))
            {
                // Получаем id и деньги от сервера
                if (DataHolder.MessageTCP.Count > 0) //TODO: Возможно в это время удалить у всех остальных Update ловить сообщения
                {
                    string[] mes = DataHolder.MessageTCP[0].Split(' ');
                    if (mes[0] == "login")
                    {
                        try
                        {
                            DataHolder.MyServerID = Convert.ToInt32(mes[1]);
                            DataHolder.Money = Convert.ToInt32(mes[2]);                           
                        }
                        catch
                        {
                            //TODO: Ошибка базы данных
                            DataHolder.Connected = false;                           
                        }
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

            if (DataHolder.Connected == false)
            {
                CleanTcpConnection();
                continue;
            }

            ShowNotif("Разрыв соединения.\r\nОжидание ответа сервера", 2);
            await Task.Run(() => LoginInServerSystem());

            if (DataHolder.Connected == false)
            {
                CleanTcpConnection();
                continue;
            }
               
            // При полном успехе
            NotificatonMultyButton(10);
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
                DataHolder.ClientTCP.SendMassage("CancelSearch");
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
        Shield.SetActive(true);

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
    private void CleanTcpConnection()
    {
        if (DataHolder.ClientTCP != null)
        {
            DataHolder.ClientTCP.CloseClient();
            DataHolder.ClientTCP = null;
        }
    }
}
