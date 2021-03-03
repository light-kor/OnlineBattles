using UnityEngine;

public class GameTypeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _singleGame, _wifiHost, _onlineGame;

    private void Awake()
    {
        _singleGame.SetActive(false);
        _wifiHost.SetActive(false);
        _onlineGame.SetActive(false);

        if (DataHolder.GameType == 1)
            _singleGame.SetActive(true);
        else if (DataHolder.GameType == 22)
            _wifiHost.SetActive(true);
        else
            _onlineGame.SetActive(true);

        GetComponent<GameTypeHandler>().enabled = false;
    }
}
