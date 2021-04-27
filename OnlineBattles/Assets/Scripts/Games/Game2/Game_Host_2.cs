using System;
using UnityEngine;

public class Game_Host_2 : GameTemplate_WifiHost
{
    [SerializeField] private Joystick joystick;
    [SerializeField] private GameObject me, enemy;

    private float x1 = -1.5f, y1 = 0.2f, x2 = 1.5f, y2 = 0.2f;


    private void Start()
    {
        BaseStart("udp");
        InvokeRepeating("GameProcess", 0f, WifiServer_Host.UpdateRate);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (_gameOn)
        {
            //TODO: Если не отпускать джойстик, то можно играть даже после включения уведомления и щита
            x1 += joystick.Horizontal / 30;
            y1 += joystick.Vertical / 30;
            GetMessage();

            me.transform.position = new Vector2(x1, y1);
            enemy.transform.position = new Vector2(x2, y2);
        }
    }

    public void GetMessage()
    {
        if (DataHolder.MessageUDPget.Count > 0)
        {
            if (!SplitFramesAndChechTrash())
                return;

            try
            {
                x2 += float.Parse(messege[1]) / 10;
                y2 += float.Parse(messege[2]) / 10;
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
                EndOfGame("drawn");
            else if (x1 < -3 || x1 > 3)
                EndOfGame("lose");
            else if (x2 < -3 || x2 > 3)
                EndOfGame("win");
        }
        else
            DataHolder.ClientUDP.SendMessage($"g {DateTime.UtcNow.Ticks} {x2} {y2} {x1} {y1}");
    }
}
