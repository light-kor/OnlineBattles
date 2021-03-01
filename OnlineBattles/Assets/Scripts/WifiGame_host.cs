using System;
using UnityEngine;

public class WifiGame_host : MonoBehaviour
{
    public event DataHolder.GameNotification EndOfGameEvent;
    public Joystick joystick;
    public GameObject me, enemy;
    public float UpdateRate = 0.05f;
    private float x1 = -1.5f, y1 = 0.2f, x2 = 1.5f, y2 = 0.2f;
    Opponent_Info client = WifiServer_Host._opponent;

    private void Start()
    {     
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
        EndOfGame(2);
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
                EndOfGame(-1);
            else if (x1 < -3 || x1 > 3)
                EndOfGame(1);
            else if (x2 < -3 || x2 > 3)
                EndOfGame(2);
        }
        CheckDisconnect();
        DataHolder.ClientUDP.SendMessage($"g {DateTime.UtcNow.Ticks} {x2} {y2} {x1} {y1}");
    }

    public void EndOfGame(int winClient)
    {
        if (winClient == -1)
        {
            SendMessage("drawn");
            EndOfGameEvent?.Invoke("drawn", 1);
        }
        else if (winClient == 1)
        {
            SendMessage("lose");
            EndOfGameEvent?.Invoke("lose", 1);
        }
        else if (winClient == 2)
        {
            SendMessage("win");
            EndOfGameEvent?.Invoke("win", 1);
        }
    }

    public void CheckDisconnect()
    {
        if ((DateTime.UtcNow - client.LastReciveTime).TotalSeconds > 5)
            EndOfGame(1); //TODO: Настроить и время и действия, а то хз, правильно так или добавить ещё ожидание и дать время на реконнект
    }
}
