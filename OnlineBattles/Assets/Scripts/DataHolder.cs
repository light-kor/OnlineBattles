using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

public static class DataHolder
{
    public static int GameType { get; set; } = 0;
    public static bool Connected { get; set; } = false;
    public static int MyID { get; set; }  = -1;
    public static int Money { get; set; } = -1;
    public static string KeyID { get; set; } = "123";
    public static int thisGameID { get; set; }
    public static bool needToReconnect { get; set; } = false;
    public static bool canMove { get; set; } = false;
    public static int WinFlag { get; set; } = 0;
    public static GameObject timerT { get; set; }
    public static TcpConnect ClientTCP { get; set; }
    public static UDPConnect ClientUDP { get; set; }

    public static List<string> messageTCP = new List<string>();
    public static List<string> messageTCPforGame = new List<string>();

    public static List<string> messageUDPget = new List<string>();

    //Временные переменные
    public static int localPort = 13130; // локальный порт для прослушивания входящих подключений
    public static int remotePort = 55555; // порт для отправки данных

    // Все варианты оповещений игрока для NotifPanel
    //TODO: Сделать NotifPanel переходящей из сцены в сцену
    public static string[] notifOptions = new string[6] { 
        "Сервер не доступен. Попробуйте позже.",
        "Разрыв соединения.\r\nПереподключение...",
        "Отсутствует подключение к интернету.",
        "Отсутствует подключение к интернету.\r\nОжидание...",
        "Переподключение к серверу...",
        "Ожидание сети..."
    };



    // Надо ли делать это в отдельном потоке?
    // потом акрыть поток
    /// <summary>
    /// Функция установки TCP соединения и получения денег, id взамен на keyid
    /// </summary>
    public static void CreateTCP()
    {
        // Если это не первая попытка (Это нельзя удалять!)
        if (ClientTCP != null)
            ClientTCP = null;

        ClientTCP = new TcpConnect();

        if (Connected == false)
        {
            ClientTCP = null;
            return;
        }
            

        ClientTCP.SendMassage("0 1 " + KeyID);

        DateTime d = DateTime.Now;
        // Просто пусть будет несколько сек, вдруг сервер тупит
        //TODO: При этом, пока телефон думает, пусть в углу будет гифка загрузки, чтоб пользователь понимал, что что-то происходит
        //TODO: Можно вызывть это не бесконечно, а раз в пол секунды
        while (true)
        {
            if (((DateTime.Now - d).TotalSeconds < 2))
            {
                // Получаем id и деньги от сервера
                if (messageTCP.Count > 0)
                {
                    string[] mes = messageTCP[0].Split(' ');
                    if (mes[0] == "0")
                    {
                        if (mes[1] != "" && mes[2] != "")
                        {
                            MyID = Convert.ToInt32(mes[1]);
                            Money = Convert.ToInt32(mes[2]);

                            messageTCP.RemoveAt(0);
                            break;
                        }
                        else
                        {
                            //TODO: Ошибка базы данных
                            ClientTCP = null;
                            Connected = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                ClientTCP = null;
                Connected = false;
                break;
            }
        }
    }

    public static void CreateUDP()
    {
        ClientUDP = new UDPConnect();
    }

    public static bool CheckConnection()
    {
        try
        {
            IPStatus status = new System.Net.NetworkInformation.Ping().Send("google.com").Status;

            if (status == IPStatus.Success)
                return true;
            else return false;
        }
        catch { return false; }
        
    }


    /// <summary>
    /// Выводит на экран уведомление об ошибке и тд.
    /// </summary>
    /// <param name="notifPanel">Ссылка на панель уведомлений</param>
    /// <param name="num">Номер уведомления в notifOptions</param>
    public static void ShowNotif(GameObject notifPanel, int num)
    {
        notifPanel.transform.Find("Text").GetComponent<Text>().text = notifOptions[num];
        notifPanel.SetActive(true);
    }

}
