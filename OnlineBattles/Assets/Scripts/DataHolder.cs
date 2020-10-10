using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using UnityEngine;

public static class DataHolder
{
    public static int GameType { get; set; } = 0;
    public static bool Connected { get; set; } = false;
    public static int MyID { get; set; }  = -1;
    public static int Money { get; set; } = -1;
    public static string KeyID { get; set; } = "123";
    public static int thisGameID { get; set; }
    public static string notifText { get; set; } = "";
    public static bool inGame { get; set; } = false;
    public static bool showNotif { get; set; } = false;
    public static bool ButtonMainMenu { get; set; } = false;
    public static bool canMove { get; set; } = false;
    public static int WinFlag { get; set; } = 0;
    public static int thisGameId { get; set; } = 0;

    public static GameObject timerT { get; set; }
    public static TcpConnect ClientTCP { get; set; }
    public static UDPConnect ClientUDP { get; set; }

    public static List<string> messageTCP = new List<string>();
    public static List<string> messageTCPforGame = new List<string>();

    public static List<string> messageUDPget = new List<string>();

    //Временные переменные
    public static int Port = 13130; // локальный порт для прослушивания входящих подключений
    public static int remotePort = 55555; // порт для отправки данных

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
                            Connected = true;
                            ButtonMainMenu = true;
                            break;
                        }
                        else
                        {
                            //TODO: Ошибка базы данных
                            break;
                        }
                    }
                }
            }
            else
            {
                ClientTCP = null;
                ButtonMainMenu = true;
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
        //TODO: проверить работу, когда вифи работает, но провод не вставлен
        try
        {
            IPStatus  status = new System.Net.NetworkInformation.Ping().Send("google.com").Status;

            if (status == IPStatus.Success)
                return true;
            else return false;
        }
        catch { return false; }
        
    }
}
