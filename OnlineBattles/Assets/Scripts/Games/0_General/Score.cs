using GameEnumerations;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public delegate void SendPlayer(PlayerTypes player);
    public static event SendPlayer RoundWinner;

    [SerializeField] private TMP_Text _firstScore, _secondScore;

    private int _blueScore = 0;
    private int _redScore = 0;

    public void UpdateScore(PlayerTypes player, GameResults result)
    {
        if (player == PlayerTypes.Both && result == GameResults.Draw)
        {
            _blueScore++;
            _redScore++;
            RoundWinner?.Invoke(PlayerTypes.Both);
        }
        else if (player == PlayerTypes.Null)
        {
            RoundWinner?.Invoke(PlayerTypes.Null);
        }          
        else 
        {
            if (player == PlayerTypes.BluePlayer)
            {
                if (result == GameResults.Lose)
                {
                    _redScore++;
                    RoundWinner?.Invoke(PlayerTypes.RedPlayer);                    
                }                                 
                else if (result == GameResults.Win)
                {
                    _blueScore++;
                    RoundWinner?.Invoke(PlayerTypes.BluePlayer);
                }                   
            }
            else if (player == PlayerTypes.RedPlayer)
            {
                if (result == GameResults.Lose)
                {
                    _blueScore++;
                    RoundWinner?.Invoke(PlayerTypes.BluePlayer);
                }
                else if (result == GameResults.Win)
                {
                    _redScore++;
                    RoundWinner?.Invoke(PlayerTypes.RedPlayer);
                }
            }
        }       
        
        DisplayScore();
    }
  
    public bool CheckEndGame(int winScore, GameTypes gameType)
    {
        if (_blueScore >= winScore || _redScore >= winScore)
        {
            if (gameType == GameTypes.Null || gameType == GameTypes.Single)
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
            else if (gameType == GameTypes.WifiHost)
            {
                string notifText = null, opponentStatus = null;

                if (_blueScore > _redScore)
                {
                    notifText = "Вы победили";
                    opponentStatus = "Lose";
                }                   
                else if (_redScore > _blueScore)
                {
                    notifText = "Вы проиграли";
                    opponentStatus = "Win";
                }                   
                else if (_redScore == _blueScore)
                {
                    notifText = "Ничья";
                    opponentStatus = "Draw";
                }
                    
                WifiServer_Host.Opponent.SendTcpMessage("EndGame " + opponentStatus);
                new Notification(notifText, Notification.ButtonTypes.MenuButton);
                return true;
            }           
        }
        return false;
    }

    private void DisplayScore()
    {
        _firstScore.text = _blueScore.ToString();
        _secondScore.text = _redScore.ToString();
    }   
}
