using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPConnect
{
    private IPEndPoint remoteIp = null; // адрес входящего подключения  
    public bool GameOn = false;

    static UdpClient sender;
    static UdpClient receiver;

    public UDPConnect()
    {        
        GameOn = true;
        //TODO: Принимать только с ip нашего сервера
        sender = new UdpClient(DataHolder.connectIp, DataHolder.remotePort); // создаем UdpClient для отправки сообщений
        receiver = new UdpClient(DataHolder.localPort); // UdpClient для получения данных

        Thread receiveThread = new Thread(ReceiveMessage);
        receiveThread.Start();
        receiveThread.IsBackground = true;
    }

    public void SendMessage(string mes)
    {
        try
        {
            //mes += "|";
            //mes.Trim();
            byte[] data = Encoding.UTF8.GetBytes(mes);
            sender.Send(data, data.Length); // отправка
        }
        catch
        {
            if (GameOn)
            {
                if (receiver != null)
                    sender.Close();
                sender = new UdpClient(DataHolder.connectIp, DataHolder.remotePort);
            }           
            // Сделать перезагрузку tcp
            // Или как-то здесь словить ошибку и что-то сделать
        }
    }

    private void ReceiveMessage()
    {
        while (GameOn)
        {
            try
            {
                byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                //string[] words = Encoding.UTF8.GetString(data).Split(new char[] { '|' });
                //// Удаляем последний пустой элемент
                //List<string> messList = new List<string>(words);
                //messList.Remove("");

                string messList = Encoding.UTF8.GetString(data);
                // Если пришло несколько сообщений, подели их на отдельные
                //for (int i = 0; i < messList.Count; i++)
                //{
                    //DataHolder.messageUDPget.Add(messList[i]);
                //}
                DataHolder.messageUDPget.Add(messList);
            }
            catch
            {
                if (GameOn)
                {
                    if (receiver != null)
                        receiver.Close();
                    receiver = new UdpClient(DataHolder.localPort);
                }
            }
        }
    }

    public void CloseClient()
    {
        if (sender != null)
        {
            sender.Dispose();
            sender.Close();
            sender = null;
            Debug.Log("Destroy sender");
        }

        if (receiver != null)
        {
            GameOn = false;
            receiver.Dispose();
            receiver.Close();
            receiver = null;
            Debug.Log("Destroy receiver");
        }
    }

    ~UDPConnect()
    {
        CloseClient();
    }
}
