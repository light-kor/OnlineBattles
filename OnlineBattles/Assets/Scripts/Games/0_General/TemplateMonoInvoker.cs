using UnityEngine;
using GameEnumerations;

public class TemplateMonoInvoker : MonoBehaviour
{
    [SerializeField] private ConnectTypes _connectType;
    [SerializeField] private GameObject _hostObj;
    [SerializeField] private GameObject _clientObj;

    private GeneralController _general;
    private GameTemplate_Online _client;
    private GameTemplate_WifiHost _host;

    private GameTypes gameType;

    private void Awake()
    {
        gameType = DataHolder.GameType;

        _general = GetComponent<GeneralController>();
        _general.newAwake();

        if (gameType == GameTypes.WifiClient)
            _client = _clientObj.GetComponent<GameTemplate_Online>();

        if (gameType == GameTypes.WifiHost)
            _host = _hostObj.GetComponent<GameTemplate_WifiHost>();
    }

    private void Start()
    {
        if (gameType == GameTypes.WifiClient)
            _client.newStart(_connectType);

        if (gameType == GameTypes.WifiHost)
            _host.newStart(_connectType);
    }

    private void Update()
    {
        _general.newUpdate();
    }

    private void FixedUpdate()
    {
        if (gameType == GameTypes.WifiHost)
            _host.newFixedUpdate();
    }

    private void OnDestroy()
    {
        _general.newOnDestroy();

        if (gameType == GameTypes.WifiClient)
            _client.newOnDestroy();

        if (gameType == GameTypes.WifiHost)
            _host.newOnDestroy();
    }
}
