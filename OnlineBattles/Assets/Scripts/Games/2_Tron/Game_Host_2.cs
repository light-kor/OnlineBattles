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
            Score.RoundWinner += SendPlayerExplosion;
            GR.NewMessageReceived += ProcessingTCPMessages;
            BaseStart(ConnectTypes.UDP);
        }

        protected override void SendFramesUDP()
        {
            if (GR.GameOn)
            {
                if (Network.ClientUDP != null)
                {
                    FrameInfo data = new FrameInfo(GR.Blue, GR.Red);
                    Serializer<FrameInfo>.SendMessage(data, ConnectTypes.UDP);
                }
            }
        }

        private void ProcessingTCPMessages(string[] mes)
        {
            if (mes[0] == "move")
            {
                Vector2 normalizedJoystick = new Vector2(-float.Parse(mes[1], GR.NumFormat), -float.Parse(mes[2], GR.NumFormat));
                GR.Red.PlayerInput.ChangeDirectionRemote(normalizedJoystick);
            }
        }

        private void SendPlayerExplosion(PlayerTypes player)
        {
            WifiServer_Host.Opponent.SendTcpMessage($"Explosion {player}");
        }

        private void OnDestroy()
        {
            Score.RoundWinner -= SendPlayerExplosion;
            GR.NewMessageReceived -= ProcessingTCPMessages;
        }
    }
}