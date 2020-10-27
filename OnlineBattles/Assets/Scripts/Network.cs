using System;
using System.Net;
using UnityEngine;

public class Network : MonoBehaviour
{
    const float TimeForWaitAnswer = 3f;

    public GameObject NotifPanel, Shield;

    void Start()
    {
        
    }

    void Update()
    {
        // Если Read поймёт, что сеть прервалось, сработает это и начнётся востановление сети.
        if (DataHolder.NeedToReconnect)
        {
            DataHolder.NeedToReconnect = false;
            StartReconnect();
        }

        //TODO: Добавить стандартные команды от сервера типо закончить и тд
    }

    public void StopTryConnectToServer()
    {

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
    public void CreateTCP()
    {
        //TODO: Добавить анимацию загрузки, что было понятно, что надо подождать
        Shield.SetActive(true);

        if (!CheckForInternetConnection())
        {
            DataHolder.ShowNotif(NotifPanel, "Отсутствует подключение к интернету.");
            Shield.SetActive(false);
            return;
        }

        DataHolder.ClientTCP = new TcpConnect();

        if (DataHolder.Connected == false)
        {
            Shield.SetActive(false);
            return;
        }
            
        LoginInServerSystem();
        Shield.SetActive(false);
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
    private void StartReconnect()
    {
        Shield.SetActive(true);
        DataHolder.ShowNotif(NotifPanel, "Разрыв соединения.\r\nПереподключение..."); //TODO: Тут везде нужна новая панель ожидания подключения

        // потом уже налаживаем соединение с сервером
        InvokeRepeating("TryReconnect", 0.0f, 1.0f);
    }

    /// <summary>
    /// Повторяющаяся функция попыток успешного соединения
    /// </summary>
    private void TryReconnect()
    {
        // Сначала ченем инет
        if (!CheckForInternetConnection())
        {
            DataHolder.ShowNotif(NotifPanel, "Отсутствует подключение к интернету.\r\nОжидание..."); //TODO: Тут везде нужна новая панель ожидания подключения
            return;
        }
        else DataHolder.ShowNotif(NotifPanel, "Подключение к серверу..."); //TODO: Тут везде нужна новая панель ожидания подключения

        DataHolder.ClientTCP.TryConnect();       

        if (DataHolder.Connected == true)
        {
            CancelInvoke("TryReconnect");

            //DataHolder.ShowNotif(NotifPanel, ?); Написать типо: "Ожидание ответа от сервера"
            LoginInServerSystem();

            Shield.SetActive(false);
            NotifPanel.SetActive(false);
        }
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
