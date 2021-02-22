using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class Opponent_Info
{
    public TcpClient Client { get; private set; }
    public IPEndPoint UdpEndPoint { get; set; } = null;
    public string PlayerName { get; set; } = null;
    public DateTime LastReciveTime { get; set; }
    public List<string> TcpMessages { get; set; } = new List<string>();
    public List<string> TcpGameMessages { get; set; } = new List<string>();
    public long Ping { get; private set; } = 0;
    private long StartPingTimeInTicks { get; set; } = 0;

    public Opponent_Info(TcpClient client, DateTime time)
    {
        Client = client;
        LastReciveTime = time;
    }

    ///// <summary>
    ///// Обработка сообщений клиента на наличие запросов на старт UDP-игры и вычисления пинга.
    ///// </summary>
    ///// <param name="words">Полученное от игрока сообщение.</param>
    //public void StartGameAndCheckPing(string[] words)
    //{
    //    if (words[0] == "ping") //TODO: А если юдп сообщение не доёдёт? надо ещё раз попробовать или через тсп настроить
    //    {
    //        Ping = DateTime.UtcNow.Ticks - StartPingTimeInTicks;
    //        StartPingTimeInTicks = 0;
    //        TcpProcessing.SendMessage(this, $"time {DateTime.UtcNow.Ticks + (Ping / 2)}"); //TODO: Нужно ли пинг прибавить?      
    //    }
    //    else if (words[0] == "start")
    //    {
    //        StartPingTimeInTicks = DateTime.UtcNow.Ticks;
    //        TcpProcessing.SendMessage(this, "ping");
    //    }
    //}

    ///// <summary>
    ///// Обновляет TcpClient клиента, если он переподключился с другого IPEndPoint и хочет вернуться.
    ///// </summary>
    ///// <param name="client">Новый TcpClient.</param>
    //public void ReplaceClient(TcpClient client)
    //{
    //    Client = client;
    //}

    ///// <summary>
    ///// Устанавливает ID игрока в системе и количество его денег.
    ///// </summary>
    ///// <param name="id">ID игрока из базы данных.</param>
    ///// <param name="money">Деньги игрока из базы данных.</param>
    //public void SetPlayerIdAndMoney(int id, int money)
    //{
    //    PlayerId = id;
    //    Money = money;
    //}

    //public void SetLobbyInfo(LobbyInfo lob, int gameId)
    //{
    //    ThisGameLobby = lob;
    //    ThisGameId = gameId;
    //}

    //public void SetLobbyInfoNull()
    //{
    //    ThisGameLobby = null;
    //    ThisGameId = 0;
    //    ChosenGame = null;
    //}

}
