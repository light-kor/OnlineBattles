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

        //TODO: После паузы исчезает trail.В смысле он становится меньше, но потом возвращается        
        //TODO: Глюки джойстика.
        //TODO: Интерполяция на клиенте
        //TODO: Тряску камеры в идеале бы сделать вместе с тряской всего UI.
        //TODO: Цвет фона. Хотелось бы что-то неоновое
        //TODO: Неоновое свечение для trails.
        //TODO: Все ли движения совершаются с помощью rigidbody?
        //TODO: Переделать поворот на принцип отлавливания изменений, чтоб реже можно было присылать сообщения
        //TODO: Иногда в новый раунд начинается с движений клиента из прошлого раунда.
        //TODO: В PlayerMover использовать _rb.MoveRotation или просто изменять rotation?
        //TODO: Мб переделать вычисление угла на кастомный lookAt


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
            yield return null; // Пропускаем один кадр, вдруг они врезались одновременно

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