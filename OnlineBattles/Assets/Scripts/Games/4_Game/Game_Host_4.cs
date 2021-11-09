using GameEnumerations;
using UnityEngine;

namespace Game4
{
    public class Game_Host_4 : GameTemplate_WifiHost
    {
        private GameResources_4 GR;

        private void Start()
        {
            GR = GameResources_4.GameResources;
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

            }
        }

        private void OnDestroy()
        {
            GR.NewMessageReceived -= ProcessingTCPMessages;
        }
    }
}