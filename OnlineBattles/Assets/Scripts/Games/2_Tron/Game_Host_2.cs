using GameEnumerations;
using System.Globalization;
using UnityEngine;

namespace Game2
{
    public class Game_Host_2 : GameTemplate_WifiHost
    {       
        private GameResources_2 GR;
        private NumberFormatInfo _numberInfo = new CultureInfo("en-US").NumberFormat;

        private void Start()
        {
            GR = GameResources_2.GameResources;
            BaseStart(ConnectTypes.UDP);
            GR.SetControlTypes(ControlTypes.Local, ControlTypes.Remote);
        }

        protected override void Update()
        {
            base.Update();

            if (_gameOn)
            {
                if (WifiServer_Host.Opponent.MessageTCPforGame.Count > 0)
                {
                    string[] mes = WifiServer_Host.Opponent.MessageTCPforGame[0].Split(' ');
                    if (mes[0] == "move")
                    {
                        GR.RemoteJoystick.Add(new Vector2(-float.Parse(mes[1], _numberInfo), -float.Parse(mes[2], _numberInfo)));
                    }
                    WifiServer_Host.Opponent.MessageTCPforGame.RemoveAt(0);
                }

                //CheckEndOfGame();
            }
        }

        //public void CheckEndOfGame()
        //{
        //    if (GR._myPoints >= GR.WinScore || GR._enemyPoints >= GR.WinScore)
        //    {
        //        CloseAll();
        //        GR._myVelocity = Vector2.zero;
        //        GR._enemyVelocity = Vector2.zero;

        //        if (GR._myPoints == GR._enemyPoints)
        //            EndOfGame("drawn");
        //        else if (GR._myPoints > GR._enemyPoints)
        //            EndOfGame("lose");
        //        else if (GR._enemyPoints > GR._myPoints)
        //            EndOfGame("win");
        //    }
        //}

        protected override void SendFramesUDP()
        {
            if (_gameOn)
            {
                SendAllChanges();
            }
        }

        private void SendAllChanges()
        {
            GR.SendFrame();
        }
    }
}