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
            GR.StartOnHost();           
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
                var dir = new Vector2(float.Parse(mes[1], GR.NumFormat), float.Parse(mes[2], GR.NumFormat));
                GR.Red.ChangeDirectionRemote(dir);             
            }
            else if (mes[0] == "change")
            {
                GR.Spawner.CreateMaze();                
            }
        }
      
        private void OnDestroy()
        {
            GR.NewMessageReceived -= ProcessingTCPMessages;
        }
    }
}
