using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class WifiServerSearch
{
    public static event DataHolder.Notification GetWifiServer;
    private const string _ip = "235.5.5.225";
    private int _port = 0;
    private IPEndPoint remoteIp = null;
    private UdpClient _client = null;
    private Timer timer = null;
    private Thread _receiveThread = null;
    private bool Working = true;
    
    private string _typeOfUDP = null;

    public WifiServerSearch(string type)
    {
        _typeOfUDP = type;
        _port = DataHolder.WifiPort;
        CreateClass();
    }

    private void CreateClass()
    {
        Working = true;
        if (_typeOfUDP == "receiving")
        {
            _client = new UdpClient(_port);
            _client.JoinMulticastGroup(IPAddress.Parse(_ip), 20);
            _receiveThread = new Thread(ReceivingMulticastMessages);
            _receiveThread.Start();
        }
        else if (_typeOfUDP == "spamming")
        {
            _client = new UdpClient(_ip, _port);
            TimerCallback tm = new TimerCallback(SpammingSeverLoop);
            timer = new Timer(tm, null, 1000, 2000);
        }
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
        while (Working)
        {
            try
            {
                byte[] data = _client.Receive(ref remoteIp);
                string messList = Encoding.UTF8.GetString(data);
                DataHolder.MessageUDPget.Add($"{messList} {remoteIp.Address}");
                GetWifiServer?.Invoke();
            }
            catch { TryReconnect(); }
        }
    }

    private void SpammingSeverLoop(object obj)
    {
        SendMessage("server");
    }

    private void TryReconnect()
    {
        if (Working == true)
        {
            CloseAll();
            CreateClass();
        }
        else CloseAll(); // На всякий случай

    }

    public void CloseAll()
    {
        Working = false;

        if (_receiveThread != null)
        {
            _receiveThread.Abort();
            _receiveThread = null;
        }
            
        if (timer != null)
        {
            timer.Dispose();
            timer = null;
        }

        if (_client != null)
        {
            _client.Close();
            _client = null;
        }                
    }

    ~WifiServerSearch()
    {
        CloseAll();
    }
}
