using System;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class Network : MonoBehaviour
{
    private const float TimeForWaitAnswer = 3f;
    private bool TryRecconect = true;

    public GameObject NotifPanel, NotifButton, StopReconnectButton;
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

        //TODO: Добавить стандартные команды от сервера типо закончить и тд
    }

    public void StopWaitingAcceptForGameFromServer()
    {

    }

    public void CreateUDP()
    {
        DataHolder.ClientUDP = new UDPConnect();
    }

    /// <summary>
    /// Проверка интернета, создание экземпляра TcpConnect и авторизация в системе сесрвера
    /// </summary>
    public async void CreateTCP() //TODO: Может следует вызвать асинхронно, чтоб всё не зависло. А то пока всё не обработается, фрейм не пройдёт
    {
        //TODO: Добавить анимацию загрузки, что было понятно, что надо подождать
        DataHolder.ShowNotif(NotifPanel, Shield, "Ожидание подключения");

        if (!CheckForInternetConnection())
        {
            DataHolder.ShowNotif(NotifPanel, Shield, "Отсутствует подключение к интернету.");
            NotifButton.SetActive(true);
            return;
        }

        // Это нужно чтоб сначала показать уведомление о начале подключения, а потом уже подключать. 
        await Task.Run(() => DataHolder.ClientTCP = new TcpConnect());

        if (DataHolder.Connected == false)
        {
            DataHolder.ShowNotif(NotifPanel, Shield, "Сервер не доступен. Попробуйте позже.");
            NotifButton.SetActive(true);
            return;
        }

        LoginInServerSystem();

        if (DataHolder.Connected == false)
        {
            DataHolder.ShowNotif(NotifPanel, Shield, "Ошибка доступа к серверу.");
            NotifButton.SetActive(true);
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
        DataHolder.ShowNotif(NotifPanel, Shield, "Разрыв соединения.\r\nПереподключение...");
        StopReconnectButton.SetActive(true);

        while (TryRecconect)
        {
            // Сначала ченем инет
            if (!await Task.Run(() => CheckForInternetConnection()))
            {
                DataHolder.ShowNotif(NotifPanel, Shield, "Разрыв соединения.\r\nОтсутствует подключение к интернету.\r\nОжидание..."); //TODO: Тут везде нужна новая панель ожидания подключения
                continue;
            }
            else DataHolder.ShowNotif(NotifPanel, Shield, "Разрыв соединения.\r\nПодключение к серверу..."); //TODO: Тут везде нужна новая панель ожидания подключения

            await Task.Run(() => DataHolder.ClientTCP.TryConnect());    

            if (DataHolder.Connected == true)
            {
                DataHolder.ShowNotif(NotifPanel, Shield, "Разрыв соединения.\r\nОжидание ответа сервера");
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

    public void StopReconnect()
    {
        TryRecconect = false;
        NotifPanel.SetActive(false);
        StopReconnectButton.SetActive(false);
        Shield.SetActive(false);
    }

    //TODO: Один раз игра тупо зависла, когда не было интернета. Опоещение даже не показала
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

}
