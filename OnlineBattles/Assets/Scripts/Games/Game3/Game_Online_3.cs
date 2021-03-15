using System;
using UnityEngine;

public class Game_Online_3 : GameTemplate_Online
{
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Joystick _joystick;
    [SerializeField] private GameObject _me, _enemy, _pointPref;
    private GameObject _maze, _points;
    private Vector2 _lastMove = Vector2.zero;
    private bool _getBigMessage = false;
    private const int Scale = 2;

    protected override void Start()
    {
        _points = new GameObject("Points");

        //���� �������� �� ��������
        Cell cell = _cellPrefab.GetComponent<Cell>();
        cell.WallLeft.GetComponent<EdgeCollider2D>().enabled = false;
        cell.WallBottom.GetComponent<EdgeCollider2D>().enabled = false;

        DataHolder.ClientTCP.GetBigMessage += GetBigMessage;
        ManualCreateMapButton.Click += SendChangeMazeRequest; 
        base.Start();       
    }

    protected override void Update()
    {
        base.Update();
        if (_gameOn)
        {
            if (_getBigMessage)
            {
                CreateMaze();
                _getBigMessage = false;
            }

            //UpdateThread();
            //SendJoy();

            if (DataHolder.MessageTCPforGame.Count > 0)
            {
                string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');
                if (mes[0] == "point")
                {
                    Instantiate(_pointPref, new Vector2(float.Parse(mes[1]), float.Parse(mes[2])), Quaternion.identity, _points.transform);
                }
                else if (mes[0] == "position")
                {
                    _me.transform.position = new Vector2(float.Parse(mes[1]), float.Parse(mes[2]));
                    _enemy.transform.position = new Vector2(float.Parse(mes[3]), float.Parse(mes[4]));
                }
                DataHolder.MessageTCPforGame.RemoveAt(0);
            }

            UpdateThread();
        }        
    }

    private void UpdateThreadOld()
    {
        if (DataHolder.MessageUDPget.Count > 1)
        {
            if (!SplitFramesAndChechTrash())
                return;

            long time = Convert.ToInt64(frame[1]);
            long time2 = Convert.ToInt64(frame2[1]);
            long vrem = DateTime.UtcNow.Ticks - DataHolder.TimeDifferenceWithServer / 2 - _delay; //TODO: ��� ����� ���������, �� ��� ���������
            //long vrem = DateTime.UtcNow.Ticks - _delay;

            if (vrem >= time2)
            {
                DataHolder.MessageUDPget.RemoveAt(0);
                UpdateThread(); // ���� � ���� ����� ���� ���-�� ��������
            }                
            else if (time <= vrem && vrem < time2)
            {
                //normalized = (x - min(x)) / (max(x) - min(x));
                float delta = (vrem - time) / (time2 - time);

                _me.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame[2]), float.Parse(frame[3])), new Vector2(float.Parse(frame2[2]), float.Parse(frame2[3])), delta);
                _enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame[4]), float.Parse(frame[5])), new Vector2(float.Parse(frame2[4]), float.Parse(frame2[5])), delta);
            }
            //else if (time > vrem) return; //�� ���� ��� ������������� �������            
        }
    }

    private void UpdateThread()
    {
        if (DataHolder.MessageUDPget.Count > 0)
        {
            frame = DataHolder.MessageUDPget[0].Split(' ');
            if (frame[0] != "g")
            {
                DataHolder.MessageUDPget.RemoveAt(0);
                return;
            }

            _me.transform.position = Vector2.MoveTowards(_me.transform.position, new Vector2(float.Parse(frame[2]), float.Parse(frame[3])), 1.0f);
            _enemy.transform.position = Vector2.MoveTowards(_enemy.transform.position, new Vector2(float.Parse(frame[4]), float.Parse(frame[5])), 1.0f);

            DataHolder.MessageUDPget.RemoveAt(0);      
        }
    }

    private void FixedUpdate()
    {
        //UpdateThread();
        SendJoy();
    }

    private void SendChangeMazeRequest()
    {
        DataHolder.ClientTCP.SendMessage("change");
    }

    private void SendJoy()
    {
        Vector2 move = new Vector2(_joystick.Horizontal, _joystick.Vertical);
        if (move != _lastMove)
        {
            DataHolder.ClientTCP.SendMessage($"move {move.x} {move.y}");
            _lastMove = move;
        }
    }

    private void GetBigMessage()
    {
        _getBigMessage = true;
    }

    private void CreateMaze()
    {
        if (_maze != null)
            Destroy(_maze);

        _maze = new GameObject("Cells");

        MazeGeneratorCell[,] maze = BigDataSendReceive<MazeGeneratorCell[,]>.GetBigMessage();
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float X = (x - (float)(width / 2)) / Scale;
                float Y = (y - (float)(height / 2)) / Scale;
                Cell c = Instantiate(_cellPrefab, new Vector2(X, Y), Quaternion.identity, _maze.transform).GetComponent<Cell>();

                c.WallLeft.SetActive(maze[x, y].WallLeft);
                c.WallBottom.SetActive(maze[x, y].WallBottom);
            }
        }
    }
}
