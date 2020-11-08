using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPConnect
{
    private IPEndPoint remoteIp = null; 
    public bool GameOn = false;
    public static UdpClient client;

    public UDPConnect()
    {        
        GameOn = true;
        client = new UdpClient(DataHolder.ConnectIp, DataHolder.RemotePort);

        Thread receiveThread = new Thread(ReceiveMessage);
        receiveThread.Start();
        receiveThread.IsBackground = true;
    }

    public void SendMessage(string mes)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(mes);
            client.Send(data, data.Length);
        }
        catch { Reconnect(); }
    }

    private void ReceiveMessage()
    {
        while (GameOn)
        {
            try
            {
                byte[] data = client.Receive(ref remoteIp);
                string messList = Encoding.UTF8.GetString(data);
                DataHolder.MessageUDPget.Add(messList);
            }
            catch { Reconnect(); }
        }
    }

    private void Reconnect()
    {
        if (GameOn)
        {
            if (client != null)
                client.Close();

            client = new UdpClient(DataHolder.ConnectIp, DataHolder.RemotePort);

            // Тестим tcp на всякий случай, вдруг надо всё перезапустить.
            //TODO: Игрок может отменить реконнект и игру, тогда надо будет обнулить и удалить все udp соединения
            DataHolder.ClientTCP.SendMassage("Check");
        }
        else CloseClient();
    }

    public void CloseClient()
    {
        if (client != null)
        {
            GameOn = false;
            client.Dispose();
            client.Close();
            client = null;
        }
    }

    ~UDPConnect()
    {
        CloseClient();
    }
}
