using UnityEngine;

namespace Game2
{
    public class GameResources_2 : GameResourcesTemplate
    {
        public event DataHolder.Notification StopTrail;
        public event DataHolder.Notification ResumeTrail;

        public static GameResources_2 GameResources;

        [SerializeField] private Player _blue, _red;

        public readonly int WinScore = 5;

        protected override void Awake()
        {
            base.Awake();
            GameResources = this;
            DeactivateSecondJoyStick();
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
                    notifText = "����� �������";
                else if (enemyPoints > myPoints)
                    notifText = "������� �������";

                new Notification(notifText, Notification.NotifTypes.EndGame);
                return true;
            }
            else return false;
        }

        public void DeactivateSecondJoyStick()
        {
            _red.SetOnlineView();
            //NetInfo dsvfe = new NetInfo(_blue, _red);
        }

        public void SendFrame()
        {
            BigDataExchange<NetInfo>.SendBigMessage(new NetInfo(_blue, _red), DataHolder.ConnectType.UDP);
        }
    }
}
