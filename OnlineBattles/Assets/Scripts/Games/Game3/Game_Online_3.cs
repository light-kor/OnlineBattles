using System;
using UnityEngine;

public class Game_Online_3 : GameTemplate_Online
{
    private Vector2 _lastMove = Vector2.zero;
    private bool _getBigMessage = false;
    private GameResources_3 GR;

    protected override void Start()
    {
        GR = transform.parent.GetComponent<GameResources_3>();
        GR._points = new GameObject("Points");

        //Чтоб коллизии не мешались
        Cell cell = GR._cellPrefab.GetComponent<Cell>();
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
                CreateMap();
                _getBigMessage = false;
            }

            //UpdateThread();
            //SendJoy();

            if (DataHolder.MessageTCPforGame.Count > 0)
            {
                string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');
                if (mes[0] == "point")
                {
                    Instantiate(GR._pointPref, new Vector2(float.Parse(mes[1]), float.Parse(mes[2])), Quaternion.identity, GR._points.transform);
                }
                else if (mes[0] == "position")
                {
                    GR._me.transform.position = new Vector2(float.Parse(mes[1]), float.Parse(mes[2]));
                    GR._enemy.transform.position = new Vector2(float.Parse(mes[3]), float.Parse(mes[4]));
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
            long vrem = DateTime.UtcNow.Ticks - DataHolder.TimeDifferenceWithServer / 2 - _delay; //TODO: Так вроде нормально, но чёт нелогично
            //long vrem = DateTime.UtcNow.Ticks - _delay;

            if (vrem >= time2)
            {
                DataHolder.MessageUDPget.RemoveAt(0);
                UpdateThread(); // Чтоб в этом кадре тоже что-то показали
            }                
            else if (time <= vrem && vrem < time2)
            {
                //normalized = (x - min(x)) / (max(x) - min(x));
                float delta = (vrem - time) / (time2 - time);

                GR._me.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame[2]), float.Parse(frame[3])), new Vector2(float.Parse(frame2[2]), float.Parse(frame2[3])), delta);
                GR._enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame[4]), float.Parse(frame[5])), new Vector2(float.Parse(frame2[4]), float.Parse(frame2[5])), delta);
            }
            //else if (time > vrem) return; //По идее это бессмысленная строчка            
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

            GR._me.transform.position = Vector2.MoveTowards(GR._me.transform.position, new Vector2(float.Parse(frame[2]), float.Parse(frame[3])), 1.0f);
            GR._enemy.transform.position = Vector2.MoveTowards(GR._enemy.transform.position, new Vector2(float.Parse(frame[4]), float.Parse(frame[5])), 1.0f);

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

    public void CreateMap()
    {
        if (!GR._lock)
        {
            GR._lock = true;

            if (GR._maze != null)
                Destroy(GR._maze);

            GR._maze = new GameObject("Maze");
            MazeGeneratorCell[,] maze = BigDataSendReceive<MazeGeneratorCell[,]>.GetBigMessage();
            GR.BuildMaze(maze);

            GR._lock = false;
        }
    }

    private void SendJoy()
    {
        Vector2 move = new Vector2(GR._firstJoystick.Horizontal, GR._firstJoystick.Vertical);
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
}
