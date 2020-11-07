using UnityEngine;
using UnityEngine.UI;

public class OnlineGameController : MonoBehaviour
{
    public GameObject endPanel, fastExitPanel, notifPanel, Shield;
    public Text endText;
    

    private void Start()
    {
        
    }


    private void Update()
    {
        while (DataHolder.MessageTCP.Count > 0)
        {
            // Обработка полученных от сервера сообщений
            string[] mes = DataHolder.MessageTCP[0].Split(' ');
            switch (mes[0])
            {
                //TODO: Добавить кейсы всех игр
                case "1":
                    DataHolder.MessageTCPforGame.Add(DataHolder.MessageTCP[0]);
                    break;

                case "N":
                    DataHolder.MessageTCPforGame.Add(DataHolder.MessageTCP[0]);                   
                    break;

                //case "S":
                //    DataHolder.canMove = true;
                //    DataHolder.timerT.GetComponent<timer>().StartTimer();
                //    break;

                case "E":
                    DataHolder.CanMove = false;
                    DataHolder.TimerT.GetComponent<timer>().StopGameTimer();
                    endPanel.SetActive(true);
                    break;

                case "A":
                    DataHolder.CanMove = false;
                    endText.text = "Победа, соперник вышел";
                    DataHolder.TimerT.GetComponent<timer>().StopGameTimer();
                    endPanel.SetActive(true);
                    break;

                case "V":
                    DataHolder.CanMove = false;
                    endText.text = "Техническое поражение";
                    DataHolder.TimerT.GetComponent<timer>().StopGameTimer();
                    endPanel.SetActive(true);
                    break;

                case "C":
                    DataHolder.CanMove = false;
                    endText.text = "Игра не началась";
                    DataHolder.TimerT.GetComponent<timer>().StopGameTimer();
                    endPanel.SetActive(true);
                    break;
            }
            DataHolder.MessageTCP.RemoveAt(0);
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
            DataHolder.CanMove = !DataHolder.CanMove;
        //}
    }

    public void AcceptExit()
    {
        DataHolder.ClientTCP.SendMassage("C");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}