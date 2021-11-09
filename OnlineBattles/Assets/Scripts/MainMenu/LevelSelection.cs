using GameEnumerations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    private string _lvlName = string.Empty;

    public void SelectLevel(int lvlNum)
    {
        DataHolder.SelectedServerGame = lvlNum;

        if (DataHolder.GameType == GameTypes.Local)
        {
            SceneManager.LoadScene("lvl" + lvlNum);
        }
        else if (DataHolder.GameType == GameTypes.WifiHost)
        {
            if (WifiServer_Host.OpponentIsReady == false)
            {
                new Notification("Ожидайте второго игрока", Notification.ButtonTypes.SimpleClose);
            }
            else
            {
                WifiServer_Host.Opponent.SendTcpMessage("wifi_go " + "lvl" + lvlNum);
                SceneManager.LoadScene("lvl" + lvlNum);
            }
        }
        else if (DataHolder.GameType == GameTypes.Multiplayer)
        {
            new Notification("Поиск игры", Notification.ButtonTypes.CancelGameSearch);
            _lvlName = "lvl" + lvlNum;
            Network.ClientTCP.SendMessage($"game {lvlNum}");
        }
    }

    public void LoadServerLevel()
    {
        SceneManager.LoadScene(_lvlName);
    }

    public void LoadLevelFromString(string lvlName)
    {
        SceneManager.LoadScene(lvlName);
    }
}