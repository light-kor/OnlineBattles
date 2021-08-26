using UnityEngine;

public class Game_onPhone_3 : MonoBehaviour
{
    private bool _gameOn = true;
    private GameResources_3 GR;   

    private void Start()
    {
        ManualCreateMapButton.Click += CreateMap;
        GR = transform.parent.GetComponent<GameResources_3>();
        GR.StartInHost();
        CreateMap();
    }

    private void Update()
    {
        if (_gameOn)
        {
            GR._myVelocity = new Vector2(GR._firstJoystick.Horizontal, GR._firstJoystick.Vertical);
            GR._enemyVelocity = new Vector2(GR._secondJoystick.Horizontal, GR._secondJoystick.Vertical);

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

    private void CreateMap()
    {
        if (!GR._lock)
        {
            GR._lock = true;

            if (GR._maze != null)
                Destroy(GR._maze);

            GR._maze = new GameObject("Maze");
            MazeGenerator generator = new MazeGenerator(GR.Width, GR.Height);
            MazeGeneratorCell[,] maze = generator.GenerateMaze();
            GR.BuildMaze(maze);


            GR._lastChangeMazeTime = 0f;

            Vector2 pointPos = GR.RandomPositionOnMap();
            Instantiate(GR._pointPref, pointPos, Quaternion.identity, GR._points.transform);

            GR._lock = false;
        }
    }

    private void CheckEndOfGame()
    {
        if (GR._myPoints >= GR.WinScore || GR._enemyPoints >= GR.WinScore)
        {
            _gameOn = false;
            GR._myVelocity = Vector2.zero;
            GR._enemyVelocity = Vector2.zero;
            string notifText = null;

            if (GR._myPoints == GR._enemyPoints)
                notifText = "Ничья";
            else if (GR._myPoints > GR._enemyPoints)
                notifText = "Синий победил";
            else if (GR._enemyPoints > GR._myPoints)
                notifText = "Красный победил";

            new Notification(notifText, Notification.ButtonTypes.ExitSingleGame);
        }
    }

    private void OnDestroy()
    {
        ManualCreateMapButton.Click -= CreateMap;
    }
}
