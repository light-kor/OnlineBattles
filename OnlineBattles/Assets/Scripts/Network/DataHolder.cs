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

    public delegate void Notification();
    public delegate void TextЕransmissionEnvent(string text);

    //"127.0.0.1" - локальный; 188.134.87.78 - общий дом
    public static string ServerIp { get; } = "188.134.87.78";
    public static string WifiGameIp { get; set; } = null;
    public static int RemoteServerPort { get; } = 55555;
    public static int WifiPort { get; } = 55550;    

    public enum GameTypes
    {
        Null,
        Single,
        WifiHost,
        WifiClient,
        Multiplayer
    }

    public enum ConnectType
    {
        TCP,
        UDP
    }
}
