namespace GameEnumerations
{
    public enum PlayerTypes
    {
        Null,
        BluePlayer,
        RedPlayer,
        Both
    }

    public enum PauseTypes
    {
        Null,
        ManualPause,       
        RemotePause, 
        BackgroundPause
    }

    public enum GameResults
    {
        Null,
        Win,
        Lose,
        Draw,
    }

    public enum GameTypes
    {
        Null,
        Local,
        WifiHost,
        WifiClient,
        Multiplayer
    }

    public enum ConnectTypes
    {
        TCP,
        UDP
    }

    public enum BackButtonTypes
    {
        Disconnect,
        Cancel,
        Back
    }
}