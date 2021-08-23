using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager NM;
    [SerializeField] private GameObject _notifPrefab;

    private List<string> _listOfNotif = new List<string>();
    private List<NotifType> _notifType = new List<NotifType>();
    private Notification _serverConnectNotification = null;
    private int _orderInLayer = 20;
    private bool _flag = false;

    private void Awake()
    {
        NM = this;
        //TODO: Следующие две строчки - это костыль от Таблички "Поиск новый игры" 
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

        if (_notifType[0] == NotifType.Connection)
        {
            if (_serverConnectNotification == null)
            {
                _serverConnectNotification = CreateNotifObj().GetComponent<Notification>();
                _serverConnectNotification.ShowNotification(notifText, NotifType.Waiting);
            }
            else
            {
                //_serverConnectNotification.SetActive(false);
                //Destroy(_serverConnectNotification);
                //_serverConnectNotification = null;
                //notification.ShowNotif(notifText, NotifType.Simple);

                //TODO: Если только нет сети, то это. Надо додумать логику
                _serverConnectNotification.UpdateNotification(notifText, NotifType.Simple);
            }
        }
        else
        {
            var notification = CreateNotifObj().GetComponent<Notification>();
            notification.ShowNotification(notifText, _notifType[0]);
        }
            

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
