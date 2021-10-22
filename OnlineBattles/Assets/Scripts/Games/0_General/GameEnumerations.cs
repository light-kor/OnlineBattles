namespace GameEnumerations
{
    public enum PlayerTypes
    {
        Null,
        BluePlayer,
        RedPlayer
    }

    public enum PauseTypes
    {
        Null,
        ManualPause,
        EndRound,
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
