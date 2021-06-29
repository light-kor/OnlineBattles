using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    public event DataHolder.Notification CloseNotification;

    [SerializeField] private GameObject _closeNotifButton, _acceptButton, _cancelButton;
    [SerializeField] private Text _messageText;

    private void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    /// <summary>
    /// Функция закрытия всех типов уведомлений NotifPanel с последующей обработкой.
    /// </summary>
    /// <param name="num">Тип уведомления.</param>
    public void NotificatonMultyButton(int num)
    {
        switch (num)
        {
            case 1: // Закрыть уведомление      
                break;

            case 2: // StopReconnect                
                Network.StopReconnecting();
                SceneManager.LoadScene("mainMenu"); // Ну если не хочешь reconnect во время игры, то не играй))
                //TODO: Ну тогда надо ещё корректно завершить игру и закрыть UDP соединение.
                break;

            case 3: // CancelGameSearch
                DataHolder.ClientTCP.SendMessage("CancelSearch");
                break;

            case 4: // ExitPresentGame
                SceneManager.LoadScene("mainMenu");
                break;

            case 50: // Искать другого противника по wifi
            case 51: // Принять противника по wifi?
                _acceptButton.SetActive(false);
                _cancelButton.SetActive(false);
                if (num == 50)
                    WifiServer_Host.OpponentStatus = "denied";
                else WifiServer_Host.OpponentStatus = "accept";
                break; 

            case 10: // Правильный выход из StartReconnect
                Network.TryRecconect = false;
                break;
        }
        CloseNotification?.Invoke();
    }

    /// <summary>
    /// Выводит на экран уведомление и устанавливает тип уведомления.
    /// </summary>
    /// <param name="notif">Текст уведомления.</param>
    /// <param name="caseNotif">Выбор типа кнопки и самого уведомления на окне.</param>
    public void ShowNotif(string notif, int caseNotif) //TODO: А если будет несколько уведомлений по очереди, надо сделать очередь.
    { 
        if (caseNotif == 5)
        {
            _acceptButton.SetActive(true);
            _cancelButton.SetActive(true);
        }
        else
        {           
            if (caseNotif != 0)
            {
                _closeNotifButton.GetComponent<Button>().onClick.AddListener(() => NotificatonMultyButton(caseNotif));
                _closeNotifButton.SetActive(true);
            }                
        }
        _messageText.text = notif;    
    }
}
