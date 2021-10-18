using GameEnumerations;
using System.Collections.Generic;
using UnityEngine;

namespace Game2
{
    public class GameResources_2 : GameResourcesTemplate
    {      
        public static GameResources_2 GameResources;
        [HideInInspector] public List<Vector2> RemoteJoystick = new List<Vector2>();
        [SerializeField] private Player _blue, _red;
        
        private readonly int WinScore = 5;

        protected override void Awake()
        {
            base.Awake();
            GameResources = this;
        }

        public void RoundResults(GameObject loserPlayer)
        {
            PauseGame(PauseType.EndRound);

            UpdateScore(loserPlayer, GameResult.Lose);

            if (CheckEndOfGame() == false)
                OpenEndRoundPanel();
        }

        protected override void ResetLevel()
        {
            base.ResetLevel();
            _blue.ResetLevel();
            _red.ResetLevel();           
        }

        private bool CheckEndOfGame()
        {
            if (_blueScore >= WinScore || _redScore >= WinScore)
            {
                string notifText = null;

                if (_blueScore > _redScore)
                    notifText = "Синий победил";
                else if (_redScore > _blueScore)
                    notifText = "Красный победил";
                else if (_redScore == _blueScore)
                    notifText = "Ничья";

                new Notification(notifText, Notification.NotifTypes.EndGame);
                return true;
            }
            else return false;
        }

        public void SetControlTypes(PlayerControl blueType, PlayerControl redType)
        {
            _blue.SetControlType(blueType);
            _red.SetControlType(redType);
        }

        public void SendFrame()
        {
            if (Network.ClientUDP != null)
            {
                FrameInfo data = new FrameInfo(_blue, _red);
                Serializer<FrameInfo>.SendMessage(data, DataHolder.ConnectType.UDP);
            }              
        }

        public void MoveToPosition(FrameInfo frame)
        {
            Vector3 position1 = new Vector3(frame.X_pos1, frame.Y_pos1);
            Vector3 position2 = new Vector3(frame.X_pos2, frame.Y_pos2);

            _blue.SetBroadcastPositions(position1, frame.GetQuaternion(frame.Quaternion1));
            _red.SetBroadcastPositions(position2, frame.GetQuaternion(frame.Quaternion2));
        }
    }
}
