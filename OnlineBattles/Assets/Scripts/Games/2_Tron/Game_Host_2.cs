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
        }

        protected override void CreateUDPFrame()
        {
            FrameInfo data = new FrameInfo(GR.Blue, GR.Red);
            Serializer<FrameInfo>.SendMessage(data, ConnectTypes.UDP);
        }

        private void ProcessingTCPMessages(string[] mes)
        {
            if (mes[0] == "move")
            {
                myVector2 joy = JsonUtility.FromJson<myVector2>(mes[1]);
                GR.Red.PlayerInput.ChangeDirectionRemote(joy.GetVector2());
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