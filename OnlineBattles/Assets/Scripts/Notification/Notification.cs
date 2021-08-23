using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class Notification : MonoBehaviour
{
    public event DataHolder.Notification CloseNotification;

    [SerializeField] private Button _closeNotifButton, _acceptButton, _cancelButton;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private a_MoveNotifButton _buttonPane;
    [SerializeField] private a_TextReplacement _textPane;

    private NotificationManager.NotifType _notifType;
    private string _replaceText;

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
        switch (_notifType)
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
                if (_notifType == NotificationManager.NotifType.CancelOpponent)
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
    public void ShowNotification(string notif, NotificationManager.NotifType type) //TODO: А если будет несколько уведомлений по очереди, надо сделать очередь.
    {
        _notifType = type;
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
                ActivateCloseButton();
            }
        }
        _messageText.text = notif;        
    }

    public void UpdateNotification(string notifText, NotificationManager.NotifType newType)
    {
        _replaceText = notifText;
        _notifType = NotificationManager.NotifType.Simple;
        Action transfer = TransferText;
        StartCoroutine(_textPane.ReplaceText(transfer));
    }

    void ActivateCloseButton()
    {
        _closeNotifButton.onClick.AddListener(() => NotificatonMultyButton());
        _closeNotifButton.gameObject.SetActive(true);
    }

    public void WifiRequestButtons(NotificationManager.NotifType type)
    {
        _notifType = type;
        NotificatonMultyButton();
    }

    public void ShowNotifButton()
    {
        if (_notifType != NotificationManager.NotifType.Waiting)
            _buttonPane.ShowButton();
    }
    
    private void TransferText()
    {
        _messageText.text = _replaceText;
        ActivateCloseButton();
        _buttonPane.ShowButton();
    }
}
