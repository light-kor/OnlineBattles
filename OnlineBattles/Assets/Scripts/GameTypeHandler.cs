using UnityEngine;

public class GameTypeHandler : MonoBehaviour
{
    [SerializeField] private GameObject _singleGame, _wifiGame, _multiGame;

    private void Awake()
    {
        _singleGame.SetActive(false);
        _wifiGame.SetActive(false);
        _multiGame.SetActive(false);

        if (DataHolder.GameType == 1)
            _singleGame.SetActive(true);
        if (DataHolder.GameType == 2)
            _wifiGame.SetActive(true);
        if (DataHolder.GameType == 3)
            _multiGame.SetActive(true);

        GetComponent<GameTypeHandler>().enabled = false;
    }
}
