using System;
using UnityEngine;

public class UDPGame : MonoBehaviour
{
    private const int delay = 100 * 10000; // 100ms для интерполяции
    public Joystick joystick;
    public GameObject me, enemy;
    public float UpdateRate = 0.05f; //TODO: Как часто клиенты должны слать свои изменения. Надо  как-то чекать это на стороне сервра. Чтоб нельзя было так читерить.
    private float buffX = 0, buffY = 0;

    private void Start()
    {       
        DataHolder.NetworkScript.CreateUDP();       
        InvokeRepeating("SendJoy", 1.0f, UpdateRate);
        DataHolder.ClientTCP.SendMessage("start");
        DataHolder.ClientUDP.SendMessage("start"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
    }

    private void Update()
    {
        if (DataHolder.MessageTCPforGame.Count > 0)
        {
            string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');
            Debug.Log($"game {DataHolder.MessageTCPforGame[0]}");
            if (mes[0] == "info")
            {
                me.transform.position = new Vector2(float.Parse(mes[1]), float.Parse(mes[2]));
                enemy.transform.position = new Vector2(float.Parse(mes[3]), float.Parse(mes[4]));
            }
                       
            DataHolder.MessageTCPforGame.RemoveAt(0);
        }
        UpdateThread();
    }

    //private void UpdateWorld()
    //{
    //    //TODO: А если накопилось уже больше одного, то мб стоит удалить несколько или обработать несколько с плавным переходом
    //    string[] frame1 = DataHolder.MessageUDPget[0].Split(' ');
    //    string[] frame2 = DataHolder.MessageUDPget[1].Split(' ');

    //    me.transform.position = new Vector2(float.Parse(frame1[1]), float.Parse(frame1[2]));
    //    enemy.transform.position = new Vector2(float.Parse(frame1[3]), float.Parse(frame1[4]));
    //}

    private void UpdateThread()
    {
        if (DataHolder.MessageUDPget.Count > 1)
        {
            string[] frame1 = DataHolder.MessageUDPget[0].Split(' ');
            string[] frame2 = DataHolder.MessageUDPget[1].Split(' ');

            long time = Convert.ToInt64(frame1[0]);
            long time2 = Convert.ToInt64(frame2[0]);
            long vrem = DataHolder.ServerTime - delay + DataHolder.NetworkScript.stopWatch.ElapsedTicks;

            if (time < vrem && vrem < time2)
            {
                //normalized = (x - min(x)) / (max(x) - min(x));
                float delta = (vrem - time) / (time2 - time);
                me.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[1]), float.Parse(frame1[2])), new Vector2(float.Parse(frame2[1]), float.Parse(frame2[2])), delta);
                enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[3]), float.Parse(frame1[4])), new Vector2(float.Parse(frame2[3]), float.Parse(frame2[4])), delta);
            }
            else if (time > vrem) return;

            DataHolder.MessageUDPget.RemoveAt(0);
        }
    }


    void SendJoy()
    {
        buffX = joystick.Horizontal;
        buffY = joystick.Vertical;
        if (buffX != 0 && buffY != 0)
        {
            DataHolder.ClientUDP.SendMessage($"{buffX} {buffY}"); //TODO: Проверять на сервере, что число от 0 до 1
        }
    }

    public void CloseAll()
    {
        CancelInvoke("SendJoy");
        // Там автоматически после GameOn = false вызовется CloseClient()
        if (DataHolder.ClientUDP != null)
        {
            DataHolder.ClientUDP.GameOn = false;
            DataHolder.ClientUDP = null;
        }
    }

    public void GiveUp()
    {
        DataHolder.ClientTCP.SendMessage("leave");
    }

    public void ExitGame()
    {        
        if (DataHolder.ClientUDP != null)
            CloseAll();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
