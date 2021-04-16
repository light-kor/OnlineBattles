using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectWifiServer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text _serverNameText;

    private string _ip;
    private string _serverName;

    public void SetNameAndIP(string text)
    {
        string[] mes = text.Split(' ');
        _serverName = mes[0];
        _ip = mes[1];

        _serverNameText.text = _serverName;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        DataHolder.WifiGameIp = _ip;
        WifiServer_Connect.ConnectToWifiServer();
    }
}
