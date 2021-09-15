using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class WifiServer_Host
{
    public const float UpdateRate = 0.03125f; // Отправка UDP инфы каждые UpdateRate мс 

    public static event DataHolder.Notification AcceptOpponent;
    public static event DataHolder.Notification OpponentGaveUp;

    public static string OpponentStatus = null;
    public static bool OpponentIsReady { get; private set; } = false; //TODO: При дисконнеекте делать false. Переделать бы на подтверждение выбора карты.
    public static WifiOpponentInfo Opponent { get; private set; } = null;
       
    private static TcpListener _listener = null;
    private static NetworkStream _streamGame = null;
    private static Thread receiveThread; //TODO: Удалить его in Close тоже.
    private static bool _working, _opponentCancelConnect, _getPing, _messageHandlerWorking;

    public static async void StartHosting()
    {
        Network.CloseTcpConnection();
        Network.CloseUdpConnection();
        await Task.Run(() => CloseConnection());
        
        _working = true;
        Network.CreateWifiServerSearcher("spamming");
        _listener = new TcpListener(IPAddress.Any, DataHolder.WifiPort);
        _listener.Start();

        while (true)
        {
            await Task.Run(() => ClearOpponentInfo()); // Это нужно для каждого повторного круга            

            await Task.Run(() => ListnerClientsTCP());

            if (_working == false)
            {
                CloseConnection();
                break;
            }
            else if (_opponentCancelConnect == true)
                continue;

            await Task.Run(() => WaitForLogin());

            if (_working == false)
            {
                CloseConnection();
                break;
            }
            else if (_opponentCancelConnect == true)
                continue;

            await Task.Run(() => WaitPlayerAnswer());

            if (_working == false)
            {
                CloseConnection();
                break;
            }
            else if (_opponentCancelConnect == true)
                continue;

            if (OpponentStatus == "accept")
            {
                Network.CloseWifiServerSearcher();
                SendTcpMessage("accept");

                await Task.Delay(500); // Чтоб всё прогрузилось на клиенте, перед запросом пинга
                await Task.Run(() => CheckPing());

                if (_working == false)
                {
                    CloseConnection();
                    break;
                }
                else if (_opponentCancelConnect == true)
                    continue;

                OpponentIsReady = true;
                AcceptOpponent?.Invoke();
                StopListening();
                break;
            }
            else if (OpponentStatus == "denied")
            {
                SendTcpMessage("denied");                
                continue;
            }
        }
    }

    private static void TcpMessageHandler()
    {
        while (_messageHandlerWorking)
        {
            GetTcpMessageFromStream();
            if (Opponent != null && Opponent.TcpMessages.Count > 0)
            {
                string[] mes = Opponent.TcpMessages[0].Split(' ');
                switch (mes[0])
                {
                    case "GiveUp":
                        OpponentGaveUp?.Invoke();
                        break;

                    case "disconnect":
                        Disconnect();
                        return;

                    case "name":
                        Opponent.PlayerName = mes[1];
                        new Notification("Подключился игрок:\r\n" + Opponent.PlayerName, Notification.NotifTypes.WifiRequest, 0);
                        break;

                    case "ping":
                        _getPing = true;
                        break;

                    case "Cancel":
                        _opponentCancelConnect = true;
                        new Notification("Игрок отменил запрос", Notification.NotifTypes.WifiRequest, Notification.ButtonTypes.SimpleClose);                       
                        break;

                    case "Check":
                        break;

                    default:
                        Opponent.MessageTCPforGame.Add(Opponent.TcpMessages[0]);
                        break;

                }
                if (Opponent != null && Opponent.TcpMessages.Count > 0) // Иначе бывают ошибки
                    Opponent.TcpMessages.RemoveAt(0);
            }
            CheckDisconnect();
        }
    }

    #region ConnectionSteps

    /// <summary>
    /// Ожидание подключения любого игрока.
    /// </summary>
    private static void ListnerClientsTCP()
    {
        while (_working)
        {
            if (_listener.Pending())
            {             
                Opponent = new WifiOpponentInfo(_listener.AcceptTcpClient(), DateTime.UtcNow);
                _streamGame = Opponent.Client.GetStream();
                _messageHandlerWorking = true;
                receiveThread = new Thread(TcpMessageHandler);
                receiveThread.Start();
                return;
            }
        }
    }

    /// <summary>
    /// Ожидание получения имени оппонента.
    /// </summary>
    private static void WaitForLogin()
    {
        while (_working)
        {
            if (Opponent.PlayerName != null)
                return;

            if (_opponentCancelConnect == true)
                return;
        }
    }

    /// <summary>
    /// Ожидание решения игрока о подключении оппонента
    /// </summary>
    private static void WaitPlayerAnswer()
    {
        while (_working)
        {
            if (OpponentStatus != null)
                return;

            if (_opponentCancelConnect == true)
                return;
        }
    }

    /// <summary>
    /// Получения пинга оппонента
    /// </summary>
    private static void CheckPing()
    {
        _getPing = false;
        SendTcpMessage("ping");
        long StartPingTimeInTicks = DateTime.UtcNow.Ticks;
        while (_working)
        {
            if (_getPing == true)
            {
                Opponent.Ping = DateTime.UtcNow.Ticks - StartPingTimeInTicks;
                SendTcpMessage($"time {DateTime.UtcNow.Ticks - (Opponent.Ping / 2)}");
                return;
            }

            if (_opponentCancelConnect == true)
                return;
        }       
    }
    
    #endregion

    public static void SendTcpMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        byte[] sizeInByte = BitConverter.GetBytes(buffer.Length);
        try
        {
            Opponent.Client.GetStream().Write(sizeInByte, 0, sizeInByte.Length);
            Opponent.Client.GetStream().Write(buffer, 0, buffer.Length);
        }
        catch { } //TODO: Тут могут быть какие-то проблемы типо реконнекта как на клиенте?
    }

    private static void GetTcpMessageFromStream()
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
        catch { return; }

        string message = Encoding.UTF8.GetString(buffer.ToArray());
        Opponent.TcpMessages.Add(message);
        Opponent.LastReciveTime = DateTime.UtcNow; //TODO: А тут вообще есть оставновки, или оно бесконечно берёт пустые сообщения?
    }

    private static void CheckStream(List<byte> Buffer)
    {
        if (_streamGame.DataAvailable)
        {
            int ReadByte = _streamGame.ReadByte();
            if (ReadByte > -1)
                Buffer.Add((byte)ReadByte);
        }
    }

    private static void CheckDisconnect()
    {
        if (Opponent != null)
            if ((DateTime.UtcNow - Opponent.LastReciveTime).TotalSeconds > 5)
                Disconnect();      
    }

    private static async void Disconnect()
    {
        await Task.Run(() => CloseConnection());
        OpponentGaveUp?.Invoke();
        DataHolder.GameType = DataHolder.GameTypes.Null; //TODO: Надо ли?
        new Notification("Игрок отключился", Notification.ButtonTypes.MenuButton); //TODO: Настроить и время и действия, а то хз, правильно так или добавить ещё ожидание и дать время на реконнект
    }

    public static void CancelConnect()
    {
        _working = false;
    }

    public static void CloseConnection()
    {
        _working = false;
        Network.CloseWifiServerSearcher();
        ClearOpponentInfo();
        StopListening();
    }

    private static void StopListening()
    {
        if (_listener != null)
        {
            _listener.Stop();
            _listener = null;
        }
    }

    private static void ClearOpponentInfo()
    {
        OpponentStatus = null;
        OpponentIsReady = false;
        _opponentCancelConnect = false;

        if (Opponent != null)
        {
            Opponent.Client.Close();
            Opponent = null;
        }

        if (_streamGame != null)
        {
            _streamGame.Close();
            _streamGame = null;
        }

        _messageHandlerWorking = false;
        if (receiveThread != null)
        {
            receiveThread.Join();
            receiveThread = null;
        }
    }
}
