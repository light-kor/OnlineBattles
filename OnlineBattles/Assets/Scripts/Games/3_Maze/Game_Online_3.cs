using System;
using UnityEngine;

namespace Game3
{
    public class Game_Online_3 : GameTemplate_Online
    {
        private Vector2 _lastMove = Vector2.zero;
        private bool _getBigMessage = false;
        private GameResources_3 GR;

        private void Start()
        {
            Network.ClientTCP.BigMessageReceived += GetBigMessage;
            ManualCreateMapButton.Click += SendChangeMazeRequest;

            GR = transform.parent.GetComponent<GameResources_3>();
            GR._points = new GameObject("Points");

            //Чтоб коллизии не мешались
            Cell cell = GR._cellPrefab.GetComponent<Cell>();
            cell.WallLeft.GetComponent<EdgeCollider2D>().enabled = false;
            cell.WallBottom.GetComponent<EdgeCollider2D>().enabled = false;
            BaseStart(DataHolder.ConnectType.UDP);
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

                if (Network.TCPMessagesForGames.Count > 0)
                {
                    string[] mes = Network.TCPMessagesForGames[0].Split(' ');
                    if (mes[0] == "point")
                    {
                        Vector2 position = new Vector2(float.Parse(mes[1]), float.Parse(mes[2]));
                        Instantiate(GR._pointPref, position, Quaternion.identity, GR._points.transform);
                    }
                    else if (mes[0] == "position")
                    {
                        Vector2 myPosition = new Vector2(float.Parse(mes[1]), float.Parse(mes[2]));
                        Vector2 enemyPosition = new Vector2(float.Parse(mes[3]), float.Parse(mes[4]));

                        GR._me.transform.position = myPosition;
                        GR._enemy.transform.position = enemyPosition;
                    }
                    Network.TCPMessagesForGames.RemoveAt(0);
                }

                UpdateThread();
            }
        }

        private void UpdateThreadOld()
        {
            if (Network.UDPMessages.Count > 1)
            {
                long time = Convert.ToInt64(_frame[1]);
                long time2 = Convert.ToInt64(_frame2[1]);
                long vrem = DateTime.UtcNow.Ticks - Network.TimeDifferenceWithServer / 2 - _delay; //TODO: Так вроде нормально, но чёт нелогично
                                                                                                   //long vrem = DateTime.UtcNow.Ticks - _delay;

                if (vrem >= time2)
                {
                    Network.UDPMessages.RemoveAt(0);
                    UpdateThread(); // Чтоб в этом кадре тоже что-то показали
                }
                else if (time <= vrem && vrem < time2)
                {
                    //normalized = (x - min(x)) / (max(x) - min(x));
                    float delta = (vrem - time) / (time2 - time);

                    GR._me.transform.position = Vector2.Lerp(new Vector2(float.Parse(_frame[2]), float.Parse(_frame[3])), new Vector2(float.Parse(_frame2[2]), float.Parse(_frame2[3])), delta);
                    GR._enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(_frame[4]), float.Parse(_frame[5])), new Vector2(float.Parse(_frame2[4]), float.Parse(_frame2[5])), delta);
                }
                //else if (time > vrem) return; //По идее это бессмысленная строчка            
            }
        }

        private void UpdateThread()
        {
            if (Network.UDPMessages.Count > 0)
            {
                _frame = Network.UDPMessages[0].Split(' ');
                if (_frame[0] != "g")
                {
                    Network.UDPMessages.RemoveAt(0);
                    return;
                }

                Vector2 myPosition = new Vector2(gg(_frame[2]), gg(_frame[3]));
                Vector2 enemyPosition = new Vector2(gg(_frame[4]), gg(_frame[5]));

                //GR._me.transform.position = Vector2.MoveTowards(GR._me.transform.position, myPosition, 1.0f);
                //GR._enemy.transform.position = Vector2.MoveTowards(GR._enemy.transform.position, enemyPosition, 1.0f);

                //Debug.Log(DataHolder.MessageUDPget[0]);

                GR._me.transform.position = myPosition;
                GR._enemy.transform.position = enemyPosition;

                Network.UDPMessages.RemoveAt(0);
            }
        }

        private void SendChangeMazeRequest()
        {
            Network.ClientTCP.SendMessage("change");
        }

        private float gg(string d)
        {
            int fd = Convert.ToInt32(d);
            return (float)fd / 100;
        }

        public void CreateMap()
        {
            if (!GR._lock)
            {
                GR._lock = true;

                if (GR._maze != null)
                    Destroy(GR._maze);

                GR._maze = new GameObject("Maze");
                MazeGeneratorCell[,] maze = BigDataExchange<MazeGeneratorCell[,]>.GetBigMessage();
                GR.BuildMaze(maze);

                GR._lock = false;
            }
        }

        public override void SendAllChanges()
        {
            Vector2 move = new Vector2(GR._firstJoystick.Horizontal, GR._firstJoystick.Vertical);
            if (move != _lastMove)
            {
                Network.ClientTCP.SendMessage($"move {move.x} {move.y}");
                Debug.Log($"move {move.x} {move.y}");
                _lastMove = move;
            }
        }

        private void GetBigMessage()
        {
            _getBigMessage = true;
        }

        private void OnDestroy()
        {
            Network.ClientTCP.BigMessageReceived -= GetBigMessage;
            ManualCreateMapButton.Click -= SendChangeMazeRequest;
        }
    }
}
