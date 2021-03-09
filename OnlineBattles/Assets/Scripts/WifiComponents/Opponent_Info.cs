using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class Opponent_Info
{
    public TcpClient Client { get; private set; }
    public string PlayerName { get; set; } = null;
    public DateTime LastReciveTime { get; set; }
    public List<string> TcpMessages { get; set; } = new List<string>();
    public List<string> MessageTCPforGame { get; set; } = new List<string>();
    public long Ping { get; set; } = 0;

    public Opponent_Info(TcpClient client, DateTime time)
    {
        Client = client;
        LastReciveTime = time;
        DataHolder.WifiGameIp = ((IPEndPoint)(Client.Client.RemoteEndPoint)).Address.ToString();
    }
}
