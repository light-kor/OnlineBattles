using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NotificationControl : MonoBehaviour
{
    public event DataHolder.Notification CloseNotification;

    [SerializeField] private Button _closeNotifButton, _acceptButton, _cancelButton;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private a_MoveNotifButton _buttonPane;
    [SerializeField] private a_TextReplacement _textPane;

    private Notification _notif;
    private Notification.ButtonTypes _fallBack = 0;

    private void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    private void NotificatonMultyButton()
    {
        Notification.ButtonTypes buttonType;

        if (_fallBack == 0)
            buttonType = _notif.ButtonType;
        else
            buttonType = _fallBack;

        switch (buttonType)
        {
            case Notification.ButtonTypes.SimpleClose: // Закрыть уведомление      
                break;

            case Notification.ButtonTypes.StopReconnect: // StopReconnect                
                Network.StopReconnecting();
                SceneManager.LoadScene("mainMenu"); // Ну если не хочешь reconnect во время игры, то не играй))
                //TODO: Ну тогда надо ещё корректно завершить игру и закрыть UDP соединение.
                break;

            case Notification.ButtonTypes.CancelGameSearch: // CancelGameSearch
                DataHolder.ClientTCP.SendMessage("CancelSearch");
                break;

            case Notification.ButtonTypes.ExitSingleGame: // ExitPresentGame
                SceneManager.LoadScene("mainMenu");
                break;

            case Notification.ButtonTypes.CancelWifiOpponent: // Искать другого противника по wifi
            case Notification.ButtonTypes.AcceptWifiOpponent: // Принять противника по wifi?
                if (buttonType == Notification.ButtonTypes.CancelWifiOpponent)
                    WifiServer_Host.OpponentStatus = "denied";
                else 
                    WifiServer_Host.OpponentStatus = "accept";
                break;

            case Notification.ButtonTypes.StopTryingReconnect: // Правильный выход из StartReconnect
                Network.TryRecconect = false;
                break;
        }
        CloseNotification?.Invoke();
        NotificationManager.NM.ReleaseNotification();
    }

    public void ShowNotification(Notification notif) //TODO: А если будет несколько уведомлений по очереди, надо сделать очередь.
    {
        _notif = notif;
        _notif.SetController(this);
        if (_notif.NotifType == Notification.NotifTypes.WifiRequest)
        {
            _acceptButton.gameObject.SetActive(true);
            _cancelButton.gameObject.SetActive(true);
            _acceptButton.onClick.AddListener(() => WifiRequestButtons(Notification.ButtonTypes.AcceptWifiOpponent));
            _cancelButton.onClick.AddListener(() => WifiRequestButtons(Notification.ButtonTypes.CancelWifiOpponent));
        }
        else
            ActivateCloseButton();

        _messageText.text = _notif.NotifText;        
    }

    public void UpdateNotification(Notification notif)
    {
        _notif = notif;
        StartCoroutine(_textPane.ReplaceText(TransferText));
    }

    private void ActivateCloseButton()
    {
        if (_notif.ButtonType != Notification.ButtonTypes.Waiting)
        {
            _closeNotifButton.onClick.AddListener(() => NotificatonMultyButton());
            _closeNotifButton.gameObject.SetActive(true);
        }
    }

    private void WifiRequestButtons(Notification.ButtonTypes type)
    {
        _fallBack = type;
        NotificatonMultyButton();
    }

    public void ShowNotifButton()
    {
        if (_notif.ButtonType != Notification.ButtonTypes.Waiting)
            _buttonPane.ShowButton();
    }
    
    private void TransferText()
    {
        _messageText.text = _notif.NotifText;
        ActivateCloseButton();
        ShowNotifButton();
    }

    public void ManualCloseNotif()
    {
        _fallBack = Notification.ButtonTypes.SimpleClose;
        NotificatonMultyButton();
    }

    public void StopTryingReconnect()
    {
        _fallBack = Notification.ButtonTypes.StopTryingReconnect;
        NotificatonMultyButton();
    }
}
