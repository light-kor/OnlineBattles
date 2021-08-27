using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager NM;
    [SerializeField] private GameObject _notifPrefab;
    private List<Notification> _newNotif = new List<Notification>();
    private List<Notification> _notifQueue = new List<Notification>();
    private NotificationControl _serverConnectNotification = null;
    private Notification _presentNotif = null;
    private int _orderInLayer = 20;
    private bool _flag = false;
    private bool _notifOnScreen = false;

    private void Awake()
    {
        NM = this;
        //TODO: Следующая строчка - это костыль от Таблички "Поиск новый игры" 
        //в начале сцены тк запрос с опоздание переходит с прошлой сцены в эту, если игра нашлась слишком быстро
        _newNotif.Clear();
        _notifQueue.Clear();
    }

    public void AddNotificationToQueue(Notification notification)
    {
        _newNotif.Add(notification);
    }

    private void Update()
    {
        if (_newNotif.Count > 0)
        {
            if (_presentNotif != null && _presentNotif.NotifType != 0 && _newNotif[0].NotifType == _presentNotif.NotifType)
                CreateNewNotification(_newNotif[0]);
            else
                _notifQueue.Add(_newNotif[0]);              

            _newNotif.RemoveAt(0);
        }

        if (!_notifOnScreen && _notifQueue.Count > 0)
        {
            CreateNewNotification(_notifQueue[0]);
            _notifQueue.RemoveAt(0);
        }

        if (_flag)
        {
            _presentNotif.Controller.ManualCloseNotif();
            _flag = false;
        }
    }

    public void CreateNewNotification(Notification notif)
    {
        _notifOnScreen = true;
        _presentNotif = notif;
        if (_presentNotif.NotifType == Notification.NotifTypes.Connection)
        {
            if (_serverConnectNotification == null)
            {
                _serverConnectNotification = CreateNotifObj().GetComponent<NotificationControl>();
                _serverConnectNotification.ShowNotification(_presentNotif);
            }
            else
                _serverConnectNotification.UpdateNotification(_presentNotif);
        }
        else
        {
            var notification = CreateNotifObj().GetComponent<NotificationControl>();
            notification.ShowNotification(_presentNotif);
        }
        //TODO: Добавить Notification.NotifTypes.Reconnect
    }

    public GameObject CreateNotifObj()
    {
        GameObject obj = Instantiate(_notifPrefab);
        obj.GetComponent<Canvas>().sortingOrder = _orderInLayer++;
        return obj;
    }

    public void CloseNotification()
    {
        _flag = true;
    }

    public void CloseStartReconnect()
    {
        _presentNotif.Controller.StopTryingReconnect();
    }

    public void ReleaseNotification()
    {
        _notifOnScreen = false;
        _presentNotif = null;
    }

}
