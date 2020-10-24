using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPConnect
{
    private IPEndPoint remoteIp = null; // адрес входящего подключения  
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
        catch
        {
            Debug.Log("Send error");
            if (GameOn)
            {
                if (client != null)
                    client.Close();
                client = new UdpClient(DataHolder.ConnectIp, DataHolder.RemotePort);
            }
            //TODO: Сделать перезагрузку tcp. Или как-то здесь словить ошибку и что-то сделать
        }
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
            catch
            {
                Debug.Log("Res error");
                if (GameOn)
                {
                    if (client != null)
                        client.Close();

                    client = new UdpClient(DataHolder.ConnectIp, DataHolder.RemotePort);
                }
            }
        }
    }

    public void CloseClient()
    {
        if (client != null)
        {
            GameOn = false;
            client.Dispose();
            client.Close();
            client = null;
            Debug.Log("Destroy udp client");
        }
    }

    ~UDPConnect()
    {
        CloseClient();
    }
}
