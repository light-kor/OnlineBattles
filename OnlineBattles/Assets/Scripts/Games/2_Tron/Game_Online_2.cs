﻿using System;
using UnityEngine;

namespace Game2
{
    public class Game_Online_2 : GameTemplate_Online
    {
        private GameResources_2 GR;
        private Vector2 _lastMove = Vector2.zero;

        private void Start()
        {
            GR = GameResources_2.GameResources;
            BaseStart(DataHolder.ConnectType.UDP);
        }

        protected override void Update()
        {
            base.Update();
            if (_gameOn)
            {
                //UpdateThread();
                //SendJoy();

                if (Network.TCPMessagesForGames.Count > 0)
                {
                    string[] mes = Network.TCPMessagesForGames[0].Split(' ');       
                    
                    if (mes[0] == "position")
                    {
                       
                    }
                    Network.TCPMessagesForGames.RemoveAt(0);
                }

                UpdateThread();
            }
        }

        private void UpdateThreadOld()
        {
            if (Network.UDPMessages.Count > 1)
            {
                long time = Convert.ToInt64(_frame[1]);
                long time2 = Convert.ToInt64(_frame2[1]);
                long vrem = DateTime.UtcNow.Ticks - Network.TimeDifferenceWithServer / 2 - _delay; //TODO: Так вроде нормально, но чёт нелогично
                                                                                                   //long vrem = DateTime.UtcNow.Ticks - _delay;

                if (vrem >= time2)
                {
                    Network.UDPMessages.RemoveAt(0);
                    UpdateThread(); // Чтоб в этом кадре тоже что-то показали
                }
                else if (time <= vrem && vrem < time2)
                {
                    //normalized = (x - min(x)) / (max(x) - min(x));
                    float delta = (vrem - time) / (time2 - time);

                //    GR._me.transform.position = Vector2.Lerp(new Vector2(float.Parse(_frame[2]), float.Parse(_frame[3])), new Vector2(float.Parse(_frame2[2]), float.Parse(_frame2[3])), delta);
                //    GR._enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(_frame[4]), float.Parse(_frame[5])), new Vector2(float.Parse(_frame2[4]), float.Parse(_frame2[5])), delta);
                }
                //else if (time > vrem) return; //По идее это бессмысленная строчка            
            }
        }

        private void UpdateThread()
        {
            if (Network.UDPMessages.Count > 0)
            {
                _frame = Network.UDPMessages[0].Split(' ');
                if (_frame[0] != "g")
                {
                    Network.UDPMessages.RemoveAt(0);
                    return;
                }

                //Vector2 myPosition = new Vector2(gg(_frame[2]), gg(_frame[3]));
                //Vector2 enemyPosition = new Vector2(gg(_frame[4]), gg(_frame[5]));

                //GR._me.transform.position = Vector2.MoveTowards(GR._me.transform.position, myPosition, 1.0f);
                //GR._enemy.transform.position = Vector2.MoveTowards(GR._enemy.transform.position, enemyPosition, 1.0f);

                //Debug.Log(DataHolder.MessageUDPget[0]);

                //GR._me.transform.position = myPosition;
                //GR._enemy.transform.position = enemyPosition;

                Network.UDPMessages.RemoveAt(0);
            }
        }

        private void SendChangeMazeRequest()
        {
            Network.ClientTCP.SendMessage("change");
        }


        public override void SendAllChanges()
        {
            //Vector2 move = new Vector2(GR._firstJoystick.Horizontal, GR._firstJoystick.Vertical);
            //if (move != _lastMove)
            //{
            //    Network.ClientTCP.SendMessage($"move {move.x} {move.y}");
            //    Debug.Log($"move {move.x} {move.y}");
            //    _lastMove = move;
            //}
        }
    }
}