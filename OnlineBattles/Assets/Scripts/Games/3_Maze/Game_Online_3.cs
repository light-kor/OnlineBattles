using System.Collections.Generic;
using UnityEngine;

namespace Game3
{
    public class Game_Online_3 : GameTemplate_Online
    {
        [SerializeField] private Joystick _joystick;
        [SerializeField] private PointsSpawner _points;

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
            if (Network.MessagesUDP.Count > 0)
            {
                FrameInfo frame = null;
                try
                {
                    frame = Serializer<FrameInfo>.GetMessage(Network.MessagesUDP[0]);
                }
                catch
                {
                    frame = null;
                }

                if (frame != null)
                    _frames.Add(frame);

                Network.MessagesUDP.RemoveAt(0);              
            }
        }

        private void ProcessingTCPMessages(string[] mes)
        {
            if (mes[0] == "point")
            {
                int number = int.Parse(mes[1]);
                myVector2 position = JsonUtility.FromJson<myVector2>(mes[2]);

                _points.SetPointPositionRemote(number, position.GetVector2());
            }
            else if (mes[0] == "position")
            {
                FrameInfo frame = JsonUtility.FromJson<FrameInfo>(mes[1]);

                Vector3 pos_blue = frame.Blue.GetVector3();
                Vector3 pos_red = frame.Red.GetVector3();

                GR.Blue.SetBroadcastPositions(pos_blue);
                GR.Red.SetBroadcastPositions(pos_red);
            }
        }

        private void SendJoystick()
        {
            Vector2 move = new Vector2(_joystick.Horizontal, _joystick.Vertical).normalized;
            if (move != _lastMove)
            {
                _lastMove = move;
                myVector2 joy = new myVector2(move);
                string json = JsonUtility.ToJson(joy);
                Network.ClientTCP.SendMessage($"move {json}");
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
            if (Network.ClientTCP != null)
                Network.ClientTCP.BigMessageReceived -= GetBigMessage;

            ManualCreateMapButton.Click -= SendChangeMazeRequest;
            GR.NewMessageReceived -= ProcessingTCPMessages;
        }
    }
}
