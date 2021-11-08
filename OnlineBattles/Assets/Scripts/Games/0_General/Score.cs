using GameEnumerations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Score : MonoBehaviour
{
    public static event UnityAction<PlayerTypes> RoundWinner;
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

    public void UpdateOnlineScore(string[] message)
    {
        PlayerTypes player = DataHolder.ParseEnum<PlayerTypes>(message[1]);
        GameResults result = DataHolder.ParseEnum<GameResults>(message[2]);
        UpdateScore(player, result);
    }

    public void UpdateAndTrySendScore(PlayerTypes player, GameResults result)
    {
        UpdateScore(player, result);

        if (DataHolder.GameType == GameTypes.WifiHost)
            GameTemplate_WifiHost.SendScore(player, result);
    }

    public void GetScore(out int blueScore, out int redScore)
    {
        blueScore = _blueScore;
        redScore = _redScore;
    }

    private void DisplayScore()
    {
        _firstScore.text = _blueScore.ToString();
        _secondScore.text = _redScore.ToString();
    }   
}
