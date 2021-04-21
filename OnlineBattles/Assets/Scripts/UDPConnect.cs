using System.Net;
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
        CreateClass();
    }

    private void CreateClass()
    {
        Working = true;
        if (DataHolder.GameType == "Multiplayer")
        {
            _ip = DataHolder.ServerIp;
            _port = DataHolder.RemoteServerPort;
            _client = new UdpClient(_ip, _port);
        }
        else
        {
            _ip = DataHolder.WifiGameIp;
            _port = DataHolder.WifiPort;
            _client = new UdpClient(_port);
            _client.Connect(_ip, _port);
        }

        _receiveThread = new Thread(ReceivingMessagesLoop);
        _receiveThread.Start();
    }

    /// <summary>
    /// Отправка пользовательских UDP сообщений с добавлением "метаданных".
    /// </summary>
    /// <param name="mes">Текст сообщения.</param>
    public void SendMessage(string mes)
    {
        byte[] data = null;
        try
        {
            if (DataHolder.GameType == "Multiplayer")
                data = Encoding.UTF8.GetBytes($"{DataHolder.SelectedServerGame} {DataHolder.LobbyID} {DataHolder.IDInThisGame} " + mes);
            else
                data = Encoding.UTF8.GetBytes(mes);

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
        if (Working == true)
        {
            CloseAll();
            CreateClass();
        }
        else CloseAll(); // На всякий случай

        //TODO: Игрок может отменить реконнект и игру, тогда надо будет обнулить и удалить все UDP соединения
        //TODO: Сделать отдельную функцию выхода в меню, если ты потерял связь во время игры и не хочешь реконнкта
    }

    /// <summary>
    /// Остановка всех UDP процессов. GameOn становится false, client уничтожается.
    /// </summary>
    public void CloseAll()
    {
        Working = false;

        if (_client != null)
        {
            _client.Close();
            _client = null;
        }
    }

    ~UDPConnect()
    {
        CloseAll();
    }
}
