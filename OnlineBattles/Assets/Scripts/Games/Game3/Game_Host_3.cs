using System;
using UnityEngine;

public class Game_Host_3 : GameTemplate_WifiHost
{
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Joystick _joystick;
    [SerializeField] private GameObject _me, _enemy, _pointPref;
    private GameObject _maze, _points;
    private Vector2 _myVelocity, _enemyVelocity;
    private Rigidbody2D _myRB, _enemyRB;
    private int _myPoints = 0, _enemyPoints = 0;
    private object locker = new object();
    private float _lastChangeMazeTime = 0f;

    private const int Scale = 2;
    private const int Width = 19;
    private const int Height = 19;

    protected override void Start()
    {
        base.Start();
        StartUdpConnection();

        Cell cell = _cellPrefab.GetComponent<Cell>();
        cell.WallLeft.GetComponent<EdgeCollider2D>().enabled = true;
        cell.WallBottom.GetComponent<EdgeCollider2D>().enabled = true;

        ManualCreateMapButton.Click += CreateMap;
        PointEnterHandler.Catch += GetPoint;

        _myRB = _me.GetComponent<Rigidbody2D>();
        _enemyRB = _enemy.GetComponent<Rigidbody2D>();

        _points = new GameObject("Points");
        CreateMap();        
        InvokeRepeating("SendAllChanges", 0f, WifiServer_Host.UpdateRate);

        _me.transform.position = RandomPositionOnMap();
        _enemy.transform.position = RandomPositionOnMap();

        WifiServer_Host.SendTcpMessage($"position {_enemy.transform.position.x} {_enemy.transform.position.y} {_me.transform.position.x} {_me.transform.position.y}");
    }    

    protected override void Update()
    {
        base.Update();

        if (_gameOn)
        {          
            if (WifiServer_Host._opponent.MessageTCPforGame.Count > 0)
            {
                string[] mes = WifiServer_Host._opponent.MessageTCPforGame[0].Split(' ');
                if (mes[0] == "move")
                {
                    _enemyVelocity = new Vector2(float.Parse(mes[1]), float.Parse(mes[2]));
                }
                else if (mes[0] == "change")
                {
                    CreateMap();
                }
                WifiServer_Host._opponent.MessageTCPforGame.RemoveAt(0);
            }

            _myVelocity = new Vector2(_joystick.Horizontal, _joystick.Vertical);

            _lastChangeMazeTime += Time.deltaTime;

            if (_lastChangeMazeTime > 5f)
                CreateMap();

            CheckEndOfGame();
        }
    }

    private void GetPoint(GameObject player)
    {
        if (player == _me)
            _myPoints++;
        else if (player == _enemy)
            _enemyPoints++;
    }

    public void CreateMap()
    {
        lock (locker)
        {
            if (_maze != null)
                Destroy(_maze);

            _maze = new GameObject("Cells");
            Create();
            _lastChangeMazeTime = 0f;

            Vector2 pointPos = RandomPositionOnMap();
            Instantiate(_pointPref, pointPos, Quaternion.identity, _points.transform);
            WifiServer_Host.SendTcpMessage($"point {pointPos.x} {pointPos.y}");
            //TODO: Карта меняется сама каждые 3 секунды, но каждый может изменить карту сам раз в 5 секунд. Это навык каждого
        }
    }

    private Vector2 RandomPositionOnMap()
    {
        float X = UnityEngine.Random.Range(-Width / 2, Width / 2) + 0.5f;
        float Y = UnityEngine.Random.Range(-Height / 2, Height / 2) + 0.5f;

        return new Vector2(X / Scale, Y / Scale);
    }


    private void Create()
    {
        MazeGenerator generator = new MazeGenerator(Width, Height);
        MazeGeneratorCell[,] maze = generator.GenerateMaze();
        BigDataSendReceive<MazeGeneratorCell[,]>.SendBigMessage(maze);

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

    private void FixedUpdate()
    {
        _myRB.MovePosition(_myRB.position + _myVelocity * Time.fixedDeltaTime * 3);
        _enemyRB.MovePosition(_enemyRB.position + _enemyVelocity * Time.fixedDeltaTime * 3);
    }

    public void CheckEndOfGame()
    {
        if (_myPoints > 5 || _enemyPoints > 5)
        {
            CloseAll();
            _myVelocity = Vector2.zero;
            _enemyVelocity = Vector2.zero;

            if (_myPoints > 5 && _enemyPoints > 5)
                EndOfGame("drawn");
            else if (_myPoints > 5)
                EndOfGame("lose");
            else if (_enemyPoints > 5)
                EndOfGame("win");
        }
    }

    public void SendAllChanges()
    {
        DataHolder.ClientUDP.SendMessage($"g {DateTime.UtcNow.Ticks} {_enemy.transform.position.x} {_enemy.transform.position.y} {_me.transform.position.x} {_me.transform.position.y}");
    }
}
