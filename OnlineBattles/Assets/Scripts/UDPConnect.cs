using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPConnect
{
    private IPEndPoint remoteIp = null; // адрес входящего подключения  
    public bool GameOn = false;
    string ip = "188.134.87.78";

    static UdpClient sender;
    static UdpClient receiver;

    public UDPConnect()
    {        
        GameOn = true;
        //TODO: Принимать только с ip нашего сервера
        sender = new UdpClient(ip, DataHolder.remotePort); // создаем UdpClient для отправки сообщений
        receiver = new UdpClient(DataHolder.localPort); // UdpClient для получения данных
        //udpClient = new UdpClient();
        //udpClient.Connect(ip, DataHolder.Port);

        Thread receiveThread = new Thread(ReceiveMessage);
        receiveThread.Start();
        receiveThread.IsBackground = true;
    }

    public void SendMessage(string mes)
    {
        try
        {
            mes += "|";
            mes.Trim();
            byte[] data = Encoding.UTF8.GetBytes(mes);
            sender.Send(data, data.Length); // отправка
        }
        catch
        {
            sender.Close();
            sender = new UdpClient();
        }
    }

    private void ReceiveMessage()
    {
        while (GameOn)
        {
            try
            {
                byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                string[] words = Encoding.UTF8.GetString(data).Split(new char[] { '|' });
                // Удаляем последний пустой элемент
                List<string> messList = new List<string>(words);
                messList.Remove("");

                // Если пришло несколько сообщений, подели их на отдельные
                for (int i = 0; i < messList.Count; i++)
                {
                    DataHolder.messageUDPget.Add(messList[i]);
                }
            }
            catch
            {
                receiver.Close();
                receiver = new UdpClient(DataHolder.localPort);
            }
        }
    }
}
