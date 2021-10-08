using UnityEngine;


public class GameResources_2 : MonoBehaviour
{
    public static GameResources_2 GameResources;

    private GeneralResources _res;
    [SerializeField] private Player _player1, _player2;


    private bool _pauseGame = false;
    public readonly int WinScore = 5;

    public bool GameOn { get; private set; } = false;

    private void Awake()
    {
        GameResources = this;
        _res = GetComponent<GeneralResources>();
        _res.UpdateScore(null);
    }

    void Start()
    {
        _res.StartTimer(3f, () => GameOn = true);
    }

    public void PauseGame(GameObject loserPlayer)
    {
        if (_pauseGame == false) //  Чтобы оба объекта не делали одно и то же
        {
            GameOn = false;
            _player1.StopTrail();
            _player2.StopTrail();           
            _pauseGame = true;
        }

        _res.UpdateScore(loserPlayer);
        if (CheckEndOfGame() == false)
            _res.OpenEndRoundPanel();
    }

    private bool CheckEndOfGame()
    {
        int myPoints = DataHolder.MyScore;
        int enemyPoints = DataHolder.EnemyScore;

        if (myPoints >= WinScore || enemyPoints >= WinScore)
        {
            string notifText = null;

            if (myPoints > enemyPoints)
                notifText = "Синий победил";
            else if (enemyPoints > myPoints)
                notifText = "Красный победил";

            new Notification(notifText, Notification.ButtonTypes.MenuButton);
            return true;
        }
        else return false;
    }   
}
