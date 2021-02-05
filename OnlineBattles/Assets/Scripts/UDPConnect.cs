using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPConnect
{
    public event DataHolder.Notification GetUdpMessage;
    public bool Working { get; set; } = true;
    private static UdpClient _client { get; set; }
    public IPEndPoint remoteIp = null;

    private Thread _receiveThread;
    private string _ip = null;
    private int _port = 0;
    private string _typeOfUDP;

    //TODO: Для всех ЮДП сообщений нужна структура: номер игры - номер лобби id - !свой id в бд! - номер игрока в лобби - сообщение
    public UDPConnect(string type)
    {
        _typeOfUDP = type;
        if (type == "broadcast")
        {
            _ip = "255.255.255.255";

            _port = 55550;
            _client = new UdpClient(_ip, _port);
            _receiveThread = new Thread(ReceivingBroadcastAnswers);
        }
        else if (type == "server")
        {
            _port = 55550;
            _client = new UdpClient(_port);
            _receiveThread = new Thread(ReceivingBroadcastAnswers);
        }
        else
        {
            if (DataHolder.GameType == 3)
            {
                _ip = DataHolder.ServerIp;
                _port = DataHolder.RemotePort;
            }
            else if (DataHolder.GameType == 2)
            {
                _ip = DataHolder.WifiGameIp;
                _port = DataHolder.RemoteWifiPort;
            }
            _client = new UdpClient(_ip, _port);
            _receiveThread = new Thread(ReceivingMessagesLoop);
        }

        _receiveThread.Start();
        _receiveThread.IsBackground = true;
    }

    /// <summary>
    /// Отправка пользовательских UDP сообщений с добавлением "метаданных".
    /// </summary>
    /// <param name="mes">Текст сообщения.</param>
    public void SendMessage(string mes)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes($"{DataHolder.SelectedServerGame} {DataHolder.LobbyID} {DataHolder.IDInThisGame} " + mes);
            _client.Send(data, data.Length);
        }
        catch { TryReconnect(); }
    }

    public void SendMessage(string mes, IPEndPoint ip)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(mes);
            _client.Send(data, data.Length, ip);
        }
        catch { TryReconnect(); }
    }

    /// <summary>
    /// Цикл приёма UDP сообщений и помещение их в MessageUDPget.
    /// </summary>
    private void ReceivingMessagesLoop()
    {
        while (Working)
        {
            try
            {
                byte[] data = _client.Receive(ref remoteIp);
                string messList = Encoding.UTF8.GetString(data);
                DataHolder.MessageUDPget.Add(messList);
            }
            catch { TryReconnect(); }
        }
    }

    private void ReceivingBroadcastAnswers()
    {
        while (Working)
        {
            try
            {
                byte[] data = _client.Receive(ref remoteIp);
                string messList = Encoding.UTF8.GetString(data);
                if (ReplyToRequest(messList))
                    continue;
                DataHolder.MessageUDPget.Add($"{messList} {remoteIp.Address} {remoteIp.Port}");
                GetUdpMessage?.Invoke();
                asfj();
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                TryReconnect(); }
        }
    }

    private void asfj()
    {
        while (DataHolder.MessageUDPget.Count > 0)
        {
            string[] mes = DataHolder.MessageUDPget[0].Split(' ');
            if (mes[0] == "server")
            {
                Debug.Log("Server: " + mes[1]);
            }
            DataHolder.MessageUDPget.RemoveAt(0);
        }
    }

    private bool ReplyToRequest(string message)
    {
        if (message == "server?")
        {
            DataHolder.ClientUDP.SendMessage("server", remoteIp);
            Debug.Log("Request from: " + remoteIp.Address);
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Уничтожение старого, и если игра ещё не завершилась, создание нового экземпляра client.
    /// </summary>
    private void TryReconnect()
    {     
        ////TODO: Это всё какой-то мусор, надо это переосмыслить
        //if (Working)
        //{
        //    CloseClient();
        //    Working = true;

        //    if (_ip == null)
        //        _client = new UdpClient(_port);
        //    else
        //        _client = new UdpClient(_ip, _port);

        //    else if (_typeOfUDP == "server")
        //    {
        //        _client = new UdpClient(_port);
        //    }
        //        //TODO: Игрок может отменить реконнект и игру, тогда надо будет обнулить и удалить все UDP соединения
        //        //TODO: Сделать отдельную функцию выхода в меню, если ты потерял связь во время игры и не хочешь реконнкта
        //}
    }

    /// <summary>
    /// Остановка всех UDP процессов. GameOn становится false, client уничтожается.
    /// </summary>
    public void CloseClient()
    {
        if (_client != null)
        {
            Working = false;
            _client.Close();
            _client = null;
        }
    }

    ~UDPConnect()
    {
        CloseClient();
    }
}
