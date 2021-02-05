using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPConnect
{
    public static event DataHolder.Notification GetWifiServer;
    public static event DataHolder.GameNotification ShowGameNotification;
    public bool Working { get; set; } = true;
    public IPEndPoint remoteIp = null;
    private static UdpClient _client { get; set; }
    
    private Thread _receiveThread;
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

        if (type == "broadcast")
        {
            _client = new UdpClient(_port, AddressFamily.InterNetwork); // Можно просто порт указать, разницы нет
            _client.Connect("255.255.255.255", _port); //TODO: Мы так устанавливаем и локальную, и удалённую точку. 
                                                       // Чтоб потом принимать по нужному порту сообщения после закрытия.
                                                       // Закрывать нужно тк нельзя принимать сокетом broadcast
            SendMessage("server?");

            _client.Close();
            _client = new UdpClient(_port);
        }
        else if (type == "server")       
            _client = new UdpClient(_port);           

        _receiveThread = new Thread(ReceivingBroadcastAnswers);
        _receiveThread.Start();
        _receiveThread.IsBackground = true;
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

                Debug.Log(messList);
                ShowGameNotification?.Invoke("Сообщение!\r\n" + messList, 1);

                if (ReplyToRequest(messList))
                    continue;

                DataHolder.MessageUDPget.Add($"{messList} {remoteIp.Address} {remoteIp.Port}");
                GetWifiServer?.Invoke();
            }
            catch { TryReconnect(); }
        }
    }

    private bool ReplyToRequest(string message)
    {
        if (message == "server?")
        {
            DataHolder.ClientUDP.SendMessage("server", remoteIp);
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
