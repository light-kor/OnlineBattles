using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class WifiServer_Searcher
{
    public static event UnityAction<string> GetMessage;
    private int _port = DataHolder.WifiPort;
    private IPEndPoint _remoteIp = null;
    private UdpClient _client = null;
    private System.Threading.Timer _timer = null;
    private Thread _receiveThread = null;
    private bool _working = true;
    private string _typeOfUDP = null;
    private string IpPiece = null;

    public WifiServer_Searcher(string type)
    {
        _typeOfUDP = type;
        IpPiece = GetLocalIPAddressPiece(); // Можно вызывать только из главного потока!!!
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
            _timer = new System.Threading.Timer(tm, null, 1000, 2000);    
        }       
    }

    private string GetLocalIPAddressPiece()
    {
        // Если подключён к wifi, то сам чекни свой       
        if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
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
                GetMessage?.Invoke($"{messList} {_remoteIp.Address}");
            }
            catch { TryReconnect(); }
        }
    }

    private void SpammingSeverIp(object obj)
    {
        int count = 0;
        while (count <= 255)
        {
            string fullIp = IpPiece + count;
            SendMessage("server " + DataHolder.NickName, fullIp);
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
        else CloseAll();
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
