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
        switch (_notifType[0])
        {
            case NotifType.Simple:
                CreateNotifObj().GetComponent<Notification>().ShowNotif(notifText, 1);
                break;

            case NotifType.Connection:
                if (_serverConnectNotification == null)
                {
                    _serverConnectNotification = CreateNotifObj();
                    _serverConnectNotification.GetComponent<Notification>().ShowNotif(notifText, 0);
                }                  
                else
                {
                    _serverConnectNotification.SetActive(false);
                    Destroy(_serverConnectNotification);
                    _serverConnectNotification = null;
                    CreateNotifObj().GetComponent<Notification>().ShowNotif(notifText, 1);
                }
                break;

            case NotifType.Waiting:
                CreateNotifObj().GetComponent<Notification>().ShowNotif(notifText, 0);
                break;

            case NotifType.Reconnect:
                CreateNotifObj().GetComponent<Notification>().ShowNotif(notifText, 2);
                break;

            case NotifType.GameSearching:
                CreateNotifObj().GetComponent<Notification>().ShowNotif(notifText, 3);
                break;

            case NotifType.FinishGame:
                CreateNotifObj().GetComponent<Notification>().ShowNotif(notifText, 4);
                break;

            case NotifType.AcceptOpponent:
                CreateNotifObj().GetComponent<Notification>().ShowNotif(notifText, 5);
                break;
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
        AcceptOpponent // num 5
    }
}
