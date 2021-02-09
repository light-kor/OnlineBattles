using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NotificationPanels : MonoBehaviour
{
    public GameObject NotifPanel, NotifButton, StopReconnectButton, CancelSearchButton, CloseEndGameButton, AcceptOpponent, CancelOpponent;
    public GameObject Shield; // Блокирует нажатия на все кнопки, кроме notifPanel

    public static List<string> ListOfNotification = new List<string>();
    public static List<int> NumOfNotification = new List<int>();

    private void Awake()
    {
        DataHolder.NotifPanels = this;

        Network.ShowGameNotification += AddNotificationToQueue;
        MainMenuScr.ShowGameNotification += AddNotificationToQueue;
        WifiServer_Host.FoundOnePlayer += AddNotificationToQueue;
        WifiServer_Connect.ShowGameNotification += AddNotificationToQueue;
        //TODO: Следующие две строчки - это костыль от Таблички "Поиск новый игры" 
        //в начале сцены тк запрос с опоздание переходит с прошлой сцены в эту, если игра нашлась слишком быстро
        ListOfNotification.Clear();
        NumOfNotification.Clear();
    }

    private void AddNotificationToQueue(string notif, int caseNotif)
    {
        ListOfNotification.Add(notif);
        NumOfNotification.Add(caseNotif);
    }

    private void Update()
    {       
        if (ListOfNotification.Count > 0)
        {
            ShowNotif(ListOfNotification[0], NumOfNotification[0]);           
        }
    }



    /// <summary>
    /// Функция закрытия всех типов уведомлений NotifPanel с последующей обработкой.
    /// </summary>
    /// <param name="num">Тип уведомления.</param>
    public void NotificatonMultyButton(int num)
    {
        switch (num)
        {
            case 1: // ExitSimpleNotif       
                NotifButton.SetActive(false);
                break;

            case 2: // StopReconnect                
                Network.StopReconnecting();
                StopReconnectButton.SetActive(false);
                SceneManager.LoadScene("mainMenu"); // Ну если не хочешь reconnect во время игры, то не играй))
                //TODO: Ну тогда надо ещё корректно завершить игру и закрыть UDP соединение.
                break;

            case 3: // CancelGameSearch
                DataHolder.ClientTCP.SendMessage("CancelSearch");
                CancelSearchButton.SetActive(false);
                break;

            case 4: // ExitPresentGame
                CloseEndGameButton.SetActive(false);
                SceneManager.LoadScene("mainMenu");
                break;

            case 50: // Искать другого противника по wifi
            case 51: // Принять противника по wifi?
                AcceptOpponent.SetActive(false);
                CancelOpponent.SetActive(false);
                if (num == 50)
                    WifiServer_Host.OpponentStatus = "cancel";
                else WifiServer_Host.OpponentStatus = "accept";
                break;

            case 0: // Выключение всех
                NotifButton.SetActive(false);
                CancelSearchButton.SetActive(false);
                CloseEndGameButton.SetActive(false);
                StopReconnectButton.SetActive(false);
                break;

            case 10: // Правильный выход из StartReconnect
                Network.TryRecconect = false;
                StopReconnectButton.SetActive(false);
                break;
        }
        NotifPanel.SetActive(false);
        Shield.SetActive(false);
    }

    /// <summary>
    /// Выводит на экран уведомление и отключает все остальные кнопки.
    /// </summary>
    /// <param name="notif">Текст уведомления.</param>
    /// <param name="caseNotif">Выбор типа кнопки и самого уведомления на окне.</param>
    public void ShowNotif(string notif, int caseNotif) //TODO: А если будет несколько уведомлений по очереди, надо сделать очередь.
    {
        Shield.SetActive(true); //TODO: При переходе между сценами связь между ссылками временно теряется и вылетает ошибка

        NotifButton.SetActive(false);
        StopReconnectButton.SetActive(false);
        CancelSearchButton.SetActive(false);
        CloseEndGameButton.SetActive(false);
        AcceptOpponent.SetActive(false);
        CancelOpponent.SetActive(false);

        if (caseNotif == 1)
            NotifButton.SetActive(true);
        else if (caseNotif == 2)
            StopReconnectButton.SetActive(true);
        else if (caseNotif == 3)
            CancelSearchButton.SetActive(true);
        else if (caseNotif == 4)
            CloseEndGameButton.SetActive(true);
        else if (caseNotif == 5)
        {
            AcceptOpponent.SetActive(true);
            CancelOpponent.SetActive(true);
        }
            

        NotifPanel.transform.Find("Text").GetComponent<Text>().text = notif;
        NotifPanel.SetActive(true);

        ListOfNotification.RemoveAt(0);
        NumOfNotification.RemoveAt(0);
    }

    //TODO: Отписаться от всех событий
}
