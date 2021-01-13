using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPConnect
{
    public bool GameOn { get; set; } = false;      
    private UdpClient client { get; set; }
    private IPEndPoint remoteIp = null;

    //TODO: Для всех ЮДП сообщений нужна структура: номер игры - номер лобби id - !свой id в бд! - номер игрока в лобби - сообщение
    public UDPConnect()
    {        
        GameOn = true;
        client = new UdpClient(DataHolder.ConnectIp, DataHolder.RemotePort);

        Thread receiveThread = new Thread(ReceivingMessagesLoop);
        receiveThread.Start();
        receiveThread.IsBackground = true;
    }

    /// <summary>
    /// Отправка пользовательских UDP сообщений с добавлением "метаданных".
    /// </summary>
    /// <param name="mes">Текст сообщения.</param>
    public void SendMessage(string mes)
    {
        try
        {       
            byte[] data = Encoding.UTF8.GetBytes($"{DataHolder.SelectedServerGame} {DataHolder.LobbyID} {DataHolder.IDInThisGame} " + mes);
            client.Send(data, data.Length);
        }
        catch { TryReconnect(); }
    }

    /// <summary>
    /// Цикл приёма UDP сообщений и помещение их в MessageUDPget.
    /// </summary>
    private void ReceivingMessagesLoop()
    {
        while (GameOn)
        {
            try
            {
                byte[] data = client.Receive(ref remoteIp);
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
        CloseClient();

        if (GameOn)
        {
            client = new UdpClient(DataHolder.ConnectIp, DataHolder.RemotePort);
            GameOn = true;
            //TODO: Игрок может отменить реконнект и игру, тогда надо будет обнулить и удалить все UDP соединения
        }
    }

    /// <summary>
    /// Остановка всех UDP процессов. GameOn становится false, client уничтожается.
    /// </summary>
    public void CloseClient()
    {
        if (client != null)
        {
            GameOn = false;
            client.Close();
            client = null;
        }
    }

    ~UDPConnect()
    {
        CloseClient();
    }
}
