using UnityEngine;

public class GameTypeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _singleGame, _wifiHost, _onlineGame;
    private DataHolder.GameTypes _selectGameType;

    private void Awake()
    {
        _singleGame.SetActive(false);
        _wifiHost.SetActive(false);
        _onlineGame.SetActive(false);

        _selectGameType = DataHolder.GameType;

        if (_selectGameType == DataHolder.GameTypes.Single || _selectGameType == DataHolder.GameTypes.Null) // Для тестов
            _singleGame.SetActive(true);
        else if (_selectGameType == DataHolder.GameTypes.WifiHost)
            _wifiHost.SetActive(true);
        else if (_selectGameType == DataHolder.GameTypes.WifiClient || _selectGameType == DataHolder.GameTypes.Multiplayer )
            _onlineGame.SetActive(true);

        enabled = false;
    }
}
