using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpConnect
{
    public event DataHolder.Notification GetMessage;
    public event DataHolder.Notification GetBigMessage;

    private const int WaitForConnection = 3000;

    public bool CanStartReconnect { get; set; } = false;
    private TcpClient _client { get; set; }
    private Thread _clientListener { get; set; }
    private NetworkStream _NS { get; set; }
    private bool _working = true;

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
        string ip = null;
        int _port = 0;

        if (DataHolder.GameType == "Multiplayer")
        {
            ip = DataHolder.ServerIp;
            _port = DataHolder.RemoteServerPort;
        }
        else if (DataHolder.GameType == "WifiClient")
        {
            ip = DataHolder.WifiGameIp;
            _port = DataHolder.WifiPort;
        }
            

        try
        {
            _client = new TcpClient();
            var result = _client.BeginConnect(ip, _port, null, null);
            if (result.AsyncWaitHandle.WaitOne(WaitForConnection, true))
            {
                _client.EndConnect(result);

                _clientListener = new Thread(ReceivingMessagesLoop);
                _clientListener.Start();
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

    public void SendMessage(string message) //TODO: На сервере я ничего подобного не делал
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            byte[] sizeInByte = BitConverter.GetBytes(buffer.Length);

            _client.GetStream().Write(sizeInByte, 0, sizeInByte.Length);
            _client.GetStream().Write(buffer, 0, buffer.Length);
            DataHolder.LastSend = DateTime.UtcNow;
        }
        catch { TryStartReconnect(); }
    }

    /// <summary>
    /// Приём TCP-сообщений с сревера с разделением их на отдельные, если склеились. 
    /// Отлавливание ошибок соединения, DataHolder.NetworkScript.StartReconnect() для начала реконнекта.
    /// </summary>
    private void ReceivingMessagesLoop()
    {
        _NS = _client.GetStream();
        while (_working)
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
            catch
            {
                TryStartReconnect();
                break;
            }

            if (buffer.Count < 500)
            {
                string message = Encoding.UTF8.GetString(buffer.ToArray());
                DataHolder.MessageTCP.Add(message);
                GetMessage?.Invoke();
            }
            else
            {
                DataHolder.BigArray = buffer;
                GetBigMessage?.Invoke();
            }
        }
    }
   
    private void CheckStream(List<byte> Buffer)
    {
        if (_NS.DataAvailable)
        {
            int ReadByte = _NS.ReadByte();
            if (ReadByte > -1)
            {
                Buffer.Add((byte)ReadByte);
            }
        }
    }

    private void TryStartReconnect()
    {
        if (CanStartReconnect)
        {
            CanStartReconnect = false;
            CloseClient();
            Network.StartReconnect();
        }
    }

    /// <summary>
    /// Закрытие всех потоков TCP соединения и удаление client.
    /// </summary>
    public void CloseClient() //TODO: Добавить этот вызов на кнопку выхода из приложения
    {
        _working = false; //TODO: Переосмыслить все реконнекты и закрытия

        if (_client != null)
        {
            _client.LingerState = new LingerOption(true, 0); //Чтоб он не ожидал.
            _client.Close();
            _client = null;
        }
        
        if (_NS != null)
        {
            _NS.Close();
            _NS = null;
        }
    }          

    ~TcpConnect()
    {
        CloseClient();
    }
}
