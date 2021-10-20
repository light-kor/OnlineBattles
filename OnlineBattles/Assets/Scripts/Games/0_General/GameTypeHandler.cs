using GameEnumerations;
using UnityEngine;

public class GameTypeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _singleGame, _wifiHost, _onlineGame;
    private GameTypes _selectGameType;

    private void Awake()
    {
        GetComponentInParent<Canvas>().worldCamera = Camera.main;

        _singleGame.SetActive(false);
        _wifiHost.SetActive(false);
        _onlineGame.SetActive(false);

        _selectGameType = DataHolder.GameType;

        if (_selectGameType == GameTypes.Single || _selectGameType == GameTypes.Null) // Null для тестов. Когда заходишь в игру, минуя меню.
            _singleGame.SetActive(true);
        else if (_selectGameType == GameTypes.WifiHost)
            _wifiHost.SetActive(true);
        else if (_selectGameType == GameTypes.WifiClient || _selectGameType == GameTypes.Multiplayer )
            _onlineGame.SetActive(true);

        enabled = false;
    }
}
