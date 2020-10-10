using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class OnlineGameController : MonoBehaviour
{
    public GameObject endPanel, fastExitPanel;
    public Text endText;
    

    private void Start()
    {
        Application.runInBackground = true;
        DataHolder.timerT = GameObject.FindGameObjectWithTag("Timer");
    }


    private void Update()
    {
        while (DataHolder.messageTCP.Count > 0)
        {
            // Обработка полученных от сервера сообщений
            string[] mes = DataHolder.messageTCP[0].Split(' ');
            switch (mes[0])
            {
                //TODO: Добавить кейсы всех игр
                case "1":
                    DataHolder.messageTCPforGame.Add(DataHolder.messageTCP[0]);
                    break;

                case "N":
                    DataHolder.messageTCPforGame.Add(DataHolder.messageTCP[0]);                   
                    break;

                //case "S":
                //    DataHolder.canMove = true;
                //    DataHolder.timerT.GetComponent<timer>().StartTimer();
                //    break;

                case "E":
                    DataHolder.canMove = false;
                    DataHolder.timerT.GetComponent<timer>().StopGameTimer();
                    endPanel.SetActive(true);
                    break;

                case "A":
                    DataHolder.canMove = false;
                    endText.text = "Победа, соперник вышел";
                    DataHolder.timerT.GetComponent<timer>().StopGameTimer();
                    endPanel.SetActive(true);
                    break;

                case "V":
                    DataHolder.canMove = false;
                    endText.text = "Техническое поражение";
                    DataHolder.timerT.GetComponent<timer>().StopGameTimer();
                    endPanel.SetActive(true);
                    break;

                case "C":
                    DataHolder.canMove = false;
                    endText.text = "Игра не началась";
                    DataHolder.timerT.GetComponent<timer>().StopGameTimer();
                    endPanel.SetActive(true);
                    break;
            }
            DataHolder.messageTCP.RemoveAt(0);
        }


        // Проверяем, не завершилась ли игра
        if (DataHolder.WinFlag != 0)
        {
            if (DataHolder.WinFlag == 1)
            {
                endText.text = "WIN";
                DataHolder.ClientTCP.SendMassage("W");
            }
            else if (DataHolder.WinFlag == -1)
            {
                endText.text = "LOSE";
                DataHolder.ClientTCP.SendMassage("L");
            }
            else if (DataHolder.WinFlag == 2)
            {
                endText.text = "DRAWN";
                DataHolder.ClientTCP.SendMassage("D");
            }
            DataHolder.WinFlag = 0;
        }

        if (!DataHolder.Connected)
        {
            DataHolder.notifText = "Разрыв соединения.";
            DataHolder.showNotif = true;
        }
        
    }

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void FastExit()
    {
        fastExitPanel.SetActive(!fastExitPanel.activeSelf);
        //if (game1)
        //{
            DataHolder.canMove = !DataHolder.canMove;
        //}
    }

    public void AcceptExit()
    {
        DataHolder.ClientTCP.SendMassage("C");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}