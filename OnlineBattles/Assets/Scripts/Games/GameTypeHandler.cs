using UnityEngine;

public class GameTypeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _singleGame, _wifiHost, _onlineGame;
    private string _selectGameType;

    private void Awake()
    {
        _singleGame.SetActive(false);
        _wifiHost.SetActive(false);
        _onlineGame.SetActive(false);

        if (DataHolder.GameType == null)
            DataHolder.GameType = "OnPhone";

        _selectGameType = DataHolder.GameType;

        if (_selectGameType == "OnPhone")
            _singleGame.SetActive(true);
        else if (_selectGameType == "WifiServer")
            _wifiHost.SetActive(true);
        else if (_selectGameType == "WifiClient" || _selectGameType == "Multiplayer")
            _onlineGame.SetActive(true);

        enabled = false;
    }
}
