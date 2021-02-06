﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPConnect
{    
    public IPEndPoint remoteIp = null;
    private static UdpClient _client = null;
    private Thread _receiveThread = null;
    private bool Working = true;
    private string _ip = null;
    private int _port = 0;

    //TODO: Для всех ЮДП сообщений для сервера нужна структура: номер игры - номер лобби id - !свой id в бд! - номер игрока в лобби - сообщение
    public UDPConnect()
    {
        if (DataHolder.GameType == 3)
        {
            _ip = DataHolder.ServerIp;
            _port = DataHolder.RemoteServerPort;
            _client = new UdpClient(_ip, _port);
        }                     
        else if (DataHolder.GameType == 2)
        {
            _ip = DataHolder.WifiGameIp;
            _port = DataHolder.WifiPort;
            _client = new UdpClient(_port);
            _client.Connect(_ip, _port);
        }
                   
        _receiveThread = new Thread(ReceivingMessagesLoop);
        _receiveThread.Start();
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
    }

    ~UDPConnect()
    {
        CloseClient();
    }
}
