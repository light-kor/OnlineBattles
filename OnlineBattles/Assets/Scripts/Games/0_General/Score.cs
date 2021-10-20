using GameEnumerations;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField] private TMP_Text _firstScore, _secondScore;

    private int _blueScore = 0;
    private int _redScore = 0;

    public void UpdateScore(PlayerTypes player, GameResults result)
    {
        if (player != PlayerTypes.Null)
        {
            if (player == PlayerTypes.BluePlayer)
            {
                if (result == GameResults.Lose)
                    _redScore++;              
                else if (result == GameResults.Win)
                    _blueScore++;
            }
            else if (player == PlayerTypes.RedPlayer)
            {
                if (result == GameResults.Lose)
                    _blueScore++;
                else if (result == GameResults.Win)
                    _redScore++;
            }
        }
        else if (player == PlayerTypes.Null && result == GameResults.Draw)
        {
            _blueScore++;
            _redScore++;
        }

        DisplayScore();
    }
  
    public bool CheckEndGame(int winScore)
    {
        if (_blueScore >= winScore || _redScore >= winScore)
        {
            string notifText = null;

            if (_blueScore > _redScore)
                notifText = "Синий победил";
            else if (_redScore > _blueScore)
                notifText = "Красный победил";
            else if (_redScore == _blueScore)
                notifText = "Ничья";

            new Notification(notifText, Notification.NotifTypes.EndGame);
            return true;
        }
        else return false;
    }

    private void DisplayScore()
    {
        _firstScore.text = _blueScore.ToString();
        _secondScore.text = _redScore.ToString();
    }   
}
