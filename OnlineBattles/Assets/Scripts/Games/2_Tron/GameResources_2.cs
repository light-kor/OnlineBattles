using System.Collections.Generic;
using UnityEngine;

namespace Game2
{
    public class GameResources_2 : GameResourcesTemplate
    {
        public event DataHolder.Notification StopTrail;
        public event DataHolder.Notification ResumeTrail;

        public static GameResources_2 GameResources;
        public List<Vector2> RemoteJoystick = new List<Vector2>();

        [SerializeField] private Player _blue, _red;

        public readonly int WinScore = 5;

        protected override void Awake()
        {
            base.Awake();
            GameResources = this;
        }

        public void RoundResults(GameObject loserPlayer)
        {
            PauseTheGame(PauseType.EndRound);

            UpdateScore(loserPlayer, Result.Lose);

            if (CheckEndOfGame() == false)
                OpenEndRoundPanel();
        }

        protected override void PauseTheGame(PauseType pauseType)
        {
            base.PauseTheGame(pauseType);
            StopTrail?.Invoke();
        }

        protected override void ResumeTheGame()
        {
            base.ResumeTheGame();

            if (_gameOver == false)
                ResumeTrail?.Invoke();
        }

        private bool CheckEndOfGame()
        {
            int myPoints = _blueScore;
            int enemyPoints = _redScore;

            if (myPoints >= WinScore || enemyPoints >= WinScore)
            {
                string notifText = null;

                if (myPoints > enemyPoints)
                    notifText = "Синий победил";
                else if (enemyPoints > myPoints)
                    notifText = "Красный победил";

                new Notification(notifText, Notification.NotifTypes.EndGame);
                return true;
            }
            else return false;
        }

        public void SetControlTypes(ControlType blueType, ControlType redType)
        {
            _blue.SetControlType(blueType);
            _red.SetControlType(redType);
        }

        public void SendFrame()
        {
            if (Network.ClientUDP != null)
                Serializer<NetInfo>.SendMessage(new NetInfo(_blue, _red), DataHolder.ConnectType.UDP);
        }

        public void MoveToPosition(NetInfo frame)
        {
            Vector3 position1 = new Vector3(frame.X_pos1, frame.Y_pos1);
            Vector3 position2 = new Vector3(frame.X_pos2, frame.Y_pos2);

            Quaternion rotation1 = new Quaternion(0, 0, frame.Z_rotation1, 0);
            Quaternion rotation2 = new Quaternion(0, 0, frame.Z_rotation2, 0);

            _blue.SetBroadcastPositions(position1, rotation1);
            _red.SetBroadcastPositions(position2, rotation2);
        }
    }
}
