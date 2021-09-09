using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WifiMenuComponents : MonoBehaviour
{
    [SerializeField] private TMP_Text _opponentName;
    [SerializeField] private GameObject _wifiServerPrefab, _serverSearchPanel;
    [SerializeField] private MultiBackButton _multiBackButton;
    private List<string> _wifiServers = new List<string>();
    private MainMenu _menuScr;
    private string _serverAnswer = null;
    private bool _canReadServerAnswer = false, _writeOpponentName = false;

    void Start()
    {
        _menuScr = GetComponent<MainMenu>();
        WifiServer_Connect.AddWifiServerToScreen += GetNewWifiServer;
        WifiServer_Host.AcceptOpponent += WriteOpponentName;
        Network.WifiServerAnswer += WifiServerAnswerProcessing;
    }

    void Update()
    {
        if (_wifiServers.Count > 0)
        {
            CreateWifiServerCopy(_wifiServers[0]);
            _wifiServers.RemoveAt(0);
        }

        if (_canReadServerAnswer)
        {
            WifiServerAnswerProcessing();
            _canReadServerAnswer = false;
        }

        if (_writeOpponentName)
        {
            ChangeOpponentNameAndButton();
            _writeOpponentName = false;
        }
    }

    public void Wifi_SetHost()
    {
        DataHolder.GameType = "WifiServer";
        WifiServer_Host.StartHosting();
        _menuScr._panelAnim.StartTransition(_menuScr.ActivateCreateWifiMenu);
    }

    public void Wifi_Connect()
    {
        DataHolder.GameType = "WifiClient";
        DestroyAllWifiServersIcons();
        WifiServer_Connect.StartSearching();
        _menuScr._panelAnim.StartTransition(_menuScr.ActivateConnectWifiMenu);
    }

    private void DestroyAllWifiServersIcons()
    {
        foreach (Transform g in _serverSearchPanel.GetComponentsInChildren<Transform>())
        {
            if (g.name.Contains("ServerSelect"))
                Destroy(g.gameObject);
        }
    }

    private void GetNewWifiServer(string text)
    {
        _wifiServers.Add(text);
    }

    private void CreateWifiServerCopy(string text)
    {
        float x = Random.Range(-360, 60); // По факту получается диапазон (-360, 360) с пропуском диапазона (-150, 150)
        if (x > -150)
            x += 300;

        float y = Random.Range(-360, 60);
        if (y > -150)
            y += 300;

        GameObject pref = Instantiate(_wifiServerPrefab, _serverSearchPanel.transform);
        pref.transform.localPosition = new Vector3(x, y, 0);
        pref.GetComponent<SelectWifiServer>().SetNameAndIP(text);
    }

    private void WifiServerAnswerProcessing(string text)
    {
        _serverAnswer = text;
        _canReadServerAnswer = true;
    }

    private void WifiServerAnswerProcessing()
    {
        if (_serverAnswer == "denied")
            _menuScr._panelAnim.StartTransition(_menuScr.ActivateMainMenu);
        else if (_serverAnswer == "accept")
            _menuScr.ActivateWaitingWifiLvl();

        _serverAnswer = null;
    }

    public void WriteOpponentName()
    {
        _writeOpponentName = true;
    }

    public void ShowOpponentNameObj()
    {
        _opponentName.text = "Ожидание игроков...";
        _opponentName.gameObject.SetActive(true);
    }

    public void ChangeOpponentNameAndButton() //TODO: Когда игрок отключится, или ты закроешь сервер, надо сделать WifiServer_Host._opponent.PlayerName = null
    {
        _opponentName.text = "Подключён: " + WifiServer_Host._opponent.PlayerName;
        _multiBackButton.ShowMultiBackButton(MultiBackButton.ButtonTypes.Disconnect);
    }

    public void ActivateServerSearchPanel()
    {
        _serverSearchPanel.SetActive(true);
    }

    public void DeactivateServerSearchAndName()
    {
        _serverSearchPanel.SetActive(false);
        _opponentName.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        WifiServer_Connect.AddWifiServerToScreen -= GetNewWifiServer;
        WifiServer_Host.AcceptOpponent -= WriteOpponentName;
        Network.WifiServerAnswer -= WifiServerAnswerProcessing;
    }
}
