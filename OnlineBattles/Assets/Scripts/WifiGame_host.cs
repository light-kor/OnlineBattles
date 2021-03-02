using System;
using UnityEngine;

public class WifiGame_host : MonoBehaviour
{
    public event DataHolder.GameNotification EndOfGameEvent;
    public Joystick joystick;
    public GameObject me, enemy;
    public float UpdateRate = 0.05f;
    private float x1 = -1.5f, y1 = 0.2f, x2 = 1.5f, y2 = 0.2f;

    private void Start()
    {
        WifiServer_Host.ClientDisconnect += EndOfGame;
        Network.CreateUDP();
        DataHolder.MessageUDPget.Clear();
        DataHolder.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
        InvokeRepeating("GameProcess", 0f, UpdateRate);
    }

    private void Update()
    {
        x1 += joystick.Horizontal / 30;
        y1 += joystick.Vertical / 30;
        GetMessage();

        me.transform.position = new Vector2(x1, y1);
        enemy.transform.position = new Vector2(x2, y2);
    }


    private void GiveUp()
    {
        EndOfGame("win");
    }

    private void CloseAll()
    {
        CancelInvoke();
        Network.CloseUdpConnection();
    }

    public void GetMessage()
    {
        if (DataHolder.MessageUDPget.Count > 0)
        {
            string[] words = DataHolder.MessageUDPget[0].Split(' ');
            if (words[0] != "g")
            {
                DataHolder.MessageUDPget.RemoveAt(0);
                return;
            }
                
            try
            {
                x2 += float.Parse(words[1]) / 10;
                y2 += float.Parse(words[2]) / 10;
            }
            catch { }
            DataHolder.MessageUDPget.RemoveAt(0);
        }
    }


    public void GameProcess()
    {
        // Чекаем конец игры
        if (x1 < -3 || x1 > 3 || x2 < -3 || x2 > 3)
        {
            if ((x1 < -3 || x1 > 3) && (x2 < -3 || x2 > 3))
                EndOfGame("drawn");
            else if (x1 < -3 || x1 > 3)
                EndOfGame("lose");
            else if (x2 < -3 || x2 > 3)
                EndOfGame("win");
        }
        DataHolder.ClientUDP.SendMessage($"g {DateTime.UtcNow.Ticks} {x2} {y2} {x1} {y1}");
    }

    public void EndOfGame(string opponentStatus)
    {
        WifiServer_Host.SendTcpMessage(opponentStatus);
        Debug.Log(opponentStatus);
        if (opponentStatus == "drawn")
        {            
            EndOfGameEvent?.Invoke(opponentStatus, 1);
        }
        else if (opponentStatus == "lose")
        {
            EndOfGameEvent?.Invoke("lose", 1);
        }
        else if (opponentStatus == "win")
        {
            EndOfGameEvent?.Invoke("lose", 1);
        }

        //TODO: На клиенте сработает завершение игры для мультиплеера, надо настроить под wifi
    }

    
}
