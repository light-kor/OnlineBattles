using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpConnect
{
    private const int WaitForConnection = 3000;

    private TcpClient client { get; set; }
    private Thread clientListener { get; set; }
    private NetworkStream NS { get; set; }
    private bool ReconnectAlreadyStart { get; set; } = false;

    /// <summary>
    /// Создание экземпляра и попытка подключения к серверу
    /// </summary>
    public TcpConnect()
    {
        //TODO: Нужно ли это где-то?
        //if (Application.internetReachability.ToString() == "ReachableViaLocalAreaNetwork")
        //{
        //ReachableViaCarrierDataNetwork  4G
        //ReachableViaLocalAreaNetwork wifi
        // NotReachable nihuya
        //}

        try
        {
            client = new TcpClient();
            var result = client.BeginConnect(DataHolder.ConnectIp, DataHolder.RemotePort, null, null);
            if (result.AsyncWaitHandle.WaitOne(WaitForConnection, true))
            {
                client.EndConnect(result);

                clientListener = new Thread(ReceivingMessagesLoop);
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
        catch
        {
            CloseClient();
            DataHolder.Connected = false;
        }
    }

    /// <summary>
    /// Отправка TCP-сообщения на сервер с добавлением разделительного знака "|".
    /// </summary>
    /// <param name="message">Текст сообщения.</param>
    public void SendMassage(string message)
    {
        try
        {
            message += "|";
            byte[] Buffer = Encoding.UTF8.GetBytes((message).ToCharArray());
            client.GetStream().Write(Buffer, 0, Buffer.Length);
        }
        catch
        {
            if (!ReconnectAlreadyStart)
            {
                ReconnectAlreadyStart = true;
                CloseClient();
                DataHolder.NetworkScript.StartReconnect();
            }
        }
    }

    /// <summary>
    /// Приём TCP-сообщений с сревера с разделением их на отдельные, если склеились. 
    /// Отлавливание ошибок соединения, DataHolder.NetworkScript.StartReconnect() для начала реконнекта.
    /// </summary>
    private void ReceivingMessagesLoop()
    {
        NS = client.GetStream();
        while (true)
        {           
            List<byte> Buffer = new List<byte>();
            try
            {                              
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
                if (!ReconnectAlreadyStart)
                {
                    ReconnectAlreadyStart = true;
                    CloseClient();
                    DataHolder.NetworkScript.StartReconnect();                   
                }
                break;
            }            
            
            if (Buffer.Count > 0 && DataHolder.Connected)
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

    /// <summary>
    /// Закрытие всех потоков TCP соединения и удаление client.
    /// </summary>
    public void CloseClient() //TODO: Добавить этот вызов на кнопку выхода из приложения
    {       
        if (client != null)
        {
            client.LingerState = new LingerOption(true, 0); //Чтоб он не ожидал.
            client.Close();
            client = null;
        }
        if (NS != null)
        {
            NS.Close();
            NS = null;
        }
    }          

    ~TcpConnect()
    {
        CloseClient();
    }
}
