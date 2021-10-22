using GameEnumerations;
using System.Collections;

namespace Game2
{
    public class GameResources_2 : GeneralController
    {      
        public static GameResources_2 GameResources;
        public Player Blue, Red;
        private bool _checkingResults = false;

        private readonly int WinScore = 5;

        protected override void Awake()
        {
            base.Awake();
            GameResources = this;
        }

        public void RoundResults()
        {
            if (DataHolder.GameType != GameTypes.WifiClient && DataHolder.GameType != GameTypes.Multiplayer)
            {
                if (_checkingResults == false)
                {
                    _checkingResults = true;
                    PauseGame(PauseTypes.EndRound);
                    StartCoroutine(FinishRound());
                }
            }               
        }

        private IEnumerator FinishRound()
        {
            yield return null;

            if (Blue.GetPoint && Red.GetPoint)
                UpdateAndTrySendScore(PlayerTypes.Null, GameResults.Draw);
            else if (Blue.GetPoint)
                UpdateAndTrySendScore(Blue.PlayerType, GameResults.Lose);
            else if (Red.GetPoint)
                UpdateAndTrySendScore(Red.PlayerType, GameResults.Lose);

            _res.GameScore.CheckEndGame(WinScore, DataHolder.GameType);
        }
        
        protected override void ResetLevel()
        {
            base.ResetLevel();
            Blue.ResetLevel();
            Red.ResetLevel();
            _checkingResults = false;
        }
       
        public void SetControlTypes(ControlTypes blueType, ControlTypes redType)
        {
            Blue.SetControlType(blueType);
            Red.SetControlType(redType);
        }              
    }
}
