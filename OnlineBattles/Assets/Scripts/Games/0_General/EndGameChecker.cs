using GameEnumerations;
using UnityEngine;

public class EndGameChecker : MonoBehaviour
{
    public bool CheckEndGame(int winScore, int blueScore, int redScore)
    {
        GameTypes gameType = DataHolder.GameType;

        if (blueScore >= winScore || redScore >= winScore)
        {
            if (gameType == GameTypes.Local)
            {
                LocalEndGame(blueScore, redScore);
                return true;
            }
            else if (gameType == GameTypes.WifiHost)
            {
                HostEndGame(blueScore, redScore);
                return true;
            }
        }
        return false;
    }

    private void LocalEndGame(int blueScore, int redScore)
    {
        string notifText = null;

        if (blueScore > redScore)
            notifText = "Синий победил";
        else if (redScore > blueScore)
            notifText = "Красный победил";
        else if (redScore == blueScore)
            notifText = "Ничья";

        new Notification(notifText, Notification.NotifTypes.EndGame);
    }

    private void HostEndGame(int blueScore, int redScore)
    {
        string notifText = null, opponentStatus = null;

        if (blueScore > redScore)
        {
            notifText = "Вы победили";
            opponentStatus = "Lose";
        }
        else if (redScore > blueScore)
        {
            notifText = "Вы проиграли";
            opponentStatus = "Win";
        }
        else if (redScore == blueScore)
        {
            notifText = "Ничья";
            opponentStatus = "Draw";
        }

        WifiServer_Host.Opponent.SendTcpMessage("EndGame " + opponentStatus);
        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    }

    public void OnlineEndGame(string status)
    {
        string notifText = null;
        if (status == "Draw")
            notifText = "Ничья";
        else if (status == "Win")
            notifText = "Вы победили";
        else if (status == "Lose")
            notifText = "Вы проиграли";

        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    }
}
