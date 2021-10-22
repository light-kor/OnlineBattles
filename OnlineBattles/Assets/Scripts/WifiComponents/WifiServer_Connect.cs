using System.Collections.Generic;

public static class WifiServer_Connect
{
    public static event DataHolder.StringEvent AddWifiServerToScreen;
    private static List<string> _wifiServers = new List<string>();

    public static void StartSearching()
    {
        WifiServer_Searcher.GetWifiServer += AddServerToList;
        _wifiServers.Clear();
        Network.CreateWifiServerSearcher("receiving");
    }

    public static void ConnectToWifiServer()
    {
        Network.CloseWifiServerSearcher();
        WifiServer_Searcher.GetWifiServer -= AddServerToList;
        Network.CreateTCP();
    }

    private static void AddServerToList()
    {
        while (Network.UDPMessages.Count > 0)
        {
            string[] mes = Network.UDPMessages[0].Split(' ');
            if (mes[0] == "server")
            {                
                if (_wifiServers.Find(x => x == mes[2]) == null)
                {
                    _wifiServers.Add(mes[2]);
                    AddWifiServerToScreen?.Invoke($"{mes[1]} {mes[2]}");                    
                }
            }
            Network.UDPMessages.RemoveAt(0);
        }
    }
}
