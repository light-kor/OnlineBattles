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
    public static event DataHolder.GameNotification FoundOnePlayer;
    public static event DataHolder.Notification CleanHostingUI;
    public static event DataHolder.Notification AcceptOpponent;

    public static string OpponentStatus = null;
    public static bool OpponentIsReady = false;

    public static Opponent_Info _opponent { get; private set; } = null;
    private const int UpdateRate = 50; // Отправка UDP инфы каждые UpdateRate мс    
    private static TcpListener _listener { get; set; } = null;
    private static NetworkStream _streamGame { get; set; } = null;
    private static Thread receiveThread = new Thread(TcpMessageHandler);
    private static bool _messageHandlerIsBusy = false;
    private static bool _searching;

    public static async void StartHosting()
    {
        Network.CloseTcpConnection();
        Network.CloseUdpConnection();

        CancelSarching();
        CleanHostingUI?.Invoke();
        //TODO: Когда начинаешь хостить, наверное надо самому отключиться от основного сервера

        _searching = true;
        Network.CreateWifiServerSearcher("spamming");       
        _listener = new TcpListener(IPAddress.Any, DataHolder.WifiPort);
        _listener.Start();

    StartSearch:
        await Task.Run(() => ListnerClientsTCP());

        if (_searching)
        {
            await Task.Run(() => WaitForConnections());

            if (_searching)
            {
                string myDecision = await Task.Run(() => WaitPlayerAnswer());
                OpponentStatus = null;
                if (myDecision == "accept")
                {
                    Network.CloseWifiServerSearcher();
                    SendMessage("accept");
                    DataHolder.WifiGameIp = ((IPEndPoint)(_opponent.Client.Client.RemoteEndPoint)).Address.ToString();
                    await Task.Run(() => CheckPing());
                    Debug.Log(_opponent.Ping/10000);
                    OpponentIsReady = true; //TODO: При дисконнеекте делать false
                    AcceptOpponent?.Invoke();                    
                    receiveThread.Start();
                }
                else if (myDecision == "denied")
                {
                    SendMessage("denied");
                    ClearOpponentInfo();
                    goto StartSearch;
                }
            }
            else
                CancelSarching();
        }
        else
            CancelSarching();
    }

    private static string WaitPlayerAnswer()
    {
        while (true)
        {
            if (OpponentStatus == "accept")
                return "accept";
            else if (OpponentStatus == "denied")
                return "denied";
        }
    }

    public static void CheckPing()
    {
        SendMessage("ping");
        _opponent.StartPingTimeInTicks = DateTime.UtcNow.Ticks;
        while (_searching)
        {
            AddMessageToPlayerBuffer();
            if (_opponent.TcpMessages.Count > 0)
            {
                string[] mes = _opponent.TcpMessages[0].Split(' ');
                if (mes[0] == "ping")
                {
                    _opponent.Ping = DateTime.UtcNow.Ticks - _opponent.StartPingTimeInTicks;
                    _opponent.StartPingTimeInTicks = 0;
                    SendMessage($"time {DateTime.UtcNow.Ticks + (_opponent.Ping / 2)}");
                    _opponent.TcpMessages.RemoveAt(0);
                    break;
                }
                _opponent.TcpMessages.RemoveAt(0);
            }
        }
        
    }

    private static void WaitForConnections()
    {
        while (_searching)
        {
            AddMessageToPlayerBuffer();
            if (_opponent.TcpMessages.Count > 0)
            {
                string[] mes = _opponent.TcpMessages[0].Split(' ');
                if (mes[0] == "name") 
                { 
                    _opponent.PlayerName = mes[1];
                    FoundOnePlayer?.Invoke("Подключился игрок:\r\n" + _opponent.PlayerName, 5);
                    _opponent.TcpMessages.RemoveAt(0);
                    break;
                }
                _opponent.TcpMessages.RemoveAt(0);
            }
        }
    }
    public static void ListnerClientsTCP()
    {
        while (_searching)
        {
            if (_listener.Pending())
            {
                _opponent = new Opponent_Info(_listener.AcceptTcpClient(), DateTime.UtcNow);
                break;
            }
        }
    }

    public static bool SendMessage(string mes)
    {
        //Обязательно добавляем "|" на конце каждого сообщения, чтоб делить их на отдельные в потоке
        mes += "|";
        byte[] Buffer = Encoding.UTF8.GetBytes((mes).ToCharArray());
        try
        {
            _opponent.Client.GetStream().Write(Buffer, 0, Buffer.Length);
            return true;
        }
        catch { return false; }
    }

    private static void CancelSarching()
    {
        Network.CloseWifiServerSearcher();
        ClearOpponentInfo();

        if (_listener != null)
        {
            _listener.Stop();
            _listener = null;
        }

        if (_streamGame != null)
        {
            _streamGame.Close();
            _streamGame = null;
        }

        OpponentStatus = null;
        //TODO: Отправить сообщение подключённом игроку на всякий случай
        //TODO: Вывести сообщение об отмене  подумать, всё ли обработал
    }

    public static void CancelWaiting() //TODO: Досрочный выход из поиска
    {
        _searching = false;
        CleanHostingUI?.Invoke();
    }

    private static void ClearOpponentInfo()
    {
        if (_opponent != null)
        {
            _opponent.Client.Close();
            _opponent = null;
        }
    }

    private static bool AddMessageToPlayerBuffer()
    {
        List<byte> buffer = new List<byte>();
        try
        {
            _streamGame = _opponent.Client.GetStream();
            if (_streamGame.DataAvailable)
            {
                while (_streamGame.DataAvailable)
                {
                    int ReadByte = _streamGame.ReadByte();
                    if (ReadByte != -1)
                    {
                        buffer.Add((byte)ReadByte);
                    }
                }
                _opponent.LastReciveTime = DateTime.UtcNow;
            }
            else return true;
        }
        catch { return false; }

        // Если пришло несколько сообщений, подели их на отдельные
        string[] message = (Encoding.UTF8.GetString(buffer.ToArray())).Split('|');
        // Удаляем последний пустой элемент (который по-любому будет после деления)
        List<string> messList = new List<string>(message);
        //TODO: Добавить проверку здесь и на клиенте, на целостность сообщения
        messList.Remove("");
        //TODO: Или не переводить в лист и искать тк тратишь много времении, нужно просто в конце цикл до length-1
        for (int i = 0; i < messList.Count; i++)
        {
            _opponent.TcpMessages.Add(messList[i]);
        }
        return true;
    }

    private static void TcpMessageHandler()
    {
        AddMessageToPlayerBuffer();

        if (!_messageHandlerIsBusy)
        {
            _messageHandlerIsBusy = true;
            while (DataHolder.MessageTCP.Count > 0)
            {
                string[] mes = DataHolder.MessageTCP[0].Split(' ');
                switch (mes[0])
                {

                    case "leave":

                        break;

                    case "exit":

                        break;

                    case "Check":

                        break;


                    default:
                        DataHolder.MessageTCPforGame.Add(DataHolder.MessageTCP[0]);
                        break;

                }
                DataHolder.MessageTCP.RemoveAt(0);
            }
            _messageHandlerIsBusy = false;
        }
    }

    //private static void LoginOrReconnecting(ClientInfo ChosenClient)
    //{
    //    ClientInfo found = AllClients.Find(item => item.PlayerId == ChosenClient.PlayerId);
    //    Console.WriteLine("eee");
    //    if (found != null && found.Client != ChosenClient.Client)
    //    {
    //        // Если игрок уже где-то подключён и это не тот же сокет, то отправлем прошлому, что он больше не в игре
    //        if (found.Client != ChosenClient.Client)
    //            SendMessage("_newLogin"); //TODO: Обработать на клиенте
    //                                      // Заменяем его экземпляр класса TcpClient, если зашёл с другого сокета
    //        found.ReplaceClient(ChosenClient.Client);
    //        SendMessage("login " + found.PlayerId + " " + found.Money);
    //        RemovingFromMain.Add(ChosenClient);

    //        if (found.ThisGameLobby != null)
    //        {
    //            SendMessage($"goto {found.ThisGameLobby.gameType} {found.ThisGameId} {found.ThisGameLobby.lobbyId}");
    //            SendMessage($"info {((UdpLobby_2)found.ThisGameLobby).GetInfo(found.ThisGameId)}");
    //        }
    //    }
    //    else
    //        SendMessage(ChosenClient, "login " + ChosenClient.PlayerId + " " + ChosenClient.Money);
    //}

    
}
