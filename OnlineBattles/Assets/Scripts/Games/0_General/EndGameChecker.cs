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
                OnlineEndGame(blueScore, redScore);
                return true;
            }
        }
        return false;
    }

    private void LocalEndGame(int blueScore, int redScore)
    {
        string notifText = null;

        if (blueScore > redScore)
            notifText = "����� �������";
        else if (redScore > blueScore)
            notifText = "������� �������";
        else if (redScore == blueScore)
            notifText = "�����";

        new Notification(notifText, Notification.NotifTypes.EndGame);
    }

    private void OnlineEndGame(int blueScore, int redScore)
    {
        string notifText = null, opponentStatus = null;

        if (blueScore > redScore)
        {
            notifText = "�� ��������";
            opponentStatus = "Lose";
        }
        else if (redScore > blueScore)
        {
            notifText = "�� ���������";
            opponentStatus = "Win";
        }
        else if (redScore == blueScore)
        {
            notifText = "�����";
            opponentStatus = "Draw";
        }

        WifiServer_Host.Opponent.SendTcpMessage("EndGame " + opponentStatus);
        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    }
}
