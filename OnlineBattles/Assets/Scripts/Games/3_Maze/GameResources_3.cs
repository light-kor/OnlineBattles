using GameEnumerations;
using TMPro;
using UnityEngine;

namespace Game3
{
    public class GameResources_3 : GeneralController
    {
        [SerializeField] private GameObject _mazeCreateButton2;
        [SerializeField] private TMP_Text _firstScore, _secondScore;
        public Joystick _firstJoystick, _secondJoystick;
        public GameObject _me, _enemy, _pointPref;
        public Cell _cellPrefab;
        [HideInInspector] public GameObject _maze, _points;
        [HideInInspector] public int _myPoints = 0, _enemyPoints = 0;
        [HideInInspector] public Vector2 _myVelocity, _enemyVelocity;
        [HideInInspector] public Rigidbody2D _myRB, _enemyRB;
        [HideInInspector] public float _lastChangeMazeTime = 0f;
        [HideInInspector] public bool _lock = false;

        public readonly float PlayersSpeed = 1.5f;
        public readonly float MazeUpdateTime = 8f;
        public readonly int Scale = 4;
        public readonly int Width = 15;
        public readonly int Height = 25;
        public readonly int WinScore = 5;

        private void Start()
        {
            PointEnterHandler.Catch += UpdateScore;
            UpdateScore(null); // ”становить счЄт 0 - 0, если в редакторе случайно изменю что-то
            if (DataHolder.GameType == GameTypes.Single)
            {
                _mazeCreateButton2.SetActive(true);
                _secondJoystick.gameObject.SetActive(true);
            }
            else
            {
                _mazeCreateButton2.SetActive(false);
                _secondJoystick.gameObject.SetActive(false);
            }
        }

        public void StartInHost()
        {
            Cell cell = _cellPrefab.GetComponent<Cell>();
            cell.WallLeft.GetComponent<EdgeCollider2D>().enabled = true;
            cell.WallBottom.GetComponent<EdgeCollider2D>().enabled = true;

            _myRB = _me.GetComponent<Rigidbody2D>();
            _enemyRB = _enemy.GetComponent<Rigidbody2D>();

            _points = new GameObject("Points");

            _me.transform.position = RandomPositionOnMap();
            _enemy.transform.position = RandomPositionOnMap();
        }

        public Vector2 RandomPositionOnMap()
        {
            float X = UnityEngine.Random.Range(-Width / 2, Width / 2) + 0.5f;
            float Y = UnityEngine.Random.Range(-Height / 2, Height / 2) + 0.5f;

            return new Vector2(X / Scale, Y / Scale);
        }

        public MazeGeneratorCell[,] CreateMaze()
        {
            MazeGenerator generator = new MazeGenerator(Width, Height);
            MazeGeneratorCell[,] maze = generator.GenerateMaze();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    float X = (x - (float)(Width / 2)) / Scale;
                    float Y = (y - (float)(Height / 2)) / Scale;
                    Cell cell = Instantiate(_cellPrefab, new Vector2(X, Y), Quaternion.identity, _maze.transform).GetComponent<Cell>();

                    cell.WallLeft.SetActive(maze[x, y].WallLeft);
                    cell.WallBottom.SetActive(maze[x, y].WallBottom);
                }
            }
            return maze;
        }

        public void BuildMaze(MazeGeneratorCell[,] maze)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    float X = (x - (float)(Width / 2)) / Scale;
                    float Y = (y - (float)(Height / 2)) / Scale;
                    Cell cell = Instantiate(_cellPrefab, new Vector2(X, Y), Quaternion.identity, _maze.transform).GetComponent<Cell>();

                    cell.WallLeft.SetActive(maze[x, y].WallLeft);
                    cell.WallBottom.SetActive(maze[x, y].WallBottom);
                }
            }
        }
        //TODO: попробовать брать не 1 длину линий, а рандомную, тогда будут по€вл€тьс€ доп проходы
        //TODO: ћожно сделать только пр€молинейное движение. ¬сЄ равно в лабиринте можно ходить только ровно по двум ос€м

        private void UpdateScore(GameObject player)
        {
            if (player == _me)
            {
                _myPoints++;
            }
            else if (player == _enemy)
            {
                _enemyPoints++;
            }
            _firstScore.text = _myPoints.ToString();
            _secondScore.text = _enemyPoints.ToString();
        }

        private void OnDestroy()
        {
            PointEnterHandler.Catch -= UpdateScore;
        }
    }
}
