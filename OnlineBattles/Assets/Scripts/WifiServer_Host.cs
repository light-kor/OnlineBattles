using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class WifiServer_Host
{  
    private const int UpdateRate = 50; // Отправка UDP инфы каждые UpdateRate мс    
    private static TcpListener _listener { get; set; } = null;
    private static NetworkStream _streamGame { get; set; } = null;

    public static Opponent_Info _opponent = null;

    public static event DataHolder.GameNotification FoundOnePlayer;
    public static event DataHolder.Notification CleanHostingUI;
    public static event DataHolder.Notification AcceptOpponent;
    public static string OpponentStatus = null;

    private static bool _searching;

    public static async void StartHosting()
    {
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
                    AcceptOpponent?.Invoke();
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

    //public void StartServer()
    //{
    //    Thread receiveThread = new Thread(UdpProcessing.ReceivingMessagesLoop);
    //    receiveThread.Start();

    //    //Создаём таймер для отправки данных игрокам UDP сервера
    //    TimerCallback SendUdp = new TimerCallback(UdpProcessing.ResendUDPMessage);
    //    Timer UdpSenderTimer = new Timer(SendUdp, null, 0, UpdateRate);

    //    while (true)
    //    {
    //        TcpMessageHandling();
    //        //TicTacToeLobby();
    //    }
    //}

    ///// <summary>
    ///// Обработка новых игроков. Добавляем из в лист MainClientsList из NewClients, далее:
    ///// авторизация, регистрация, коннект к какой-либо игре или дисконнект.
    ///// </summary>
    //private void TcpMessageHandling()
    //{

    //    // Проверяем поток клиента
    //    if (!AddMessageToPlayerBuffer())
    //    {
    //        RemovingFromMain.Add(client); // Удаляем, если потеряли соединение с сокетом. //TODO: Сделать реконнект
    //    }

    //    bool chooselvl = false;
    //    while (client.TcpMessages.Count > 0)
    //    {
    //        string[] mes = client.TcpMessages[0].Split(new char[] { ' ' });

    //        // Выбор игры.
    //        if (mes[0] == "game")
    //        {
    //            if (client.ChosenGame == null)
    //            {
    //                if (mes[1] == "1")
    //                    client.ChosenGame = forGame1;
    //                else if (mes[1] == "2")
    //                    client.ChosenGame = forGame2;
    //                chooselvl = true;
    //            }
    //        }
    //        // Проверка пинга и старт игры
    //        else if (mes[0] == "ping" || mes[0] == "start") { client.StartGameAndCheckPing(mes); }
    //        // Выход из начавшейся игры.
    //        else if (mes[0] == "leave") { client.ThisGameLobby.EndOfGame(client); }
    //        // Авторизация в системе. Принимаем keyid и возвращаем Игроку его ID и Money
    //        else if (mes[0] == "login") { SearchForARegisteredUser(client, mes); } //TODO: Сначала он должен залогиниться, а потом уже всё остальное
    //                                                                               // Регистрация новых пользователей
    //        else if (mes[0] == "reg")
    //        {
    //            if (client.PlayerId != 0)
    //                continue;

    //            //                                                 
    //        }
    //        // Отмена поиска игры
    //        else if (mes[0] == "CancelSearch") { client.ChosenGame = null; }
    //        // Вышел из приложения через кнопку
    //        else if (mes[0] == "exit") { RemovingFromMain.Add(client); }
    //        // Поддержание жизни.
    //        else if (mes[0] == "Check") { }
    //        // Значит сообщение идёт в его игру
    //        else { client.TcpGameMessages.Add(client.TcpMessages[0]); }

    //        client.TcpMessages.RemoveAt(0);
    //    }
    //    if (chooselvl)
    //        client.ChosenGame.Add(client);

    //}

    ///// <summary>
    ///// Ищем игрока среди всех подключённых пользователей.
    ///// Логинем, отказваем во втором логине или востанавливаем Client, в случае необходимости.
    ///// </summary>
    //private void LoginOrReconnecting(ClientInfo ChosenClient)
    //{
    //    ClientInfo found = AllClients.Find(item => item.PlayerId == ChosenClient.PlayerId);
    //    Console.WriteLine("eee");
    //    if (found != null && found.Client != ChosenClient.Client)
    //    {
    //        // Если игрок уже где-то подключён и это не тот же сокет, то отправлем прошлому, что он больше не в игре
    //        if (found.Client != ChosenClient.Client)
    //            SendMessage("_newLogin"); //TODO: Обработать на клиенте
    //                                                           // Заменяем его экземпляр класса TcpClient, если зашёл с другого сокета
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


    //public static void SplitByLobby(List<ClientInfo> players, int GameType)
    //{
    //    List<ClientInfo> buff = new List<ClientInfo>();

    //    // Если нечётное кол-во игроков, то отправляем последнего в следующую партию
    //    if (players.Count % 2 != 0)
    //    {
    //        buff.Add(players[players.Count - 1]);
    //        players.RemoveAt(players.Count - 1);
    //    }

    //    //Распределяем игроков по лобби
    //    for (int i = 0; i < players.Count; i += 2)
    //    {
    //        bool one, two;
    //        // Разрешить клиентам начать игру и кидаем их номер в этом матче
    //        one = TcpProcessing.SendMessage(players[i], $"S 1 {IdLobbyCount}");
    //        two = TcpProcessing.SendMessage(players[i + 1], $"S 2 {IdLobbyCount}");

    //        // Проверить, отправилось ли сообщение
    //        if (one && two)
    //        {
    //            if (GameType == 1)
    //                Lobby_game1.Add(new LobbyInfo(players[i], players[i + 1], IdLobbyCount, DateTime.UtcNow, 1, 100));
    //            else if (GameType == 2)
    //                Lobby_game2.Add(new UdpLobby_2(players[i], players[i + 1], IdLobbyCount, DateTime.UtcNow, GameType, 100));
    //            IdLobbyCount++;
    //        }
    //        else //TODO: Переосмыслить и перенастроить весь этот блок
    //        {
    //            // Если не получилось отправить кому-то сообщение, то его удаляем, а второй продолжает ждать новое лобби. Или обоих удалим  
    //            if (one)
    //            {
    //                TcpProcessing.SendMessage(players[i], "R");
    //                TcpProcessing.SendMessage(players[i + 1], "C");
    //                buff.Add(players[i]);
    //            }
    //            else if (two)
    //            {
    //                TcpProcessing.SendMessage(players[i + 1], "R");
    //                TcpProcessing.SendMessage(players[i], "C");
    //                buff.Add(players[i + 1]);
    //            }
    //            else
    //            {
    //                TcpProcessing.SendMessage(players[i], "C");
    //                TcpProcessing.SendMessage(players[i + 1], "C");
    //            }
    //        }
    //    }
    //    // Чистим лист этой игры, когда все зашли по лобби
    //    players.Clear();
    //    // Возвращаем одиночек на следующий круг
    //    players.AddRange(buff);
    //}
}
