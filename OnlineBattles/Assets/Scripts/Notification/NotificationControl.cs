using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NotificationControl : MonoBehaviour
{
    [SerializeField] private GameObject _doubleButton;
    [SerializeField] private Button _main, _left, _right, _cancelConnect;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private a_MoveNotifButton _buttonPane;
    [SerializeField] private a_TextReplacement _textPane;
    private a_ShowMovingPanel _anim;

    private Notification _notif;
    private Notification.ButtonTypes _fallBack = 0;

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

            case Notification.ButtonTypes.StopConnecting:             
                Network.StopConnecting();
                break;

            case Notification.ButtonTypes.CancelGameSearch: // CancelGameSearch
                Network.ClientTCP.SendMessage("CancelSearch");
                break;

            case Notification.ButtonTypes.MenuButton: // ExitPresentGame
                SceneManager.LoadScene("mainMenu");
                break;

            case Notification.ButtonTypes.RestartLevel: // Начать игру заново
                GameResourcesTemplate.ResetScore();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name); //TODO: Работает только для оффлайн игры
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
        _anim.ClosePanel();
        NotificationManager.NM.ReleaseNotification();
    }

    public void ManualCloseNotif(Notification.ButtonTypes type)
    {
        _fallBack = type;
        NotificatonMultyButton();
    }

    public void CreateNotification(Notification notif, int orderInLayer)
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = orderInLayer;

        _notif = notif;
        _notif.SetController(this);

        ShowNotification();
    }

    private void ShowNotification()
    {       
        ActivateButtons();
        _anim = GetComponent<a_ShowMovingPanel>();
        _anim.ShowPanel(this);

        if (_notif.NotifType == Notification.NotifTypes.Connection)
        {
            _cancelConnect.GetComponent<a_ShowCancelButton>().ShowButton();
            _cancelConnect.onClick.AddListener(() => ManualCloseNotif(Notification.ButtonTypes.StopConnecting));
        }
        else if (_notif.NotifType == Notification.NotifTypes.Reconnect) //TODO: Это вроде пока не имеет смысла
        {
            _cancelConnect.GetComponent<a_ShowCancelButton>().ShowButton();
            _cancelConnect.onClick.AddListener(() => ManualCloseNotif(Notification.ButtonTypes.StopReconnect));
        }

        _messageText.text = _notif.NotifText;        
    }

    public void UpdateNotification(Notification notif)
    {
        _notif = notif;
        _notif.SetController(this);

        if ((_notif.NotifType == Notification.NotifTypes.Connection || _notif.NotifType == Notification.NotifTypes.Reconnect) && _notif.ButtonType != Notification.ButtonTypes.Waiting)
            _cancelConnect.GetComponent<a_ShowCancelButton>().HideButton();

        _textPane.ReplaceText(ChangeNotification);
    }   

    private void ActivateButtons()
    {
        if (_notif.NotifType == Notification.NotifTypes.EndGame)
        {            
            _left.onClick.AddListener(() => ManualCloseNotif(Notification.ButtonTypes.RestartLevel));
            _right.onClick.AddListener(() => ManualCloseNotif(Notification.ButtonTypes.MenuButton));
            _left.GetComponentInChildren<TMP_Text>().text = "Ещё раз";
            _right.GetComponentInChildren<TMP_Text>().text = "Выход";
            _doubleButton.SetActive(true);
        }
        else if (_notif.NotifType == Notification.NotifTypes.WifiRequest && _notif.ButtonType == 0) //TODO: Зачем проверка на ноль?
        {
            _left.onClick.AddListener(() => ManualCloseNotif(Notification.ButtonTypes.AcceptWifiOpponent));
            _right.onClick.AddListener(() => ManualCloseNotif(Notification.ButtonTypes.CancelWifiOpponent));
            _left.GetComponentInChildren<TMP_Text>().text = "Принять";
            _right.GetComponentInChildren<TMP_Text>().text = "Отклонить";
            _doubleButton.SetActive(true);
        }
        else if (_notif.ButtonType != Notification.ButtonTypes.Waiting)
        {
            _main.onClick.AddListener(() => NotificatonMultyButton());
            _main.gameObject.SetActive(true);
        }
    }

    private void DeactivateButtons()
    {
        _doubleButton.SetActive(false);
        _main.gameObject.SetActive(false);
    }

    public void ShowButtonPane()
    {
        if (_notif.ButtonType != Notification.ButtonTypes.Waiting)
            _buttonPane.ShowButton();       
    }

    private void ChangeNotification()
    {
        _messageText.text = _notif.NotifText;

        if (_notif.NotifType == Notification.NotifTypes.WifiRequest && _notif.ButtonType == Notification.ButtonTypes.SimpleClose)
        {
            _buttonPane.HideButton();
            DeactivateButtons();
            ActivateButtons();
            ShowButtonPane();
        }
        else
        {
            ActivateButtons();
            ShowButtonPane();
        }       
    }   
}
