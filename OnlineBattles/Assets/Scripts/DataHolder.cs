using System;
using System.Collections.Generic;
using UnityEngine;

public static class DataHolder
{
    public static List<string> MessageTCP { get; set; } = new List<string>();
    public static List<string> MessageTCPforGame { get; set; } = new List<string>();
    public static List<string> MessageUDPget { get; set; } = new List<string>();
    public static NotificationPanels NotifPanels { get; set; } = null;
    public static TcpConnect ClientTCP { get; set; } = null;
    public static UDPConnect ClientUDP { get; set; } = null;
    public static GameObject TimerT { get; set; } = null;
    public static DateTime LastSend { get; set; } = DateTime.UtcNow;
    public static string KeyCodeName { get; set; } = "123";
    public static string NickName { get; set; } = "Mihuil";
    public static bool Connected { get; set; } = false;
    public static bool CanMove { get; set; } = false;
    public static long TimeDifferenceWithServer { get; set; }
    public static int GameType { get; set; } = -1;
    public static int SelectedServerGame { get; set; } = -1;           
    public static int MyServerID { get; set; }  = -1;   
    public static int LobbyID { get; set; } = -1;   
    public static int IDInThisGame { get; set; } = -1;
    public static int Money { get; set; } = -1;
    public static int WinFlag { get; set; } = -1;

    public delegate void Notification();
    public delegate void GameNotification(string text, int number);

    //"127.0.0.1" - локальный; 188.134.87.78 - общий дом
    public static string ServerIp { get; } = "188.134.87.78";
    public static string WifiGameIp { get; } = null;
    public static int RemotePort { get; } = 55555;

}
