using System;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Network : MonoBehaviour
{
    private const float TimeForWaitAnswer = 3f;
    private bool TryRecconect = true;

    public GameObject NotifPanel, NotifButton, StopReconnectButton, CancelSearchButton, CloseEndGameButton;
    public GameObject Shield; // Блокирует нажатия на все кнопки, кроме notifPanel

    private void Start()
    {
        
    }

    private void Update()
    {
        // Если Reader в TcpConnect поймёт, что сеть прервалась, то сработает это и начнётся востановление сети.
        if (DataHolder.NeedToReconnect)
        {
            DataHolder.NeedToReconnect = false;
            StartReconnect();
        }

        if (DataHolder.MessageTCP.Count > 0)
        {
            string[] mes = DataHolder.MessageTCP[0].Split(' ');

            switch (mes[0])
            {
                case "win":
                case "lose":
                case "drawn":
                    //TODO: Нужен какой-то мультивыбор скрипта ниже, а не только этот
                    GetComponent<Joystic_controller>().CloseAll();
                    ShowNotif("Игра завершена\r\n" + mes[0], 4);
                    break;
            }



            DataHolder.MessageTCP.RemoveAt(0);
        }

        //TODO: Добавить стандартные команды от сервера типо закончить и тд
    }

    

    public void CreateUDP()
    {
        DataHolder.ClientUDP = new UDPConnect();
    }

    /// <summary>
    /// Проверка интернета, создание экземпляра TcpConnect и авторизация в системе сесрвера
    /// </summary>
    public async void CreateTCP()
    {
        //TODO: Добавить анимацию загрузки, что было понятно, что надо подождать
        ShowNotif("Ожидание подключения", 0);

        if (!CheckForInternetConnection())
        {
            ShowNotif("Отсутствует подключение к интернету.", 1);           
            return;
        }

        // Асинхронность нужна чтоб сначала показать уведомление о начале подключения, а потом уже подключать. 
        await Task.Run(() => DataHolder.ClientTCP = new TcpConnect());

        if (DataHolder.Connected == false)
        {
            ShowNotif("Сервер не доступен. Попробуйте позже.", 1);
            return;
        }

        LoginInServerSystem();

        if (DataHolder.Connected == false)
        {
            ShowNotif("Ошибка доступа к серверу.", 1);
            return;
        }
        else GetComponent<MainMenuScr>().GoToMulty();
    }

    /// <summary>
    /// Отправка запроса на авторизацию и ожидание подтверждения, id и money
    /// </summary>
    private void LoginInServerSystem()
    {
        DataHolder.ClientTCP.SendMassage("login " + DataHolder.KeyID);

        DateTime d = DateTime.Now;
        // Просто пусть будет несколько сек, вдруг сервер тупит
        //TODO: При этом, пока телефон думает, пусть в углу будет гифка загрузки, чтоб пользователь понимал, что что-то происходит
        //TODO: Можно вызывть это не бесконечно, а раз в пол секунды
        while (true)
        {
            if (((DateTime.Now - d).TotalSeconds < TimeForWaitAnswer))
            {
                // Получаем id и деньги от сервера
                if (DataHolder.MessageTCP.Count > 0)
                {
                    string[] mes = DataHolder.MessageTCP[0].Split(' ');
                    if (mes[0] == "0")
                    {
                        try
                        {
                            DataHolder.MyID = Convert.ToInt32(mes[1]);
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
                    else DataHolder.MessageTCP.RemoveAt(0);
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
    /// Начало реконнекта, показ всех нужных уведомлений, проверка сети и запуск цикла запросов повторное на соединение
    /// </summary>
    private async void StartReconnect()
    {
        ShowNotif("Разрыв соединения.\r\nПереподключение...", 2);
        //TODO: Перед каждой новой попыткой очистетить все старые соединения

        while (TryRecconect)
        {
            // Сначала ченем инет
            if (!await Task.Run(() => CheckForInternetConnection()))
            {
                ShowNotif("Разрыв соединения.\r\nОтсутствует подключение к интернету.\r\nОжидание...", 2);
                continue;
            }
            else ShowNotif("Разрыв соединения.\r\nПодключение к серверу...", 2);

            await Task.Run(() => DataHolder.ClientTCP.TryConnect());    

            if (DataHolder.Connected == true)
            {
                ShowNotif("Разрыв соединения.\r\nОжидание ответа сервера", 2);
                await Task.Run(() => LoginInServerSystem()); //TODO: А если не получится?
                StopReconnect();
            }
        }
        TryRecconect = true; // Возвращаем true в переменную 
    }

    public void ExitNotif()
    {
        NotifPanel.SetActive(false);
        NotifButton.SetActive(false);
        Shield.SetActive(false);
    }

    public void ExitGame()
    {
        NotifPanel.SetActive(false);
        CloseEndGameButton.SetActive(false);
        Shield.SetActive(false);
        SceneManager.LoadScene("mainMenu");
    }

    public void StopReconnect()
    {
        TryRecconect = false;
        NotifPanel.SetActive(false);
        StopReconnectButton.SetActive(false);
        Shield.SetActive(false);
    }

    public void CancelGameSearch() //TODO: Не забудь обработать отмену на сервере
    {
        DataHolder.ClientTCP.SendMassage("CancelSearch");
        NotifPanel.SetActive(false);
        CancelSearchButton.SetActive(false);
        Shield.SetActive(false);
    }

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
    /// Выводит на экран уведомление и отключает все остальные кнопки.
    /// </summary>
    /// <param name="num">Текст ведомления</param>
    /// <param name="caseNotif">Выбор типа и кнопки на окне уведомления</param>
    public void ShowNotif(string notif, int caseNotif)
    {
        Shield.SetActive(true);
        NotifPanel.transform.Find("Text").GetComponent<Text>().text = notif;
        NotifPanel.SetActive(true);

        if (caseNotif == 1)
            NotifButton.SetActive(true);
        else if (caseNotif == 2)
            StopReconnectButton.SetActive(true);
        else if (caseNotif == 3)
            CancelSearchButton.SetActive(true);
        else if (caseNotif == 4)
            CloseEndGameButton.SetActive(true);

    }

}
