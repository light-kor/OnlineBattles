using System.Collections.Generic;
using UnityEngine.Events;

public static class WifiServer_Connect
{
    public static event UnityAction<string> AddWifiServerToScreen;
    private static List<string> _wifiServers = new List<string>();

    public static void StartSearching()
    {
        WifiServer_Searcher.GetMessage += AddServerToList;
        _wifiServers.Clear();
        Network.CreateWifiServerSearcher("receiving");
    }

    public static void ConnectToWifiServer()
    {
        Network.CloseWifiServerSearcher();
        WifiServer_Searcher.GetMessage -= AddServerToList;
        Network.CreateTCP();
    }

    private static void AddServerToList(string message)
    {
        string[] mes = message.Split(' ');
        if (mes[0] == "server")
        {
            if (_wifiServers.Find(x => x == mes[2]) == null)
            {
                _wifiServers.Add(mes[2]);
                AddWifiServerToScreen?.Invoke($"{mes[1]} {mes[2]}");
            }
        }
    }
}
