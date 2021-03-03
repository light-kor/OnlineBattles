using System.Collections.Generic;

public static class WifiServer_Connect
{
    public static event DataHolder.Text≈ransmissionEnvent AddWifiServerToScreen;
    private static List<string> WifiServers = new List<string>();

    public static void StartSearching()
    {
        WifiServer_Searcher.GetWifiServer += AddServerToList;
        WifiServers.Clear();
        Network.CreateWifiServerSearcher("receiving");
    }

    public static void ConnectToWifiServer()
    {
        Network.CloseWifiServerSearcher();
        WifiServer_Searcher.GetWifiServer -= AddServerToList;

        if (DataHolder.Connected)
            Network.CloseTcpConnection();

            Network.CreateTCP();
    }

    private static void AddServerToList()
    {
        while (DataHolder.MessageUDPget.Count > 0)
        {
            string[] mes = DataHolder.MessageUDPget[0].Split(' ');
            if (mes[0] == "server")
            {                
                if (WifiServers.Find(x => x == mes[2]) == null)
                {
                    WifiServers.Add(mes[2]);
                    AddWifiServerToScreen?.Invoke($"{mes[1]} {mes[2]}");                    
                }
            }
            DataHolder.MessageUDPget.RemoveAt(0);
        }
    }
}
