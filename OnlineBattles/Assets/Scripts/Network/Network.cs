﻿using System;
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
                        NotificationPanels.NP.CloseSimple(); // Выключаем панель ожидания
                        NotificationPanels.NP.AddNotificationToQueue("Запрос отклонён", 1);
                        WifiServerAnswer?.Invoke("denied");
                        break;

                    case "accept":
                        NotificationPanels.NP.CloseSimple(); // Выключаем панель ожидания
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
        NotificationPanels.NP.AddNotificationToQueue("Ожидание подключения", 0);

        TcpConnectionProcess(1);

        if (DataHolder.Connected)
        {
            TcpConnectionIsDone?.Invoke();
            if (DataHolder.ClientTCP != null)
            {
                DataHolder.ClientTCP.CanStartReconnect = true; //TODO: Чёт здесь какая-то фигня
            }
        }
    }

    static async void TcpConnectionProcess(int num)
    {
        CloseTcpConnection();

        if (DataHolder.GameType == "Multiplayer")
        {
            if (!await Task.Run(() => CheckForInternetConnection()))
            {
                NotificationPanels.NP.AddNotificationToQueue("Отсутствует подключение к интернету.", num);
                return;
            }
        }

        // Асинхронность нужна чтоб сначала показать уведомление о начале подключения, а потом уже подключать. 
        await Task.Run(() => DataHolder.ClientTCP = new TCPConnect());

        if (!DataHolder.Connected)
        {
            CloseTcpConnection();
            NotificationPanels.NP.AddNotificationToQueue("Сервер не доступен.", num);
            return;
        }

        NotificationPanels.NP.AddNotificationToQueue("Ожидание ответа сервера", 0);
        await Task.Run(() => LoginInServerSystem());

        if (!DataHolder.Connected)
        {
            CloseTcpConnection();
            NotificationPanels.NP.AddNotificationToQueue("Ошибка доступа к серверу.", num);
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
    public static void StartReconnect()
    {
        DataHolder.Connected = false;
        NotificationPanels.NP.AddNotificationToQueue("Разрыв соединения.\r\nПереподключение...", 2);

        while (TryRecconect)
        {
            TcpConnectionProcess(2);
            if (DataHolder.Connected)
            {
                // При полном успехе
                NotificationPanels.NP.NotificatonMultyButton(10);
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

    //TODO: Очистка всего, связанного  с соединениями, и тем, что создано в этом скрипте
    private static void Clear()
    {

    }

}