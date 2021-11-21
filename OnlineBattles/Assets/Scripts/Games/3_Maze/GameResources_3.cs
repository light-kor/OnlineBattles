using GameEnumerations;
using UnityEngine;

namespace Game3
{
    public class GameResources_3 : GeneralController
    {
        public static GameResources_3 GameResources;
        [SerializeField] private Player _blue, _red;             
        [SerializeField] private MazeSpawner _spawner;

        public Player Blue => _blue;
        public Player Red => _red;
        public MazeSpawner Spawner => _spawner;                

        public const int Scale = 4;
        public const int Width = 15;
        public const int Height = 25;
        private const int WinScore = 5;

        //TODO: Добавит звук или какую-то анимацию после Collect point.
        //TODO: Добавить проверку времени на нажатие кнопки клиентом. Чтоб не взломали и не нажимали слишком часто на смену лабиринта
        //TODO: Добавить интерполяцию

        private void Awake()
        {
            GameResources = this;
        }

        private void Start()
        {
            if (DataHolder.GameType == GameTypes.Local)
                StartOnLocal();
        }

        public void UpdateScore(PlayerTypes playerType)
        {
            UpdateScoreAndCheckGameState(playerType, GameResults.Win, WinScore, false);
        }                

        public void StartOnLocal()
        {
            _spawner.MazeColliderSwitch(true);
            _blue.transform.position = RandomPositionOnMap();
            _red.transform.position = RandomPositionOnMap();

            _spawner.CreateMaze();
        }

        public Vector2 RandomPositionOnMap()
        {
            float X = UnityEngine.Random.Range(-Width / 2, Width / 2) + 0.5f;
            float Y = UnityEngine.Random.Range(-Height / 2, Height / 2) + 0.5f;

            return new Vector2(X / Scale, Y / Scale);
        }                    
    }
}
