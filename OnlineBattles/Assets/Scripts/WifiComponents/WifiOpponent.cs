using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class WifiOpponent
{
    public TcpClient Client { get; private set; }
    public string PlayerName { get; set; } = null;
    public DateTime LastReciveTime { get; private set; }
    public long Ping { get; set; } = 0;

    private List<string[]> _tcpMessages = new List<string[]>();  
    private NetworkStream _streamGame = null;

    public int TCPMessagesCount => _tcpMessages.Count;

    public WifiOpponent(TcpClient client, DateTime time)
    {
        Client = client;
        LastReciveTime = time;
        DataHolder.WifiGameIp = ((IPEndPoint)(Client.Client.RemoteEndPoint)).Address.ToString();
        _streamGame = Client.GetStream();
    }   

    public string[] UseAndDeleteFirstMessage()
    {
        string[] message = _tcpMessages[0];
        _tcpMessages.RemoveAt(0);
        return message;
    }   

    public void GetMessagesFromStream()
    {
        List<byte> buffer = new List<byte>();
        try
        {
            while (buffer.Count < 4)
                CheckStream(buffer);

            int mesCount = BitConverter.ToInt32(buffer.ToArray(), 0);
            buffer.Clear();

            while (buffer.Count < mesCount)
                CheckStream(buffer);
        }
        catch { return; }

        string message = Encoding.UTF8.GetString(buffer.ToArray());
        AddNewMessage(message);
    }

    public void SendTcpMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        byte[] sizeInByte = BitConverter.GetBytes(buffer.Length);
        try
        {
            _streamGame.Write(sizeInByte, 0, sizeInByte.Length);
            _streamGame.Write(buffer, 0, buffer.Length);
        }
        catch { } //TODO: Тут могут быть какие-то проблемы типо реконнекта как на клиенте?
    }

    private void AddNewMessage(string message)
    {
        _tcpMessages.Add(message.Split(' '));
        LastReciveTime = DateTime.UtcNow;
    }

    private void CheckStream(List<byte> Buffer)
    {
        if (_streamGame.DataAvailable)
        {
            int ReadByte = _streamGame.ReadByte();
            if (ReadByte > -1)
                Buffer.Add((byte)ReadByte);
        }
    }

    public void ClearStream()
    {
        if (_streamGame != null)
        {
            _streamGame.Close();
            _streamGame = null;
        }
    }
}
