using System;
using UnityEngine;

public class Game_Online_2 : GameTemplate_Online
{
    
    public Joystick joystick;
    public GameObject me, enemy;
    public float UpdateRate = 0.05f; //TODO: Как часто клиенты должны слать свои изменения. Надо  как-то чекать это на стороне сервра. Чтоб нельзя было так читерить.
    private float buffX = 0, buffY = 0;
    
    
    protected override void Start()
    {
        base.Start();
        InvokeRepeating("SendJoy", 0.5f, UpdateRate);

    }


    protected override void Update()
    {
        base.Update();

        //TODO: Это для мультплеера, для wifi бесполезно
        //if (DataHolder.MessageTCPforGame.Count > 0 && _gameOn)
        //{
        //    string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');
        //    if (mes[0] == "info")
        //    {
        //        me.transform.position = new Vector2(float.Parse(mes[2]), float.Parse(mes[3]));
        //        enemy.transform.position = new Vector2(float.Parse(mes[4]), float.Parse(mes[5]));
        //    }
                       
        //    DataHolder.MessageTCPforGame.RemoveAt(0);
        //}
        
        UpdateThread();
    }

    
    private void UpdateThread()
    {
        if (DataHolder.MessageUDPget.Count > 1 && _gameOn)
        {
            if (!SplitFramesAndChechTrash())
                return;

            long time = Convert.ToInt64(frame[1]);
            long time2 = Convert.ToInt64(frame2[1]);
            //long vrem = DateTime.UtcNow.Ticks + DataHolder.TimeDifferenceWithServer - _delay;
            long vrem = DateTime.UtcNow.Ticks + DataHolder.TimeDifferenceWithServer;

            if (time < vrem && vrem < time2)
            {
                //normalized = (x - min(x)) / (max(x) - min(x));
                float delta = (vrem - time) / (time2 - time);
                me.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame[2]), float.Parse(frame[3])), new Vector2(float.Parse(frame2[2]), float.Parse(frame2[3])), delta);
                enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame[4]), float.Parse(frame[5])), new Vector2(float.Parse(frame2[4]), float.Parse(frame2[5])), delta);
            }
            else if (time > vrem) return;

            DataHolder.MessageUDPget.RemoveAt(0);
        }
    }

    void SendJoy()
    {
        buffX = joystick.Horizontal;
        buffY = joystick.Vertical;
        if (buffX != 0 && buffY != 0)
        {
            DataHolder.ClientUDP.SendMessage($"g {buffX} {buffY}"); //TODO: Проверять на сервере, что число от 0 до 1           
        }
    }
}