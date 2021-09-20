using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    private const float TimeBetweenNotif = 1f;

    public static NotificationManager NM;
    [SerializeField] private NotificationControl _notifPrefab;
    private List<Notification> _newNotif = new List<Notification>();
    private List<Notification> _notifQueue = new List<Notification>();
    private Notification _presentNotif = null;
    private int _orderInLayer = 20;
    private bool _closeFlag = false;
    private bool _notifOnScreen = false;
    private float _timeDelay = 0f;

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
        _timeDelay += Time.deltaTime;

        if (_closeFlag)
        {
            _presentNotif.Controller.ManualCloseNotif(Notification.ButtonTypes.SimpleClose);
            _closeFlag = false;
        }

        if (_timeDelay > TimeBetweenNotif)
        {
            if (_newNotif.Count > 0)
            {
                if (_presentNotif != null && _presentNotif.NotifType != 0 && _newNotif[0].NotifType == _presentNotif.NotifType)
                    ShowNotification(_newNotif[0]);
                else
                    _notifQueue.Add(_newNotif[0]);

                _newNotif.RemoveAt(0);
            }

            if (!_notifOnScreen && _notifQueue.Count > 0)
            {
                ShowNotification(_notifQueue[0]);
                _notifQueue.RemoveAt(0);
                _timeDelay = 0f;
            }
        }         
    }

    private void ShowNotification(Notification notif)
    {
        _notifOnScreen = true;

        if (_presentNotif == null)
        {
            NotificationControl notifControl = Instantiate(_notifPrefab);
            notifControl.SetStartSettings(_orderInLayer++);
            notifControl.ShowNotification(notif);
        }
        else
            _presentNotif.Controller.UpdateNotification(notif);

        _presentNotif = notif;        
    }

    public void CloseNotification()
    {
        _closeFlag = true;
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
