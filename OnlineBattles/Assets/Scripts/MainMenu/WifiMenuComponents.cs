using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WifiMenuComponents : MonoBehaviour
{
    [SerializeField] private TMP_Text _opponent;
    [SerializeField] private GameObject _wifiServerPrefab, _serverSearchPanel;
    [SerializeField] private MultiBackButton _multiBackButton;
    private List<string> _wifiServers = new List<string>();
    private MainMenu _menuScr;
    private string _serverAnswer = null;
    private bool _canReadServerAnswer = false, _writeOpponentName = false, _hideOpponentName = false, _showOpponentNameObj = false;

    void Start()
    {
        _menuScr = GetComponent<MainMenu>();
        WifiServer_Connect.AddWifiServerToScreen += GetNewWifiServer;
        WifiServer_Host.CleanHostingUI += HideOpponentName;
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

        if (_showOpponentNameObj)
        {
            _opponent.text = "�������� �������...";
            _opponent.gameObject.SetActive(true);
            _showOpponentNameObj = false;
        }

        //TODO: ����� ����� ����������, ��� �� �������� ������, ���� ������� WifiServer_Host._opponent.PlayerName = null
        if (_writeOpponentName)
        {
            _opponent.text = "���������: " + WifiServer_Host._opponent.PlayerName;
            _multiBackButton.UpdateButtonText(MultiBackButton.ButtonTypes.Disconnect);
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
        _menuScr.SwitchToTargetPanel(null);
    }

    public void Wifi_ConnectHost()
    {
        DataHolder.GameType = "WifiClient";
        DestroyAllWifiServersIcons();
        WifiServer_Connect.StartSearching();
        _menuScr.SwitchToTargetPanel(_serverSearchPanel);
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
        float x = Random.Range(-360, 60); // �� ����� ���������� �������� (-360, 360) � ��������� ��������� (-150, 150)
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
            _menuScr.SwitchToMenuPanel();
        else if (_serverAnswer == "accept")
        {
            _menuScr.DeactivatePanels();
            _menuScr.ChangeLvlChoseWaitingState(true);
            _multiBackButton.UpdateButtonText(MultiBackButton.ButtonTypes.Disconnect);
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
