using GameEnumerations;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TCPConnect
{
    public event DataHolder.Notification BigMessageReceived;

    public bool ConnectionIsReady = false;
    public DateTime LastSend = DateTime.UtcNow;
    public List<byte> BigArray = new List<byte>();

    private const int ConnectionTimedOut = 3000;
    private const int MessageLengthLimit = 500;

    private TcpClient _client;
    private Thread _clientListener;
    private NetworkStream _NS;
    private bool _working = true;

    /// <summary>
    /// Выбор типа сервера и попытка подключения.
    /// </summary>
    public TCPConnect()
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

        if (DataHolder.GameType == GameTypes.Multiplayer)
        {
            ip = DataHolder.ServerIp;
            _port = DataHolder.RemoteServerPort;
        }
        else if (DataHolder.GameType == GameTypes.WifiClient)
        {
            ip = DataHolder.WifiGameIp;
            _port = DataHolder.WifiPort;
        }
            

        try
        {
            _client = new TcpClient();
            var result = _client.BeginConnect(ip, _port, null, null);
            if (result.AsyncWaitHandle.WaitOne(ConnectionTimedOut, true))
            {
                _client.EndConnect(result);

                _clientListener = new Thread(ReceivingMessagesLoop);
                _clientListener.Start();
                Network.ConnectionInProgress = true;
            }
            else
            {
                CloseClient();
                Network.ConnectionInProgress = false;
            }                
        }
        catch 
        { 
            CloseClient();
            Network.ConnectionInProgress = false;
        }           
    }

    /// <summary>
    /// Сериализация и отправка TCP сообщения. Сначала отправляется размер сообщения в байтах, а потом уже сам текст.
    /// </summary>
    /// <param name="message">Текст сообщения.</param>
    public void SendMessage(string message) //TODO: На выделенном сервере ничего подобного ещё нет
    {
        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            byte[] sizeInByte = BitConverter.GetBytes(buffer.Length);

            _client.GetStream().Write(sizeInByte, 0, sizeInByte.Length);
            _client.GetStream().Write(buffer, 0, buffer.Length);
            LastSend = DateTime.UtcNow;
        }
        catch { TryStartReconnect(); }
    }

    /// <summary>
    /// Приём TCP-потока с сервера с разделением потока на разные сообщения по байтам. 
    /// Полученные сообщения помещаются в DataHolder.MessageTCP.
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
                    GetByteFromStream(buffer);

                int mesCount = BitConverter.ToInt32(buffer.ToArray(), 0);
                buffer.Clear();

                while (buffer.Count < mesCount)
                    GetByteFromStream(buffer);
            }
            catch
            {
                TryStartReconnect();
                break;
            }

            if (buffer.Count < MessageLengthLimit)
            {
                string message = Encoding.UTF8.GetString(buffer.ToArray());
                Network.AddNewTCPMessage(message);
            }
            else
            {
                BigArray = buffer;
                BigMessageReceived?.Invoke();
            }
        }
    }

    /// <summary>
    /// Проверка потока на наличие полученных байт и добавление их к _receivedBytesBuffer.
    /// </summary>
    private void GetByteFromStream(List<byte> buffer)
    {
        if (_NS.DataAvailable)
        {
            int ReadByte = _NS.ReadByte();
            if (ReadByte > -1)
            {
                buffer.Add((byte)ReadByte);
            }
        }
    }

    /// <summary>
    /// Закрытие сокетов, потока чтения и этого экземпляра, и вызов функции реконнекта.
    /// </summary>
    private void TryStartReconnect()
    {
        //TODO: Пока отключил возможность реконнкта.
        //TODO: Получается, если реально будет ошибка сети, то она не обработается, но да ладно

        //if (ConnectionIsReady)
        //{
        //    ConnectionIsReady = false;
        //    CloseClient();
        //    Network.StartReconnect();
        //}
        ////TODO: Наверное надо и else добавить?
    }

    /// <summary>
    /// Закрытие TCP соединения.
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

    ~TCPConnect()
    {
        CloseClient();
    }
}
