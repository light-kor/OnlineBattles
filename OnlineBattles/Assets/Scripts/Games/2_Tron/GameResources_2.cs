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

        //TODO: После паузы исчезает trail.В смысле он становится меньше, но потом возвращается        
        //TODO: Глюки джойстика.
        //TODO: Интерполяция на клиенте
        //TODO: Тряска камеры при взрыве. Тогда нужно делать всё не на канвасе!

        protected override void Awake()
        {
            base.Awake();
            GameResources = this;
            ResetTheGame += ResetLevel;
        }

        public void RoundResults()
        {
            if (DataHolder.GameType != GameTypes.WifiClient && DataHolder.GameType != GameTypes.Multiplayer)
            {
                if (_checkingResults == false)
                {
                    _checkingResults = true;                   
                    StartCoroutine(FinishRound());
                }
            }               
        }

        private IEnumerator FinishRound()
        {
            yield return null; 

            if (Blue.GetPoint)
                UpdateScoreAndCheckGameState(Blue.PlayerType, GameResults.Win, WinScore, true);
            else if (Red.GetPoint)
                UpdateScoreAndCheckGameState(Red.PlayerType, GameResults.Win, WinScore, true);  
            else
                UpdateScoreAndCheckGameState(PlayerTypes.Null, GameResults.Null, WinScore, true);
        }

        private void ResetLevel()
        {
            Blue.ResetLevel();
            Red.ResetLevel();
            _checkingResults = false;
        }

        public void SetControlTypes(ControlTypes blueType, ControlTypes redType)
        {
            Blue.SetControlType(blueType);
            Red.SetControlType(redType);
        }

        private void OnDestroy()
        {
            ResetTheGame -= ResetLevel;
        }
    }
}
