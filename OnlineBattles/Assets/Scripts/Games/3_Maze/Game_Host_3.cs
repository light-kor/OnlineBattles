using GameEnumerations;
using UnityEngine;

namespace Game3
{
    public class Game_Host_3 : GameTemplate_WifiHost
    {
        private GameResources_3 GR;

        private void Start()
        {
            GR = GameResources_3.GameResources;
            GR.NewMessageReceived += ProcessingTCPMessages;
            StartOnHost();           
        }

        protected override void CreateUDPFrame()
        {
            FrameInfo frame = new FrameInfo(GR.Blue, GR.Red);
            Serializer<FrameInfo>.SendMessage(frame, ConnectTypes.UDP);
        }

        private void ProcessingTCPMessages(string[] mes)
        {
            if (mes[0] == "move")
            {
                myVector2 joy = JsonUtility.FromJson<myVector2>(mes[1]);
                GR.Red.ChangeDirectionRemote(joy.GetVector2());             
            }
            else if (mes[0] == "change")
            {
                GR.Spawner.CreateMaze();                
            }
        }

        public void StartOnHost()
        {
            GR.StartOnLocal();

            FrameInfo frame = new FrameInfo(GR.Blue, GR.Red);
            string jsonFrame = JsonUtility.ToJson(frame);
            WifiServer_Host.Opponent.SendTcpMessage($"position {jsonFrame}");
        }

        private void OnDestroy()
        {
            GR.NewMessageReceived -= ProcessingTCPMessages;
        }
    }
}
