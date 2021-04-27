using System;
using UnityEngine;

public class Game_Host_3 : GameTemplate_WifiHost
{
    private GameResources_3 GR;

    private void Start()
    {
        ManualCreateMapButton.Click += CreateMap;
        GR = transform.parent.GetComponent<GameResources_3>();
        BaseStart("udp");

        GR.StartInHost();
        CreateMap();        
        WifiServer_Host.SendTcpMessage($"position {GR._enemy.transform.position.x} {GR._enemy.transform.position.y} {GR._me.transform.position.x} {GR._me.transform.position.y}");
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
                    GR._enemyVelocity = new Vector2(float.Parse(mes[1]), float.Parse(mes[2]));
                }
                else if (mes[0] == "change")
                {
                    CreateMap();
                }
                WifiServer_Host._opponent.MessageTCPforGame.RemoveAt(0);
            }

            GR._myVelocity = new Vector2(GR._firstJoystick.Horizontal, GR._firstJoystick.Vertical);

            GR._lastChangeMazeTime += Time.deltaTime;

            if (GR._lastChangeMazeTime > GR.MazeUpdateTime)
                CreateMap();

            CheckEndOfGame();
        }
    }

    private void FixedUpdate()
    {
        GR._myRB.MovePosition(GR._myRB.position + GR._myVelocity * Time.fixedDeltaTime * GR.PlayersSpeed);
        GR._enemyRB.MovePosition(GR._enemyRB.position + GR._enemyVelocity * Time.fixedDeltaTime * GR.PlayersSpeed);
    }

    public void CreateMap()
    {
        if (!GR._lock)
        {
            GR._lock = true;
            if (GR._maze != null)
                Destroy(GR._maze);

            GR._maze = new GameObject("Cells");           
            MazeGenerator generator = new MazeGenerator(GR.Width, GR.Height);
            MazeGeneratorCell[,] maze = generator.GenerateMaze();
            BigDataSendReceive<MazeGeneratorCell[,]>.SendBigMessage(maze);
            GR.BuildMaze(maze);

            GR._lastChangeMazeTime = 0f;

            Vector2 pointPos = GR.RandomPositionOnMap();
            Instantiate(GR._pointPref, pointPos, Quaternion.identity, GR._points.transform);
            WifiServer_Host.SendTcpMessage($"point {pointPos.x} {pointPos.y}");
            //TODO: Карта меняется сама каждые 3 секунды, но каждый может изменить карту сам раз в 5 секунд. Это навык каждого
            GR._lock = false;
        }
    }

    public void CheckEndOfGame()
    {
        if (GR._myPoints > GR.WinScore || GR._enemyPoints > GR.WinScore)
        {
            CloseAll();
            GR._myVelocity = Vector2.zero;
            GR._enemyVelocity = Vector2.zero;

            if (GR._myPoints > GR.WinScore && GR._enemyPoints > GR.WinScore)
                EndOfGame("drawn");
            else if (GR._myPoints > GR.WinScore)
                EndOfGame("lose");
            else if (GR._enemyPoints > GR.WinScore)
                EndOfGame("win");
        }
    }

    public override void SendAllChanges()
    {
        DataHolder.ClientUDP.SendMessage($"g {DateTime.UtcNow.Ticks} {GR._enemy.transform.position.x} {GR._enemy.transform.position.y} {GR._me.transform.position.x} {GR._me.transform.position.y}");
    }

    private void OnDestroy()
    {
        ManualCreateMapButton.Click -= CreateMap;
    }
}
