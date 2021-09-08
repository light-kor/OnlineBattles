using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    private const float TimeBetweenNotif = 1f;

    public static NotificationManager NM;
    [SerializeField] private GameObject _notifPrefab;
    private List<Notification> _newNotif = new List<Notification>();
    private List<Notification> _notifQueue = new List<Notification>();
    private NotificationControl _serverConnectNotif = null;
    private Notification _presentNotif = null;
    private int _orderInLayer = 20;
    private bool _flag = false;
    private bool _notifOnScreen = false;
    private float _timeDelay = 0f;

    private void Awake()
    {
        NM = this;
        //TODO: ��������� ������� - ��� ������� �� �������� "����� ����� ����" 
        //� ������ ����� �� ������ � ��������� ��������� � ������� ����� � ���, ���� ���� ������� ������� ������
        _newNotif.Clear();
        _notifQueue.Clear();
    }

    public void AddNotificationToQueue(Notification notification)
    {
        _newNotif.Add(notification);
    }

    private void Update()
    {
        _timeDelay += Time.deltaTime;

        if (_flag)
        {
            _presentNotif.Controller.ManualCloseNotif(Notification.ButtonTypes.SimpleClose);
            _flag = false;
        }

        if (_timeDelay > TimeBetweenNotif)
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
                _timeDelay = 0f;
            }
        }         
    }

    private void CreateNewNotification(Notification notif)
    {
        _notifOnScreen = true;
        _presentNotif = notif;
        if (_presentNotif.NotifType == Notification.NotifTypes.Connection || _presentNotif.NotifType == Notification.NotifTypes.Reconnect)
        {
            if (_serverConnectNotif == null)
            {
                _serverConnectNotif = CreateNotifObj().GetComponent<NotificationControl>();
                _serverConnectNotif.ShowNotification(_presentNotif);
            }
            else
                _serverConnectNotif.UpdateNotification(_presentNotif);
        }
        else
        {
            var notification = CreateNotifObj().GetComponent<NotificationControl>();
            notification.ShowNotification(_presentNotif);
        }
    }

    private GameObject CreateNotifObj()
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
        _presentNotif.Controller.ManualCloseNotif(Notification.ButtonTypes.StopTryingReconnect);
    }

    public void ReleaseNotification()
    {
        _notifOnScreen = false;
        _presentNotif = null;
    }
}
