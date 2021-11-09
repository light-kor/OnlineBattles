using GameEnumerations;
using System.Collections.Generic;

namespace Game4
{
    public class Game_Online_4 : GameTemplate_Online
    {
        private GameResources_4 GR;
        private List<FrameInfo> _frames = new List<FrameInfo>();

        private void Start()
        {
            GR = GameResources_4.GameResources;
            GR.NewMessageReceived += ProcessingTCPMessages;
            BaseStart(ConnectTypes.UDP);
        }

        private void Update()
        {
            if (GR.GameOn)
            {
                UpdateThread();
                //MoveToPosition();
            }
        }

        private void UpdateThread()
        {
            if (Network.UDPMessagesBig.Count > 0)
            {
                FrameInfo frame = Serializer<FrameInfo>.GetMessage(Network.UDPMessagesBig[0]);
                Network.UDPMessagesBig.RemoveAt(0);

                _frames.Add(frame);
            }
        }

        //private void MoveToPosition()
        //{
        //    Vector3 pos_blue = _frames[0].Blue.GetPosition();
        //    Vector3 pos_red = _frames[0].Red.GetPosition();

        //    Quaternion rot_blue = _frames[0].Blue.GetRotation();
        //    Quaternion rot_red = _frames[0].Red.GetRotation();

        //    GR.Blue.PlayerMover.SetBroadcastPositions(pos_blue, rot_blue);
        //    GR.Red.PlayerMover.SetBroadcastPositions(pos_red, rot_red);

        //    _frames.RemoveAt(0);
        //}

        //private void SendJoystick()
        //{
        //    if (GR.GameOn)
        //    {
        //        Vector2 move = new Vector2(_joystick.Horizontal, _joystick.Vertical).normalized;
        //        if (move != _lastMove)
        //        {
        //            Network.ClientTCP.SendMessage($"move {move.x} {move.y}");
        //            _lastMove = move;
        //        }
        //    }
        //}

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