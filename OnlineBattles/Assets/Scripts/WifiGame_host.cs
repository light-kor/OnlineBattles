using System;
using UnityEngine;

public class WifiGame_host : MonoBehaviour
{
    
    public Joystick joystick;
    public GameObject me, enemy;
    private float UpdateRate = 0.05f;
    private float x1 = -1.5f, y1 = 0.2f, x2 = 1.5f, y2 = 0.2f;
    private bool _finishTheGame = false;

    private void Start()
    {
        WifiServer_Host.OpponentLeaveTheGame += FinishGame;
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

        if (_finishTheGame)
        {
            _finishTheGame = false;
            CloseAll();
            WifiServer_Host.EndOfGame("lose");           
        }
    }


    public void GiveUp()
    {
        CloseAll();
        WifiServer_Host.EndOfGame("win");
    }

    public void FinishGame()
    {
        _finishTheGame = true;
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
            CloseAll();

            if ((x1 < -3 || x1 > 3) && (x2 < -3 || x2 > 3))
                WifiServer_Host.EndOfGame("drawn");
            else if (x1 < -3 || x1 > 3)
                WifiServer_Host.EndOfGame("lose");
            else if (x2 < -3 || x2 > 3)
                WifiServer_Host.EndOfGame("win");
        }
        else
            DataHolder.ClientUDP.SendMessage($"g {DateTime.UtcNow.Ticks} {x2} {y2} {x1} {y1}");
    }

    

    
}
