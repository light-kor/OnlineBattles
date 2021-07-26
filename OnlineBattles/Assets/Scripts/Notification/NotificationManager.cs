using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager NM;
    [SerializeField] private GameObject _notifPrefab;

    private List<string> _listOfNotif = new List<string>();
    private List<NotifType> _notifType = new List<NotifType>();
    private GameObject _serverConnectNotification = null;
    private int _orderInLayer = 20;
    private bool _flag = false;

    private void Awake()
    {
        NM = this;
        //TODO: —ледующие две строчки - это костыль от “аблички "ѕоиск новый игры" 
        //в начале сцены тк запрос с опоздание переходит с прошлой сцены в эту, если игра нашлась слишком быстро
        _listOfNotif.Clear();
        _notifType.Clear();
    }

    public void AddNotificationToQueue(NotifType caseNotif, string notif)
    {
        _listOfNotif.Add(notif);
        _notifType.Add(caseNotif);
    }

    private void Update()
    {
        if (_listOfNotif.Count > 0)
        {           
            CreateNewNotification();
            _listOfNotif.RemoveAt(0);
            _notifType.RemoveAt(0);
        }

        if (_flag)
        {
            //NotificatonMultyButton(1);
            _flag = false;
        }
    }

    public void CreateNewNotification()
    {
        var notifText = _listOfNotif[0];
        var notification = CreateNotifObj().GetComponent<Notification>();

        if (_notifType[0] == NotifType.Connection)
        {
            if (_serverConnectNotification == null)
            {
                _serverConnectNotification = notification.gameObject;
                notification.ShowNotif(notifText, NotifType.Waiting);
            }
            else
            {
                _serverConnectNotification.SetActive(false);
                Destroy(_serverConnectNotification);
                _serverConnectNotification = null;
                notification.ShowNotif(notifText, NotifType.Simple);
            }
        }
        else
            notification.ShowNotif(notifText, _notifType[0]);

    }

    public GameObject CreateNotifObj()
    {
        GameObject obj;
        obj = Instantiate(_notifPrefab);
        obj.GetComponent<Canvas>().sortingOrder = _orderInLayer++;
        return obj;
    }

    public void CloseAllNotification()
    {
        // наверное тут
        // flag = true;
    }

    public void CloseStartReconnect()
    {
        // NotificatonMultyButton(10)
    }


    public enum NotifType
    {
        Waiting, // num  0
        Simple, // num 1
        Connection, 
        Reconnect, // num 2
        GameSearching, // num 3
        FinishGame, // num 4
        WifiRequest, // num 5

        CancelOpponent,
        AcceptOpponent,
        StopTryingReconnect
    }
}
