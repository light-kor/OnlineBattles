using GameEnumerations;
using UnityEngine;

public class GameTypeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _wifiHost, _onlineGame;

    private void Awake()
    {
        _wifiHost.SetActive(false);
        _onlineGame.SetActive(false);

        GameTypes _selectGameType = DataHolder.GameType;

        if (_selectGameType == GameTypes.WifiHost)
            _wifiHost.SetActive(true);
        else if (_selectGameType == GameTypes.WifiClient || _selectGameType == GameTypes.Multiplayer)
            _onlineGame.SetActive(true);

        enabled = false;
    }
}
