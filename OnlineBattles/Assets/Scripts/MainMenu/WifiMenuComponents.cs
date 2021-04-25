using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WifiMenuComponents : MonoBehaviour
{
    [SerializeField] private TMP_Text _opponent;
    [SerializeField] private GameObject _wifiServerPrefab, _serverSearchPanel;
    private List<string> WifiServers = new List<string>();
    private MainMenu MenuScr;
    private string _serverAnswer = null;
    private bool _canReadServerAnswer = false, _writeOpponentName = false, _hideOpponentName = false, _showOpponentNameObj = false;

    void Start()
    {
        MenuScr = GetComponent<MainMenu>();
        WifiServer_Connect.AddWifiServerToScreen += GetNewWifiServer;
        WifiServer_Host.CleanHostingUI += HideOpponentName;
        WifiServer_Host.AcceptOpponent += WriteOpponentName;
        Network.WifiServerAnswer += WifiServerAnswerProcessing;
    }

    void Update()
    {
        if (WifiServers.Count > 0)
        {
            CreateWifiServerCopy(WifiServers[0]);
            WifiServers.RemoveAt(0);
        }

        if (_canReadServerAnswer)
        {
            WifiServerAnswerProcessing();
            _canReadServerAnswer = false;
        }

        if (_showOpponentNameObj)
        {
            _opponent.text = "Ожидание игроков...";
            _opponent.gameObject.SetActive(true);
            _showOpponentNameObj = false;
        }

        //TODO: Когда игрок отключится, или ты закроешь сервер, надо сделать WifiServer_Host._opponent.PlayerName = null
        if (_writeOpponentName)
        {
            _opponent.text = "Подключён: " + WifiServer_Host._opponent.PlayerName;
            MenuScr.ShowMultiBackButton("Отключиться");
            _writeOpponentName = false;
        }

        if (_hideOpponentName)
        {
            _opponent.gameObject.SetActive(false);
            _hideOpponentName = false;
        }
    }

    public void Wifi_SetHost()
    {
        DataHolder.GameType = "WifiServer";
        WifiServer_Host.StartHosting();
        ShowOpponentNameObj();
        MenuScr.ActivatePanel(MenuScr._lvlPanel);
    }

    public void Wifi_ConnectHost()
    {
        DataHolder.GameType = "WifiClient";
        DestroyAllWifiServersIcons();
        WifiServer_Connect.StartSearching();
        MenuScr.ActivatePanel(_serverSearchPanel);
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
        WifiServers.Add(text);
    }
    
    private void CreateWifiServerCopy(string text)
    {
        float x = 0, y = 0;
        while (x < 150 && x > -150)
        {
            x = Random.Range(-360, 360);
        }

        while (y < 150 && y > -150)
        {
            y = Random.Range(-640, 640);
        }
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
            MenuScr.ActivateMenuPanel();
        else if (_serverAnswer == "accept")
        {
            MenuScr.DeactivatePanels();
            MenuScr._lvlChoseWaiting.SetActive(true);
            MenuScr.ShowMultiBackButton("Отключиться");
        }

        _serverAnswer = null;
    }

    public void DeactivateServerSearchPanel()
    {
        _serverSearchPanel.SetActive(false);
    }

    public void HideOpponentName()
    {
        _hideOpponentName = true;
    }

    public void WriteOpponentName()
    {
        _writeOpponentName = true;
    }

    public void ShowOpponentNameObj()
    {
        _showOpponentNameObj = true;
    }

    private void OnDestroy()
    {
        WifiServer_Connect.AddWifiServerToScreen -= GetNewWifiServer;
        WifiServer_Host.CleanHostingUI -= HideOpponentName;
        WifiServer_Host.AcceptOpponent -= WriteOpponentName;
        Network.WifiServerAnswer -= WifiServerAnswerProcessing;
    }
}
