using GameEnumerations;
using UnityEngine;

namespace Game2
{
    public class Game_Host_2 : GameTemplate_WifiHost
    {       
        private GameResources_2 GR;

        private void Start()
        {
            GR = GameResources_2.GameResources;
            BaseStart(ConnectTypes.UDP);
            GR.SetControlTypes(ControlTypes.Local, ControlTypes.Remote);
        }

        private void Update()
        {
            if (GR.GameOn)
            {
                if (GR.GameMessagesCount > 0)
                {
                    string[] mes = GR.UseAndDeleteGameMessage();
                    if (mes[0] == "move")
                    {
                        Vector2 normalizedJoystick = new Vector2(-float.Parse(mes[1], GR.NumFormat), -float.Parse(mes[2], GR.NumFormat));
                        GR.Red.PlayerMover.ChangeDirection(normalizedJoystick);
                    }
                }
            }
        }

        protected override void SendFramesUDP()
        {
            if (GR.GameOn)
            {
                SendAllChanges();
            }
        }

        private void SendAllChanges()
        {
            if (Network.ClientUDP != null)
            {
                FrameInfo data = new FrameInfo(GR.Blue, GR.Red);
                Serializer<FrameInfo>.SendMessage(data, ConnectTypes.UDP);
            }
        }
    }
}