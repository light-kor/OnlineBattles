using System.Collections.Generic;
using UnityEngine;

public static class DataHolder
{
    public static int GameType { get; set; } = -1;
    public static bool Connected { get; set; } = false;
    public static int MyID { get; set; }  = -1;
    public static int Money { get; set; } = -1;
    public static int GameId { get; set; } = -1;
    public static string KeyID { get; set; } = "123";
    public static int ThisGameID { get; set; } = -1;
    public static bool NeedToReconnect { get; set; } = false;
    public static bool CanMove { get; set; } = false;
    public static int WinFlag { get; set; } = -1;
    public static GameObject TimerT { get; set; }
    public static TcpConnect ClientTCP { get; set; }
    public static UDPConnect ClientUDP { get; set; }

    public static List<string> MessageTCP { get; set; } = new List<string>();
    public static List<string> MessageTCPforGame { get; set; } = new List<string>();
    public static List<string> MessageUDPget { get; set; } = new List<string>();

    //"127.0.0.1" - локальный; 188.134.87.78 - общий дом
    public static string ConnectIp { get; set; } = "188.134.87.78";
    public static int RemotePort { get; set; } = 55555;

    // Все варианты оповещений игрока для NotifPanel
    //TODO: Сделать NotifPanel переходящей из сцены в сцену
    public static string[] NotifOptions = new string[6] { 
        "Сервер не доступен. Попробуйте позже.",
        "Разрыв соединения.\r\nПереподключение...",
        "Отсутствует подключение к интернету.",
        "Отсутствует подключение к интернету.\r\nОжидание...",
        "Переподключение к серверу...",
        "Ожидание сети..."
    };   
   
}
