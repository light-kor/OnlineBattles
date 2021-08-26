using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class NotificationControl : MonoBehaviour
{
    public event DataHolder.Notification CloseNotification;

    [SerializeField] private Button _closeNotifButton, _acceptButton, _cancelButton;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private a_MoveNotifButton _buttonPane;
    [SerializeField] private a_TextReplacement _textPane;

    private Notification _notif;

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
        var notif = _notif.ButtonType;
        switch (notif)
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
                if (notif == Notification.ButtonTypes.CancelWifiOpponent)
                    WifiServer_Host.OpponentStatus = "denied";
                else 
                    WifiServer_Host.OpponentStatus = "accept";
                break;

            case Notification.ButtonTypes.StopTryingReconnect: // Правильный выход из StartReconnect
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
    public void ShowNotification(Notification notif) //TODO: А если будет несколько уведомлений по очереди, надо сделать очередь.
    {
        _notif = notif;
        if (_notif.NotifType == Notification.NotifTypes.WifiRequest)
        {
            _acceptButton.gameObject.SetActive(true);
            _cancelButton.gameObject.SetActive(true);
            _acceptButton.onClick.AddListener(() => WifiRequestButtons(Notification.ButtonTypes.AcceptWifiOpponent));
            _cancelButton.onClick.AddListener(() => WifiRequestButtons(Notification.ButtonTypes.CancelWifiOpponent));
        }
        else
        {
            if (_notif.ButtonType != Notification.ButtonTypes.Waiting)
            { 
                ActivateCloseButton();
            }
        }
        _messageText.text = _notif.NotifText;        
    }

    public void UpdateNotification(Notification notif)
    {
        _notif = notif;
        Action transfer = TransferText;
        StartCoroutine(_textPane.ReplaceText(transfer));
    }

    void ActivateCloseButton()
    {
        _closeNotifButton.onClick.AddListener(() => NotificatonMultyButton());
        _closeNotifButton.gameObject.SetActive(true);
    }

    public void WifiRequestButtons(Notification.ButtonTypes type)
    {
        _notif.ButtonType = type;
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
        _buttonPane.ShowButton();
    }
}
