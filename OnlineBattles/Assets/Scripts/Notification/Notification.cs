using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Notification : MonoBehaviour
{
    public event DataHolder.Notification CloseNotification;

    [SerializeField] private Button _closeNotifButton, _acceptButton, _cancelButton;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private a_MoveNotifButton _buttonPane;

    private NotificationManager.NotifType _buttonType;

    private void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    /// <summary>
    /// Функция закрытия всех типов уведомлений NotifPanel с последующей обработкой.
    /// </summary>
    /// <param name="num">Тип уведомления.</param>
    public void NotificatonMultyButton()
    {
        switch (_buttonType)
        {
            case NotificationManager.NotifType.Simple: // Закрыть уведомление      
                break;

            case NotificationManager.NotifType.Reconnect: // StopReconnect                
                Network.StopReconnecting();
                SceneManager.LoadScene("mainMenu"); // Ну если не хочешь reconnect во время игры, то не играй))
                //TODO: Ну тогда надо ещё корректно завершить игру и закрыть UDP соединение.
                break;

            case NotificationManager.NotifType.GameSearching: // CancelGameSearch
                DataHolder.ClientTCP.SendMessage("CancelSearch");
                break;

            case NotificationManager.NotifType.FinishGame: // ExitPresentGame
                SceneManager.LoadScene("mainMenu");
                break;

            case NotificationManager.NotifType.CancelOpponent: // Искать другого противника по wifi
            case NotificationManager.NotifType.AcceptOpponent: // Принять противника по wifi?
                if (_buttonType == NotificationManager.NotifType.CancelOpponent)
                    WifiServer_Host.OpponentStatus = "denied";
                else 
                    WifiServer_Host.OpponentStatus = "accept";
                break;

            case NotificationManager.NotifType.StopTryingReconnect: // Правильный выход из StartReconnect
                Network.TryRecconect = false;
                break;
        }
        CloseNotification?.Invoke();
    }

    /// <summary>
    /// Выводит на экран уведомление и устанавливает тип уведомления.
    /// </summary>
    /// <param name="notif"> Текст уведомления. </param>
    /// <param name="type"> Выбор типа кнопки и самого уведомления на окне. </param>
    public void ShowNotif(string notif, NotificationManager.NotifType type) //TODO: А если будет несколько уведомлений по очереди, надо сделать очередь.
    {
        _buttonType = type;
        if (type == NotificationManager.NotifType.WifiRequest)
        {
            _acceptButton.gameObject.SetActive(true);
            _cancelButton.gameObject.SetActive(true);
            _acceptButton.onClick.AddListener(() => WifiRequestButtons(NotificationManager.NotifType.AcceptOpponent));
            _cancelButton.onClick.AddListener(() => WifiRequestButtons(NotificationManager.NotifType.CancelOpponent));
        }
        else
        {
            if (type != NotificationManager.NotifType.Waiting)
            {
                _closeNotifButton.onClick.AddListener(() => NotificatonMultyButton());
                _closeNotifButton.gameObject.SetActive(true);
            }
        }
        _messageText.text = notif;        
    }

    public void WifiRequestButtons(NotificationManager.NotifType type)
    {
        _buttonType = type;
        NotificatonMultyButton();
    }

    public void ShowNotifButton()
    {
        if (_buttonType != NotificationManager.NotifType.Waiting)
            _buttonPane.ShowButton();
    }
}
