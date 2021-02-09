using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class WifiServer_Searcher
{
    public static event DataHolder.Notification GetWifiServer;
    private const string IP = "235.5.5.225";
    private int _port = 0;
    private IPEndPoint _remoteIp = null;
    private UdpClient _client = null;
    private Timer _timer = null;
    private Thread _receiveThread = null;
    private bool _working = true;
    private string IpPiece;

    private string _typeOfUDP = null;

    public WifiServer_Searcher(string type)
    {
        _typeOfUDP = type;
        _port = DataHolder.WifiPort;

        string localIP = GetLocalIPAddress();
        string[] IpPieces = localIP.Split('.');
        IpPiece = $"{IpPieces[0]}.{IpPieces[1]}.{IpPieces[2]}.";

        CreateClass();
    }

    private void CreateClass()
    {
        _working = true;
        if (_typeOfUDP == "receiving")
        {
            _client = new UdpClient(_port);
            //_client.JoinMulticastGroup(IPAddress.Parse(IP));
            _receiveThread = new Thread(ReceivingMulticastMessages);
            _receiveThread.Start();
        }
        else if (_typeOfUDP == "spamming")
        {
            _client = new UdpClient();
            //_client = new UdpClient(IP, _port);
            TimerCallback tm = new TimerCallback(SpammingSeverLoop2);
            _timer = new Timer(tm, null, 1000, 3000);
            _client.ExclusiveAddressUse = false;
        }       
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return null;
    }

    public void SendMessage(string mes, string ip)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(mes);
            _client.Send(data, data.Length, ip, _port);
        }
        catch { TryReconnect(); }
    }

    public void SendMessage(string mes)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(mes);
            _client.Send(data, data.Length);
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

    private void SpammingSeverLoop(object obj)
    {
        SendMessage("server");
    }

    private void SpammingSeverLoop2(object obj)
    {
        int count = 2;
        while (count < 255)
        {
            string fullIp = IpPiece + count;
            SendMessage("server", fullIp);
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
