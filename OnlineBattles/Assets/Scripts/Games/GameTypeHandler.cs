using UnityEngine;

public class GameTypeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _singleGame, _wifiHost, _onlineGame;

    private void Awake()
    {
        _singleGame.SetActive(false);
        _wifiHost.SetActive(false);
        _onlineGame.SetActive(false);

        if (DataHolder.GameType == "OnPhone")
            _singleGame.SetActive(true);
        else if (DataHolder.GameType == "WifiServer")
            _wifiHost.SetActive(true);
        else if (DataHolder.GameType == "WifiClient" || DataHolder.GameType == "Multiplayer")
            _onlineGame.SetActive(true);

        GetComponent<GameTypeHandler>().enabled = false;
    }
}
