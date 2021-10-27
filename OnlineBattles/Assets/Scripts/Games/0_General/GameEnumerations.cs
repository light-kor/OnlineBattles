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

    public enum ControlTypes
    {
        Local,
        Remote,
        Broadcast
    }

    public enum GameTypes
    {
        Null,
        Single,
        WifiHost,
        WifiClient,
        Multiplayer
    }

    public enum ConnectTypes
    {
        TCP,
        UDP
    }
}
