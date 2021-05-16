﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TCPConnect
{
    public event DataHolder.Notification MessageReceived;
    public event DataHolder.Notification BigMessageReceived;

    public bool CanStartReconnect = false;

    private const int ConnectionTimedOut = 3000;
    private const int MessageLengthLimit = 100;

    private TcpClient _client;
    private Thread _clientListener;
    private NetworkStream _NS;
    private bool _working = true;
    private List<byte> _receivedBytesBuffer = new List<byte>();

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
            if (result.AsyncWaitHandle.WaitOne(ConnectionTimedOut, true))
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
            DataHolder.LastSend = DateTime.UtcNow;
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
            _receivedBytesBuffer.Clear();
            try
            {
                while (_receivedBytesBuffer.Count < 4)
                    GetByteFromStream();

                int mesCount = BitConverter.ToInt32(_receivedBytesBuffer.ToArray(), 0);
                _receivedBytesBuffer.Clear();

                while (_receivedBytesBuffer.Count < mesCount)
                    GetByteFromStream();
            }
            catch
            {
                TryStartReconnect();
                break;
            }

            if (_receivedBytesBuffer.Count < MessageLengthLimit)
            {
                string message = Encoding.UTF8.GetString(_receivedBytesBuffer.ToArray());
                DataHolder.MessageTCP.Add(message);
                MessageReceived?.Invoke();
            }
            else
            {
                DataHolder.BigArray = _receivedBytesBuffer;
                BigMessageReceived?.Invoke();
            }
        }
    }

    /// <summary>
    /// Проверка потока на наличие полученных байт и добавление их к _receivedBytesBuffer.
    /// </summary>
    private void GetByteFromStream()
    {
        if (_NS.DataAvailable)
        {
            int ReadByte = _NS.ReadByte();
            if (ReadByte > -1)
            {
                _receivedBytesBuffer.Add((byte)ReadByte);
            }
        }
    }

    /// <summary>
    /// Закрытие сокетов, потока чтения и этого экземпляра, и вызов функции реконнекта.
    /// </summary>
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
