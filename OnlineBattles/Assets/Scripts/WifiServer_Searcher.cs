using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class WifiServer_Searcher
{
    public static event DataHolder.Notification GetWifiServer;
    private int _port = DataHolder.WifiPort;
    private IPEndPoint _remoteIp = null;
    private UdpClient _client = null;
    private Timer _timer = null;
    private Thread _receiveThread = null;
    private bool _working = true;
    private string _typeOfUDP = null;

    public WifiServer_Searcher(string type)
    {
        _typeOfUDP = type;   
        CreateClass();
    }

    private void CreateClass()
    {
        _working = true;
        if (_typeOfUDP == "receiving")
        {
            _client = new UdpClient(_port);
            _client.EnableBroadcast = true;

            _receiveThread = new Thread(ReceivingMulticastMessages);
            _receiveThread.Start();
        }
        else if (_typeOfUDP == "spamming")
        {
            _client = new UdpClient();
            _client.EnableBroadcast = true;
            _client.ExclusiveAddressUse = false;

            TimerCallback tm = new TimerCallback(SpammingSeverIp);
            _timer = new Timer(tm, null, 1000, 2000);    
        }       
    }

    private static string GetLocalIPAddressPiece()
    {
        // Если подключён к wifi, то сам чекни свой 
        if (Application.internetReachability.ToString() == "ReachableViaLocalAreaNetwork")
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    string[] IpPieces = ip.ToString().Split('.');
                    return $"{IpPieces[0]}.{IpPieces[1]}.{IpPieces[2]}.";                   
                }
            }
            return null;
        }
        else
        {
            return "192.168.43."; // Стандартный адрес для всех Андроидов
            //TODO: Для айфонов нужен другой адрес, возможно там даже есть функция для этого
        }
    }

    private void SendMessage(string mes, string ip)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(mes);
            _client.Send(data, data.Length, ip, _port);
        }
        catch { TryReconnect(); }
    }

    private void ReceivingMulticastMessages()
    {
        while (_working)
        {
            try
            {
                byte[] data = _client.Receive(ref _remoteIp);
                string messList = Encoding.UTF8.GetString(data);               
                DataHolder.MessageUDPget.Add($"{messList} {_remoteIp.Address}");
                GetWifiServer?.Invoke();
            }
            catch { TryReconnect(); }
        }
    }

    private void SpammingSeverIp(object obj)
    {
        string IpPiece = GetLocalIPAddressPiece();
        int count = 0;
        while (count <= 255)
        {
            string fullIp = IpPiece + count;
            SendMessage("server " + DataHolder.ServerName, fullIp);
            count++;
        }
    }

    private void TryReconnect()
    {
        if (_working == true)
        {
            CloseAll();
            CreateClass();
        }
        else CloseAll(); // На всякий случай
    }

    public void CloseAll()
    {
        _working = false;

        if (_timer != null)
        {
            _timer.Dispose();
            _timer = null;
        }

        if (_client != null)
        {
            _client.Close();
            _client = null;
        }                
    }

    ~WifiServer_Searcher()
    {
        CloseAll();
    }
}
