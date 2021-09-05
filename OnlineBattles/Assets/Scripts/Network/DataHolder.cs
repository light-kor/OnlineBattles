using System;
using System.Collections.Generic;

public static class DataHolder
{
    public static List<string> MessageTCP { get; set; } = new List<string>();
    public static List<string> MessageTCPforGame { get; set; } = new List<string>();
    public static List<string> MessageUDPget { get; set; } = new List<string>();
    public static List<byte> BigArray { get; set; } = new List<byte>();
    public static TCPConnect ClientTCP { get; set; } = null;
    public static UDPConnect ClientUDP { get; set; } = null;
    public static WifiServer_Searcher ServerSearcher { get; set; } = null;
    public static DateTime LastSend { get; set; } = DateTime.UtcNow;
    public static string KeyCodeName { get; set; } = "123";
    public static string StartMenuView { get; set; } = null;
    public static bool Connected { get; set; } = false;
    public static long TimeDifferenceWithServer { get; set; }
    public static string GameType { get; set; } = null;
    public static int SelectedServerGame { get; set; } = -1;           
    public static int MyIDInServerSystem { get; set; }  = -1;   
    public static int LobbyID { get; set; } = -1;   
    public static int IDInThisGame { get; set; } = -1;
    public static int Money { get; set; } = -1;

    // Settings
    public static string NickName { get; set; } = null;
    public static float Volume { get; set; } = 1f;
    //

    public delegate void Notification();
    public delegate void TextЕransmissionEnvent(string text);

    //"127.0.0.1" - локальный; 188.134.87.78 - общий дом
    public static string ServerIp { get; } = "188.134.87.78";
    public static string WifiGameIp { get; set; } = null;
    public static int RemoteServerPort { get; } = 55555;
    public static int WifiPort { get; } = 55550;
    public static string ServerName { get; } = "Kate";

}
