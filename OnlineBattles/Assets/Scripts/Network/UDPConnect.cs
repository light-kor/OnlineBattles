using GameEnumerations;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPConnect
{
    private IPEndPoint _remoteIp = null;
    private UdpClient _client = null;
    private Thread _receiveThread = null;
    private bool _working = true;
    private string _ip = null;
    private int _port = 0;

    //TODO: Для всех ЮДП сообщений для сервера нужна структура: номер игры - номер лобби id - !свой id в бд! - номер игрока в лобби - сообщение
    public UDPConnect()
    {
        CreateClass();
    }

    /// <summary>
    /// Создание подключения в зависимости от типа игры.
    /// </summary>
    private void CreateClass()
    {
        _working = true;
        if (DataHolder.GameType == GameTypes.Multiplayer)
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
    /// Отправка пользовательских UDP сообщений с добавлением "метаданных" для сервера.
    /// </summary>
    /// <param name="mes">Текст сообщения.</param>
    public void SendMessage(string mes)
    {
        byte[] data = null;
        try
        {
            if (DataHolder.GameType == GameTypes.Multiplayer)
                data = Encoding.UTF8.GetBytes($"{DataHolder.SelectedServerGame} {DataHolder.LobbyID} {DataHolder.IDInThisGame} " + mes);
            else
                data = Encoding.UTF8.GetBytes(mes);

            _client.Send(data, data.Length);
        }
        catch { TryReconnect(); }
    }

    public void SendMessage(byte[] data)
    {
        try
        {
            _client.Send(data, data.Length);
        }
        catch { TryReconnect(); }
    }

    /// <summary>
    /// Цикл приёма UDP сообщений и помещение их в DataHolder.MessageUDPget.
    /// </summary>
    private void ReceivingMessagesLoop()
    {
        while (_working)
        {
            try
            {
                byte[] data = _client.Receive(ref _remoteIp);
                //string messList = Encoding.UTF8.GetString(data);
                //Network.UDPMessages.Add(messList);
             
                Network.MessagesUDP.Add(data);
            }
            catch { TryReconnect(); }
        }
    }
    
    /// <summary>
    /// Уничтожение старого, и если игра ещё не завершилась, создание нового экземпляра client.
    /// </summary>
    private void TryReconnect()
    {
        if (_working == true)
        {
            CloseAll();
            CreateClass();
        }
        else CloseAll();
    }

    /// <summary>
    /// Остановка всех UDP процессов. GameOn становится false, client уничтожается.
    /// </summary>
    public void CloseAll()
    {
        _working = false;

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
