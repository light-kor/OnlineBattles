using TMPro;
using UnityEngine;

public class GameResources_3 : MonoBehaviour
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
    [HideInInspector] public int _speed = 2;

    public readonly int Scale = 4;
    public readonly int Width = 15;
    public readonly int Height = 25;
    public readonly int WinScore = 5;

    private void Start()
    {
        PointEnterHandler.Catch += GetPoint;

        if (DataHolder.GameType == "OnPhone")
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
    //TODO: попробовать брать не 1 длину линий, а рандомную, тогда будут появляться доп проходы
    //TODO: Можно сделать только прямолинейное движение. Всё равно в лабиринте можно ходить только ровно по двум осям

    private void GetPoint(GameObject player)
    {
        if (player == _me)
        {
            _myPoints++;
            _firstScore.text = _myPoints.ToString();
        }
        else if (player == _enemy)
        {
            _enemyPoints++;
            _secondScore.text = _myPoints.ToString();
        }
    }
}
