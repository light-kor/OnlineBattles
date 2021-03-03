using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WifiUIComponents : MonoBehaviour
{
    [SerializeField] private TMP_Text _opponent;
    [SerializeField] private GameObject _serverPrefab;
    private List<string> WifiServers = new List<string>();
    private MainMenu MenuScr;
    private string _serverAnswer = null;
    private bool _canReadServerAnswer = false, _showOpponentName = false, _stopAnimWaiting = false;

    // Start is called before the first frame update
    void Start()
    {
        MenuScr = GetComponent<MainMenu>();
        WifiServer_Connect.AddWifiServerToScreen += GetNewWifiServer;
        WifiServer_Host.CleanHostingUI += StopAnimWaitingWifiPlayers;
        WifiServer_Host.AcceptOpponent += ShowOpponentName;
        Network.WifiServerAnswer += WifiServerAnswerProcessing;
    }

    // Update is called once per frame
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

        if (_showOpponentName)
        {
            MenuScr._waitingAnim.SetActive(false);
            _opponent.gameObject.SetActive(true);
            _opponent.text = "Подключён: " + WifiServer_Host._opponent.PlayerName;
            _showOpponentName = false;
        }

        if (_stopAnimWaiting)
        {
            MenuScr._waitingAnim.SetActive(false);
            _opponent.gameObject.SetActive(false);
            MenuScr.ActivateMenuPanel();
            _stopAnimWaiting = false;
        }
    }

    private void GetNewWifiServer(string text)
    {
        WifiServers.Add(text);
    }

    private void StopAnimWaitingWifiPlayers()
    {       
        _stopAnimWaiting = true;
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
        GameObject pref = Instantiate(_serverPrefab, MenuScr._serverSearchPanel.transform);
        pref.transform.localPosition = new Vector3(x, y, 0);
        pref.GetComponent<WifiServerSelect>().SetNameAndIP(text);
    }

    public void ShowOpponentName()
    {
        _showOpponentName = true;
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
        }

        _serverAnswer = null;
    }
}
