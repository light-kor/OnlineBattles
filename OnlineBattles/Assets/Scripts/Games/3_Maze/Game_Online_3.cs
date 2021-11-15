using System.Collections.Generic;
using UnityEngine;

namespace Game3
{
    public class Game_Online_3 : GameTemplate_Online
    {
        [SerializeField] private Joystick _joystick;
        private bool _getBigMessage = false;
        private Vector2 _lastMove = Vector2.zero;
        private List<FrameInfo> _frames = new List<FrameInfo>();
        private GameResources_3 GR;

        private void Start()
        {
            Network.ClientTCP.BigMessageReceived += GetBigMessage;
            ManualCreateMapButton.Click += SendChangeMazeRequest;

            GR = GameResources_3.GameResources;
            GR.NewMessageReceived += ProcessingTCPMessages;
            GR.Spawner.MazeColliderSwitch(false);
        }

        private void FixedUpdate()
        {
            if (GeneralController.GameOn)
                SendJoystick();
        }

        private void Update()
        {
            if (_getBigMessage)
            {
                GR.Spawner.CreateMazeRemote();
                _getBigMessage = false;
            }

            if (GeneralController.GameOn)
            {
                UDPFramesProccesing();
                MoveToPosition();             
            }
        }

        private void MoveToPosition()
        {
            if (_frames.Count > 0)
            {
                Vector3 pos_blue = _frames[0].Blue.GetVector3();
                Vector3 pos_red = _frames[0].Red.GetVector3();

                GR.Blue.SetBroadcastPositions(pos_blue);
                GR.Red.SetBroadcastPositions(pos_red);

                _frames.RemoveAt(0);
            }
        }

        private void UDPFramesProccesing()
        {
            if (Network.UDPMessagesBig.Count > 0)
            {
                FrameInfo frame = Serializer<FrameInfo>.GetMessage(Network.UDPMessagesBig[0]);
                Network.UDPMessagesBig.RemoveAt(0);

                _frames.Add(frame);
            }
        }

        private void ProcessingTCPMessages(string[] mes)
        {
            if (mes[0] == "point")
            {
                Vector2 position = new Vector2(float.Parse(mes[1], GR.NumFormat), float.Parse(mes[2], GR.NumFormat));
               // Instantiate(GR._pointPref, position, Quaternion.identity, GR._points.transform);
            }
            else if (mes[0] == "position")
            {
                Vector2 redPosition = new Vector2(float.Parse(mes[1], GR.NumFormat), float.Parse(mes[2], GR.NumFormat));
                Vector2 bluePosition = new Vector2(float.Parse(mes[3], GR.NumFormat), float.Parse(mes[4], GR.NumFormat));

                GR.Blue.SetBroadcastPositions(bluePosition);
                GR.Red.SetBroadcastPositions(redPosition);
            }
        }

        private void SendJoystick()
        {
            Vector2 move = new Vector2(_joystick.Horizontal, _joystick.Vertical).normalized;
            if (move != _lastMove)
            {
                Network.ClientTCP.SendMessage($"move {move.x} {move.y}");
                _lastMove = move;
            }
        }

        private void SendChangeMazeRequest()
        {
            Network.ClientTCP.SendMessage("change");
        }

        private void GetBigMessage()
        {
            _getBigMessage = true;
        }

        private void OnDestroy()
        {
            Network.ClientTCP.BigMessageReceived -= GetBigMessage;
            ManualCreateMapButton.Click -= SendChangeMazeRequest;
            GR.NewMessageReceived -= ProcessingTCPMessages;
        }
    }
}
