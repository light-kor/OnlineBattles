using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpConnect
{
    public TcpClient client;
    private Thread clientListener;
    private NetworkStream NS;
    Timer myTimer;

    //"127.0.0.1" - локальный; "192.168.0.1" ; "5.18.204.242" - общий ip сети; "192.168.0.107" - второй ноут; "192.168.137.1" - этот ноут
    private string connectIp = "127.0.0.1";
    //192.168.0.106

    private static IEnumerable<IPAddress> GetLocalIPAddress()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(f => f.AddressFamily == AddressFamily.InterNetwork);
    }

    public TcpConnect()
    {
        
        if (Application.internetReachability.ToString() == "ReachableViaLocalAreaNetwork")
        {
            //ReachableViaCarrierDataNetwork  4G
            //ReachableViaLocalAreaNetwork wifi
            // NotReachable nihuya
        }

#if UNITY_ANDROID


#endif
        IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Any, DataHolder.localPort);
        client = new TcpClient(ipLocalEndPoint);

        //NATUPNPLib.UPnPNAT upnpnat = new NATUPNPLib.UPnPNAT();
        //NATUPNPLib.IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;
        //mappings.Add(DataHolder.localPort, "TCP", DataHolder.localPort, ((IPEndPoint)client.Client.LocalEndPoint).Address.ToString(), true, "BattlesPort");

        //Open Nat - какая-то другая библиотека
        TryConnect();

        //Debug.Log("My local IpAddress is : " + IPAddress.Parse(((IPEndPoint)client.Client.LocalEndPoint).Address.ToString()) + " | I am connected on port number " + ((IPEndPoint)client.Client.LocalEndPoint).Port.ToString());
        //mappings.Remove(DataHolder.Port, "TCP");
    }

    private void TryConnect()
    {
        //client = new TcpClient();
        try
        {
            var result = client.BeginConnect(connectIp, DataHolder.remotePort, null, null);
            if (result.AsyncWaitHandle.WaitOne(2000, true))
            {
                client.EndConnect(result);
                clientListener = new Thread(Reader);
                clientListener.Start();
                clientListener.IsBackground = true;
                DataHolder.Connected = true;
            }
            else
            {
                client.Dispose();
                client.Close();
                DataHolder.Connected = false;
            }
        } catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public void SendMassage(string message)
    {
        //TODO: Добавить здесь try/catch и выключать соединение в случае ошибки. Если ошибка, то заново создать tcpConnect. ЛОГИКА РЕКОННЕКТА
        try
        {
            message += "|";
            message.Trim();
            byte[] Buffer = Encoding.UTF8.GetBytes((message).ToCharArray());
            client.GetStream().Write(Buffer, 0, Buffer.Length);
        }
        catch
        {
            DataHolder.Connected = false;
            DataHolder.needToReconnect = true;
        }
    }

    private void Reader()
    {
        Debug.Log("Start thread");
        while (true)
        {           
            List<byte> Buffer = new List<byte>();
            try
            {               
                NS = client.GetStream();
                while (NS.DataAvailable)
                {
                    int ReadByte = NS.ReadByte();
                    if (ReadByte > -1)
                    {
                        Buffer.Add((byte)ReadByte);
                    }
                }
            }
            catch 
            {
                //TODO: До сюда не всегда доходит
                Debug.Log("Close thread");
                DataHolder.Connected = false;
                DataHolder.needToReconnect = true;
                break;
            }            
            

            if (Buffer.Count > 0)
            {
                string[] words = Encoding.UTF8.GetString(Buffer.ToArray()).Split(new char[] { '|' });
                // Удаляем последний пустой элемент
                List<string> messList = new List<string>(words);
                messList.Remove("");

                for (int i = 0; i < messList.Count; i++)
                {
                    DataHolder.messageTCP.Add(messList[i]);
                }               
            }

            if (client == null)
            {
                break;
            }
        }
    }

    public void Reconnect(GameObject notifPanel)
    {
        Debug.Log("Start Reconnect");

        if (!DataHolder.CheckConnection())
        {
            DataHolder.ShowNotif(notifPanel, 3);
            return;
        }
        else DataHolder.ShowNotif(notifPanel, 4);

        CloseClient();
        TryConnect();
    }


    private void CloseClient()
    {
        client.Dispose();
        client.Close();
        client = null;
    }

    ~TcpConnect()
    {
        if (client != null)
        {
            //TODO: Завершается ли при этом поток?
            CloseClient();
            Debug.Log("Destroy TcpConnect");
        }
    }
}
