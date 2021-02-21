﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpConnect
{
    public event DataHolder.Notification GetMessage;

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

        if (DataHolder.GameType == 3)
        {
            ip = DataHolder.ServerIp;
            _port = DataHolder.RemoteServerPort;
        }
        else if (DataHolder.GameType == 2)
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

    /// <summary>
    /// Отправка TCP-сообщения на сервер с добавлением разделительного знака "|".
    /// </summary>
    /// <param name="message">Текст сообщения.</param>
    public void SendMessage(string message)
    {
        try
        {
            message += "|";
            byte[] Buffer = Encoding.UTF8.GetBytes((message).ToCharArray());
            _client.GetStream().Write(Buffer, 0, Buffer.Length);
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
            List<byte> Buffer = new List<byte>();
            try
            {                              
                while (_NS.DataAvailable)
                {
                    int ReadByte = _NS.ReadByte();
                    if (ReadByte > -1)
                    {
                        Buffer.Add((byte)ReadByte);
                    }
                }
            }
            catch
            {
                TryStartReconnect();
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
                    GetMessage?.Invoke();
                }               
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
