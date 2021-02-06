using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPConnect
{
    public static event DataHolder.Notification GetWifiServer;
    public bool Working { get; set; } = true;
    public IPEndPoint remoteIp = null;
    private static UdpClient _client { get; set; } = null;

    private Timer timer = null;
    private Thread _receiveThread = null;
    private string _ip = null;
    private int _port = 0;
    private string _typeOfUDP = null;

    //TODO: Для всех ЮДП сообщений нужна структура: номер игры - номер лобби id - !свой id в бд! - номер игрока в лобби - сообщение
    public UDPConnect()
    {
        if (DataHolder.GameType == 3)
            _ip = DataHolder.ServerIp;           
        else if (DataHolder.GameType == 2)
            _ip = DataHolder.WifiGameIp;

        _port = DataHolder.RemotePort;
        _client = new UdpClient(_ip, _port);
        _receiveThread = new Thread(ReceivingMessagesLoop);

        _receiveThread.Start();
        _receiveThread.IsBackground = true; //TODO: Надо ли?
    }

    public UDPConnect(string type)
    {
        _typeOfUDP = type;
        _port = DataHolder.RemotePort;
        _ip = "235.5.5.225";

        if (type == "waiting")
        {
            _client = new UdpClient(_port);
            _client.JoinMulticastGroup(IPAddress.Parse(_ip), 20);
            _receiveThread = new Thread(ReceivingMulticastMessages);
            _receiveThread.Start();
        }
        else if (type == "multicast")
        {
            _client = new UdpClient(_ip, _port);
            TimerCallback tm = new TimerCallback(SpammingSeverLoop);
            timer = new Timer(tm, null, 1000, 2000);
        }
                   
    }

    public void SendMessage(string mes)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(mes);
            _client.Send(data, data.Length);
        }
        catch { TryReconnect(); }
    }

    /// <summary>
    /// Отправка пользовательских UDP сообщений с добавлением "метаданных".
    /// </summary>
    /// <param name="mes">Текст сообщения.</param>
    public void SendMessage(string mes, bool online)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes($"{DataHolder.SelectedServerGame} {DataHolder.LobbyID} {DataHolder.IDInThisGame} " + mes);
            _client.Send(data, data.Length);
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

    private void ReceivingMulticastMessages()
    {       
        while (Working)
        {
            try
            {
                byte[] data = _client.Receive(ref remoteIp);                
                string messList = Encoding.UTF8.GetString(data);
                DataHolder.MessageUDPget.Add($"{messList} {remoteIp.Address} {remoteIp.Port}");
                GetWifiServer?.Invoke();
            }
            catch { TryReconnect(); }
        }
    }

    private void SpammingSeverLoop(object obj)
    {
        SendMessage("server");
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
        Working = false;

        if (_receiveThread != null)
            _receiveThread.Abort();

        if (_client != null)           
            _client.Close();

        if (timer != null)
            timer.Dispose();
    }

    ~UDPConnect()
    {
        CloseClient();
    }
}
