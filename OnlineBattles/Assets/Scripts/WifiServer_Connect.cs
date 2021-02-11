using UnityEngine;

public static class WifiServer_Connect
{
    public static void StartConnection()
    {
        WifiServer_Searcher.GetWifiServer += AddServerToList;
        Network.CreateWifiServerSearcher("receiving");
    }

    private static void TcpConnect()
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
                Debug.Log("Server: " + mes[1]);
                DataHolder.WifiGameIp = mes[1];
                TcpConnect();
            }
            DataHolder.MessageUDPget.RemoveAt(0);
        }
    }
}
