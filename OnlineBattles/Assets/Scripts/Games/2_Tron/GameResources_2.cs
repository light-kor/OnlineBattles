using GameEnumerations;
using System.Collections;

namespace Game2
{
    public class GameResources_2 : GeneralController
    {      
        public static GameResources_2 GameResources;
        public Player Blue, Red;
        
        private readonly int WinScore = 5;

        //TODO: ����� ����� �������� trail.� ������ �� ���������� ������, �� ����� ������������        
        //TODO: ����� ���������.
        //TODO: ������������ �� �������
        //TODO: ������ ������ ��� ������. ����� ����� ������ �� �� �� �������!

        protected override void Awake()
        {
            base.Awake();
            GameResources = this;
            ResetTheGame += ResetLevel;
        }

        public void RoundResults()
        {
            if (StartCheckingResults() == true)
                StartCoroutine(FinishRound());
        }

        private IEnumerator FinishRound()
        {
            yield return null; // ���������� ���� ����, ����� ��� ��������� ������������

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
