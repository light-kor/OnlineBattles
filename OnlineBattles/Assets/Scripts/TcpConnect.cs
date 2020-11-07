using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpConnect
{
    public TcpClient client;
    private Thread clientListener;
    private NetworkStream NS;

    public TcpConnect()
    {       
        //TODO: Нужно ли это где-то?
        //if (Application.internetReachability.ToString() == "ReachableViaLocalAreaNetwork")
        //{
            //ReachableViaCarrierDataNetwork  4G
            //ReachableViaLocalAreaNetwork wifi
            // NotReachable nihuya
        //}
        TryConnect();
    }

    /// <summary>
    /// Попытка подключения к серверу
    /// </summary>
    public void TryConnect()
    {
        CloseClient();
        client = new TcpClient();
        try
        {
            var result = client.BeginConnect(DataHolder.ConnectIp, DataHolder.RemotePort, null, null);
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
                CloseClient();
                DataHolder.Connected = false;
            }
        } 
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    /// <summary>
    /// Отправка TCP-сообщения на сервер с добавлением разделительного знака "|"
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public void SendMassage(string message)
    {
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
            DataHolder.NeedToReconnect = true;
        }
    }

    /// <summary>
    /// Приём TCP-сообщений с сревера с разделением их на отдельные, если склеились. 
    /// Отлавливание ошибок соединения, контроль флага NeedToReconnect для начала реконнекта.
    /// </summary>
    private void Reader()
    {
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
                // В Network начнётся процесс реконнекта
                DataHolder.Connected = false;
                DataHolder.NeedToReconnect = true;
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
                    DataHolder.MessageTCP.Add(messList[i]);
                }               
            }
        }
    }

    private void CloseClient()
    {
        if (client != null)
        {
            //TODO: Завершается ли при этом поток?
            client.Dispose();
            client.Close();
            client = null;
        }
    }

    ~TcpConnect()
    {
        CloseClient();
    }
}
