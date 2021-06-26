using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    [SerializeField] private GameObject NotifPanel, Shield;
    [SerializeField] private GameObject NotifButton, AcceptOpponent, CancelOpponent;
    [SerializeField] private Text Message;

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
                AcceptOpponent.SetActive(false);
                CancelOpponent.SetActive(false);
                if (num == 50)
                    WifiServer_Host.OpponentStatus = "denied";
                else WifiServer_Host.OpponentStatus = "accept";
                break; 

            case 10: // Правильный выход из StartReconnect
                Network.TryRecconect = false;
                break;
        }
        NotifPanel.SetActive(false);
        Shield.SetActive(false);
    }

    /// <summary>
    /// Выводит на экран уведомление и устанавливает тип уведомления.
    /// </summary>
    /// <param name="notif">Текст уведомления.</param>
    /// <param name="caseNotif">Выбор типа кнопки и самого уведомления на окне.</param>
    public void ShowNotif(string notif, int caseNotif) //TODO: А если будет несколько уведомлений по очереди, надо сделать очередь.
    {
        Shield.SetActive(true); //TODO: При переходе между сценами связь между ссылками временно теряется и вылетает ошибка       

        if (caseNotif == 5)
        {
            AcceptOpponent.SetActive(true);
            CancelOpponent.SetActive(true);
        }
        else
        {           
            if (caseNotif != 0)
            {
                NotifButton.GetComponent<Button>().onClick.AddListener(() => NotificatonMultyButton(caseNotif));
                NotifButton.SetActive(true);
            }                
        }

        Message.text = notif;
        NotifPanel.SetActive(true);        
    }
}
