using GameEnumerations;
using System;
using System.Collections.Generic;

public static class DataHolder
{   
    public static GameTypes GameType { get; set; } = GameTypes.Null;
    public static int SelectedServerGame { get; set; } = -1;           
    public static int MyIDInServerSystem { get; set; }  = -1;   
    public static int LobbyID { get; set; } = -1;   
    public static int IDInThisGame { get; set; } = -1;
    public static int Money { get; set; } = -1;  

    // Settings
    public static string NickName { get; set; } = null;
    public static float Volume { get; set; } = 1f;
    //

    //public delegate void Notification();
    //public delegate void StringEvent(string text);
    //public delegate void StringArrayMessage(string[] message);
    //public delegate void Pause(PauseTypes pauseType);

    //"127.0.0.1" - локальный; 188.134.87.78 - общий дом
    public static string ServerIp { get; } = "188.134.87.78";
    public static string WifiGameIp { get; set; } = null;
    public static int RemoteServerPort { get; } = 55555;
    public static int WifiPort { get; } = 55550;

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static string[] UseAndDeleteFirstListMessage(List<string[]> list)
    {
        string[] message = list[0];
        list.RemoveAt(0);
        return message;
    }
}
