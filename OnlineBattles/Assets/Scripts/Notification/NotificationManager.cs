using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager NM;
    [SerializeField] private GameObject _notifPrefab;
    public List<Notification> _notifList = new List<Notification>();
    private NotificationControl _serverConnectNotification = null;
    private int _orderInLayer = 20;
    private bool _flag = false;

    private void Awake()
    {
        NM = this;
        //TODO: Следующая строчка - это костыль от Таблички "Поиск новый игры" 
        //в начале сцены тк запрос с опоздание переходит с прошлой сцены в эту, если игра нашлась слишком быстро
        _notifList.Clear();
    }

    public void AddNotificationToQueue(Notification notification)
    {
        _notifList.Add(notification);
    }

    private void Update()
    {
        if (_notifList.Count > 0)
        {           
            CreateNewNotification();
            _notifList.RemoveAt(0);
        }

        if (_flag)
        {
            //NotificatonMultyButton(1);
            _flag = false;
        }
    }

    public void CreateNewNotification()
    {
        var notifText = _notifList[0].NotifText;

        if (_notifList[0].NotifType == Notification.NotifTypes.Connection)
        {
            if (_serverConnectNotification == null)
            {
                _serverConnectNotification = CreateNotifObj().GetComponent<NotificationControl>();
                _serverConnectNotification.ShowNotification(_notifList[0]);
            }
            else
            {
                _serverConnectNotification.UpdateNotification(_notifList[0]);
            }
        }
        else
        {
            var notification = CreateNotifObj().GetComponent<NotificationControl>();
            notification.ShowNotification(_notifList[0]);
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


    
}
