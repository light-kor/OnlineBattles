using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StopWaitWifiClients : MonoBehaviour, IPointerClickHandler
{
    Image _myColor;

    public void OnPointerClick(PointerEventData eventData)
    {
        WifiServer_Host.CancelWaiting();
    }

    void Start()
    {
        _myColor = GetComponent<Image>();
    }

    void Update()
    {
        _myColor.color = new Color(255, 255, 255, Mathf.PingPong(Time.time, 1));
        //TODO: �������� �� ���-�� ���������� �� ������ ��� ��������
    }
}