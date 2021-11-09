using GameEnumerations;
using System.Collections.Generic;
using UnityEngine;

namespace Game2
{
    public class Game_Online_2 : GameTemplate_Online
    {       
        [SerializeField] private Joystick _joystick;

        private GameResources_2 GR;
        private Vector2 _lastMove = Vector2.zero;
        private List<FrameInfo> _frames = new List<FrameInfo>();
        private float _frameTime = 0f;
        private CameraShaker _cameraShaker;

        private void Start()
        {
            GR = GameResources_2.GameResources;
            GR.NewMessageReceived += ProcessingTCPMessages;
            GR.ResetTheGame += ResetLevel;
            _cameraShaker = Camera.main.GetComponent<CameraShaker>();
            BaseStart(ConnectTypes.UDP);
        }
       
        private void FixedUpdate()
        {
            if (GR.GameOn)
                SendJoystick();            
        }

        private void Update()
        {
            if (GR.GameOn)
            {
                UpdateThread();
                //LerpTransforms();
                MoveToPosition();
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

        private void MoveToPosition()
        {
            if (_frames.Count > 0)
            {
                Vector3 pos_blue = _frames[0].Blue.Position.GetVector3();
                Vector3 pos_red = _frames[0].Red.Position.GetVector3();

                Quaternion rot_blue = _frames[0].Blue.Rotation.GetQuaternion();
                Quaternion rot_red = _frames[0].Red.Rotation.GetQuaternion();

                GR.Blue.PlayerMover.SetBroadcastPositions(pos_blue, rot_blue);
                GR.Red.PlayerMover.SetBroadcastPositions(pos_red, rot_red);

                _frames.RemoveAt(0);
            }
        }

        private void LerpTransforms()
        {
            if (_frames.Count > 1)
            {
                while (_frames.Count >= 3)
                {
                    _frames.RemoveAt(0);
                    _frameTime = _frames[0].TimeSinceLevelLoad;
                }

                _frameTime += Time.deltaTime;

                float time = _frames[0].TimeSinceLevelLoad;
                float time2 = _frames[1].TimeSinceLevelLoad;
                //float vrem = Time.timeSinceLevelLoad - _delay;

                //normalized = (x - min(x)) / (max(x) - min(x));
                //float delta = (vrem - time) / (time2 - time);
                float delta = (_frameTime - time) / (time2 - time);

                Vector2 pos_blue = Vector2.LerpUnclamped(_frames[0].Blue.Position.GetVector3(), _frames[1].Blue.Position.GetVector3(), delta);
                Vector2 pos_red = Vector2.LerpUnclamped(_frames[0].Red.Position.GetVector3(), _frames[1].Red.Position.GetVector3(), delta);

                Quaternion rot_blue = Quaternion.LerpUnclamped(_frames[0].Blue.Rotation.GetQuaternion(), _frames[1].Blue.Rotation.GetQuaternion(), delta);
                Quaternion rot_red = Quaternion.LerpUnclamped(_frames[0].Red.Rotation.GetQuaternion(), _frames[1].Red.Rotation.GetQuaternion(), delta);

                GR.Blue.PlayerMover.SetBroadcastPositions(pos_blue, rot_blue);
                GR.Red.PlayerMover.SetBroadcastPositions(pos_red, rot_red);
            }
        }

        private void ResetLevel()
        {
            _frames.Clear();
            _lastMove = Vector2.zero;
        }

        //private void LerpTransforms_Ticks()
        //{
        //    if (_frames.Count > 1)
        //    {
        //        long time = _frames[0].Ticks;
        //        long time2 = _frames[1].Ticks;
        //        long vrem = DateTime.UtcNow.Ticks - Network.TimeDifferenceWithServer / 2 - _delay; //TODO: Так вроде нормально, но чёт нелогично           

        //        if (vrem >= time2)
        //        {
        //            _frames.RemoveAt(0);
        //            //TODO: Мб надо что-то всё равно показать. Наверное надо вернуть в начало функции
        //        }
        //        else if (time <= vrem && vrem < time2)
        //        {
        //            //normalized = (x - min(x)) / (max(x) - min(x));
        //            float delta = (vrem - time) / (time2 - time);

        //            Vector2 pos_blue = Vector2.Lerp(_frames[0].Blue.GetPosition(), _frames[1].Blue.GetPosition(), delta);
        //            Vector2 pos_red = Vector2.Lerp(_frames[0].Red.GetPosition(), _frames[1].Red.GetPosition(), delta);

        //            Quaternion rot_blue = Quaternion.Lerp(_frames[0].Blue.GetQuaternion(), _frames[1].Blue.GetQuaternion(), delta);
        //            Quaternion rot_red = Quaternion.Lerp(_frames[0].Red.GetQuaternion(), _frames[1].Red.GetQuaternion(), delta);

        //            //TODO: Quaternion.LerpUnclamped

        //            GR.Blue.PlayerMover.SetBroadcastPositions(pos_blue, rot_blue);
        //            GR.Red.PlayerMover.SetBroadcastPositions(pos_red, rot_red);
        //        }
        //        else if (time > vrem) //По идее это бессмысленная строчка  
        //        {
        //            Debug.Log("Time error");
        //            return;
        //        }
        //    }
        //}  

        private void SendJoystick()
        {
            if (GR.GameOn)
            {
                Vector2 move = new Vector2(_joystick.Horizontal, _joystick.Vertical).normalized;
                if (move != _lastMove)
                {
                    Network.ClientTCP.SendMessage($"move {move.x} {move.y}");
                    _lastMove = move;
                }
            }           
        }

        private void ProcessingTCPMessages(string[] mes)
        {
            if (mes[0] == "Explosion")
            {
                PlayerTypes player = DataHolder.ParseEnum<PlayerTypes>(mes[1]);
                ExplosionHandling(player);
            }
        }

        private void ExplosionHandling(PlayerTypes player)
        {
            if (player == PlayerTypes.BluePlayer)
                GR.Red.LoseAnimation();
            else if (player == PlayerTypes.RedPlayer)
                GR.Blue.LoseAnimation();
            else
            {
                GR.Blue.LoseAnimation();
                GR.Red.LoseAnimation();
            }

            _cameraShaker.ShakeOnce();
        }

        private void OnDestroy()
        {
            GR.NewMessageReceived -= ProcessingTCPMessages;
            GR.ResetTheGame -= ResetLevel;
        }
    }
}