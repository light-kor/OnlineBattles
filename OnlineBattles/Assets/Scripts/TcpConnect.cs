using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpConnect
{
    public static TcpClient client;
    Thread clientListener;
    public static IEnumerable<IPAddress> GetLocalIPAddress()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(f => f.AddressFamily == AddressFamily.InterNetwork);
    }

    public TcpConnect()
    {
        //"127.0.0.1" - локальный; "192.168.0.1" ; "5.18.204.242" - общий ip сети; "192.168.0.107" - второй ноут; "192.168.137.1" - этот ноут
        string connectIp = "127.0.0.1";
        //192.168.0.106
        // Нет разницы First или Last
        string ip = GetLocalIPAddress().First().ToString();
        IPAddress localIP = GetLocalIPAddress().Last();
        IPEndPoint localSocket = new IPEndPoint(localIP, DataHolder.Port);
        //client.Client.Bind(localIP);

        client = new TcpClient();

        // Включть вместо верхнего, если тестишь на разных устройствах
        //client = new TcpClient(localSocket);

        //Пробрасывать First или Last ip, но для сокета точно последний

        //NATUPNPLib.UPnPNAT upnpnat = new NATUPNPLib.UPnPNAT();
        //NATUPNPLib.IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;
        //mappings.Add(localPort, "TCP", localPort, ip, true, "BattlesPort"); //если требуется пробрасываем порт

        //client.Connect(IPAddress.Parse(connectIp), Port);

        var result = client.BeginConnect(connectIp, DataHolder.remotePort, null, null);
        if (result.AsyncWaitHandle.WaitOne(2000, true))
        {
            client.EndConnect(result);
            clientListener = new Thread(Reader);
            clientListener.Start();
            clientListener.IsBackground = true;
        }
        else
        {           
            client.Dispose();
            client.Close();
            Debug.Log("fwef");
            // Вся обработка этого момента в DataHolder
            //TODO: Сообщить в DataHolder, что создать не удалось
        }


        //TODO: Что делать, если соединение обрубится во время игры? Надо как-то обработать

        //Debug.Log("My local IpAddress is : " + IPAddress.Parse(((IPEndPoint)client.Client.LocalEndPoint).Address.ToString()) + " | I am connected on port number " + ((IPEndPoint)client.Client.LocalEndPoint).Port.ToString());
        //mappings.Remove(port, "TCP");
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
            //TODO: И что дальше?
        }

    }

    static void Reader()
    {
        NetworkStream NS;
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
                Debug.Log("Close thread");
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
        }
    }

    ~TcpConnect()
    {
        if (client != null)
        {
            //TODO: Завершается ли при этом поток?
            client.Dispose();
            client.Close();
            Debug.Log("closee");
        }
    }
}
