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

        //TODO: ѕосле паузы исчезает trail.¬ смысле он становитс€ меньше, но потом возвращаетс€
        //TODO: ƒобавить световые вспышки между перезапусками
        //TODO: ƒобавить анимации взрывов на клиенте

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
                    StartCoroutine(FinishRound());
                }
            }               
        }

        private IEnumerator FinishRound()
        {
            yield return null; 

            if (Blue.GetPoint && Red.GetPoint)
                UpdateScoreAndCheckGameState(PlayerTypes.Null, GameResults.Draw, WinScore, true);
            else if (Blue.GetPoint)
                UpdateScoreAndCheckGameState(Blue.PlayerType, GameResults.Lose, WinScore, true);
            else if (Red.GetPoint)
                UpdateScoreAndCheckGameState(Red.PlayerType, GameResults.Lose, WinScore, true);  
            else
                UpdateScoreAndCheckGameState(PlayerTypes.Null, GameResults.Null, WinScore, true);
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
