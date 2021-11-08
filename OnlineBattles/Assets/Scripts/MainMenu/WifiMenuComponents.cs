using GameEnumerations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WifiMenuComponents : MonoBehaviour
{    
    [SerializeField] private WifiServerCopy _wifiServerPrefab;
    [SerializeField] private GameObject _serverSearchPanel;
    [SerializeField] private a_TextReplacement _opponentNamePane;
    [SerializeField] private MultiBackButton _multiBackButton;

    private List<string> _wifiServers = new List<string>();
    private MenuView _menuView;
    private TMP_Text _opponentName;
    private string _serverAnswer = null;
    private bool _canReadServerAnswer = false, _writeOpponentName = false;

    void Start()
    {
        _menuView = GetComponent<MenuView>();
        WifiServer_Connect.AddWifiServerToScreen += AddNewWifiServer;
        WifiServer_Host.AcceptOpponent += ShowOpponentName;
        Network.WifiServerAnswer += WifiServerAnswerProcessing;
    }

    void Update()
    {
        // ��� 3 if ����� ��� �������� �� ������ Network � ������ Unity.

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
            ChangeOpponentNameAndButton(true);
            _writeOpponentName = false;
        }
    }

    public void Wifi_SetHost()
    {
        DataHolder.GameType = GameTypes.WifiHost;
        WifiServer_Host.StartHosting();
        _menuView.ChangePanelWithAnimation(_menuView.ActivateCreateWifiMenu);
    }

    public void Wifi_Connect()
    {
        DataHolder.GameType = GameTypes.WifiClient;
        DestroyAllWifiServersCopies();
        WifiServer_Connect.StartSearching();
        _menuView.ChangePanelWithAnimation(_menuView.ActivateConnectWifiMenu);
    }

    private void DestroyAllWifiServersCopies()
    {
        foreach (WifiServerCopy g in _serverSearchPanel.GetComponentsInChildren<WifiServerCopy>())
        {
            Destroy(g.gameObject);
        }
    }

    private void AddNewWifiServer(string text)
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

        WifiServerCopy pref = Instantiate(_wifiServerPrefab, _serverSearchPanel.transform);
        pref.transform.localPosition = new Vector3(x, y, 0);
        pref.SetNameAndIP(text);
    }

    private void WifiServerAnswerProcessing(string text)
    {
        _serverAnswer = text;
        _canReadServerAnswer = true;
    }

    private void WifiServerAnswerProcessing()
    {
        if (_serverAnswer == "denied")
            _menuView.ChangePanelWithAnimation(_menuView.ActivateMainMenu);
        else if (_serverAnswer == "accept")
            _menuView.ChangePanelWithAnimation(_menuView.ActivateWaitingWifiLvl);

        _serverAnswer = null;
    }

    public void ShowOpponentName()
    {
        _writeOpponentName = true;
    }

    public void ShowOpponentNameText()
    {
        _opponentName = _opponentNamePane.GetComponentInChildren<TMP_Text>();
        _opponentName.text = "�������� �������...";
        _opponentNamePane.gameObject.SetActive(true);
    }

    public void ChangeOpponentNameAndButton(bool withAnimation) //TODO: ����� ����� ����������, ��� �� �������� ������, ���� ������� WifiServer_Host._opponent.PlayerName = null
    {
        if (withAnimation)
        {
            _opponentNamePane.ReplaceText(ChangeOpponentNameText);
            _multiBackButton.UpdateMultiBackButton(BackButtonTypes.Disconnect);
        }
        else
        {
            ChangeOpponentNameText();
            _multiBackButton.ShowMultiBackButton(BackButtonTypes.Disconnect);
        }        
    }

    private void ChangeOpponentNameText()
    {
        _opponentName.text = "���������: " + WifiServer_Host.Opponent.PlayerName;
    }

    public void ActivateServerSearchPanel()
    {
        _serverSearchPanel.SetActive(true);
    }

    public void DeactivateServerSearchAndName()
    {
        _serverSearchPanel.SetActive(false);
        _opponentNamePane.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        WifiServer_Connect.AddWifiServerToScreen -= AddNewWifiServer;
        WifiServer_Host.AcceptOpponent -= ShowOpponentName;
        Network.WifiServerAnswer -= WifiServerAnswerProcessing;
    }
}