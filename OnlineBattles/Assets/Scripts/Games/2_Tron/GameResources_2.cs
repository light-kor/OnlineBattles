using GameEnumerations;
using System.Collections;
using UnityEngine;

namespace Game2
{
    public class GameResources_2 : GeneralController
    {
        [SerializeField] private Player _blue, _red;
        public static GameResources_2 GameResources;

        public Player Blue => _blue;
        public Player Red => _red;

        private CameraShaker _cameraShaker;

        private const int WinScore = 5;

        //TODO: !!���!! ����� ����� �������� trail. � ������ �� ���������� ������, �� ����� ������������.       
        //TODO: ������������ �� �������
        //TODO: �������� �������� ��� trails � �������� ����.

        private void Awake()
        {
            GameResources = this;
            _cameraShaker = Camera.main.GetComponent<CameraShaker>();
            ResetTheGame += ResetLevel;
        }

        public void RoundResults()
        {
            if (StartCheckingResults() == true)
            {
                _cameraShaker.ShakeOnce();
                StartCoroutine(FinishRound());
            }               
        }

        private IEnumerator FinishRound()
        {
            yield return null; // ���������� ���� ����, ����� ��� ��������� ������������

            if (_blue.GetPoint)
                UpdateScoreAndCheckGameState(_blue.PlayerType, GameResults.Win, WinScore, true);
            else if (_red.GetPoint)
                UpdateScoreAndCheckGameState(_red.PlayerType, GameResults.Win, WinScore, true);  
            else
                UpdateScoreAndCheckGameState(PlayerTypes.Null, GameResults.Null, WinScore, true);
        }

        private void ResetLevel()
        {
            _blue.ResetLevel();
            _red.ResetLevel();            
        }

        private void OnDestroy()
        {
            ResetTheGame -= ResetLevel;
        }
    }
}