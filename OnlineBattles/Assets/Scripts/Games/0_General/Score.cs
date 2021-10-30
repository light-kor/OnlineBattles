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
                LocalEndGame();
                return true;
            }
            else if (gameType == GameTypes.WifiHost)
            {
                OnlineEndGame();
                return true;
            }           
        }
        return false;
    }

    private void LocalEndGame()
    {
        string notifText = null;

        if (_blueScore > _redScore)
            notifText = "����� �������";
        else if (_redScore > _blueScore)
            notifText = "������� �������";
        else if (_redScore == _blueScore)
            notifText = "�����";

        new Notification(notifText, Notification.NotifTypes.EndGame);
    }

    private void OnlineEndGame()
    {
        string notifText = null, opponentStatus = null;

        if (_blueScore > _redScore)
        {
            notifText = "�� ��������";
            opponentStatus = "Lose";
        }
        else if (_redScore > _blueScore)
        {
            notifText = "�� ���������";
            opponentStatus = "Win";
        }
        else if (_redScore == _blueScore)
        {
            notifText = "�����";
            opponentStatus = "Draw";
        }

        WifiServer_Host.Opponent.SendTcpMessage("EndGame " + opponentStatus);
        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    }

    private void DisplayScore()
    {
        _firstScore.text = _blueScore.ToString();
        _secondScore.text = _redScore.ToString();
    }   
}
