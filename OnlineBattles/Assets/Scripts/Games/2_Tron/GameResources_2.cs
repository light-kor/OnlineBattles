using UnityEngine;

public class GameResources_2 : GameResourcesTemplate
{
    public event DataHolder.Notification StopTrail;
    public event DataHolder.Notification ResumeTrail;

    public static GameResources_2 GameResources;

    public readonly int WinScore = 5;

    protected override void Awake()
    {
        base.Awake();
        GameResources = this;
    }

    public void RoundResults(GameObject loserPlayer) 
    {
        PauseTheGame(PauseType.EndRound);

        UpdateScore(loserPlayer, Result.Lose);

        if (CheckEndOfGame() == false)
            OpenEndRoundPanel();
    }

    protected override void PauseTheGame(PauseType pauseType)
    {
        base.PauseTheGame(pauseType);
        StopTrail?.Invoke();
    }

    protected override void ResumeTheGame()
    {
        base.ResumeTheGame();

        if (_gameOver == false)
            ResumeTrail?.Invoke();
    }

    private bool CheckEndOfGame()
    {
        int myPoints = _blueScore;
        int enemyPoints = _redScore;

        if (myPoints >= WinScore || enemyPoints >= WinScore)
        {
            string notifText = null;

            if (myPoints > enemyPoints)
                notifText = "Синий победил";
            else if (enemyPoints > myPoints)
                notifText = "Красный победил";

            new Notification(notifText, Notification.NotifTypes.EndGame);
            return true;
        }
        else return false;
    }
}
