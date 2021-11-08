using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WifiServerCopy : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text _serverNameText;

    private string _ip = null;
    private string _serverName = null;

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