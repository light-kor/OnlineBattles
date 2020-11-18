using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Joystic_controller : MonoBehaviour
{
    public Joystick joystick;
    public GameObject me, enemy;
    public float UpdateRate = 0.05f; //TODO: Как часто клиенты должны слать свои изменения
    private float buffX = 0, buffY = 0;
    private Network NetworkScript;
    private DateTime LastSend;
    double delta = 0f;
    //private Coroutine sss;

    private void Start()
    {
        NetworkScript = GetComponent<Network>();
        NetworkScript.CreateUDP();       
        InvokeRepeating("SendJoy", 1.0f, UpdateRate);
        DataHolder.ClientUDP.SendMessage($"2 {DataHolder.GameId} {DataHolder.ThisGameID} {buffX} {buffY}");
        LastSend = DateTime.UtcNow;


        if (DataHolder.MessageTCP.Count > 0)
        {
            string[] mes = DataHolder.MessageTCP[0].Split(' ');
            if (mes[0] == "info")
            {             
                me.transform.position = new Vector2(float.Parse(mes[1]), float.Parse(mes[2]));
                enemy.transform.position = new Vector2(float.Parse(mes[3]), float.Parse(mes[4]));
            }
            DataHolder.MessageTCP.RemoveAt(0);
        }
        //DataHolder.ClientUDP.UpdateUdpInfo += ClientUDP_UpdateUdpInfo;

        //sss = StartCoroutine(DrtetoMove());
    }

    //private void ClientUDP_UpdateUdpInfo()
    //{
       
    //}

    private void Update()
    {
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

    //private IEnumerator DrtetoMove()
    //{        
    //    while (true)
    //    {
    //        if (DataHolder.MessageUDPget.Count > 1)
    //        {
    //            string[] frame1 = DataHolder.MessageUDPget[0].Split(' ');
    //            string[] frame2 = DataHolder.MessageUDPget[1].Split(' ');

    //            long time = Convert.ToInt64(frame1[0]);
    //            long time2 = Convert.ToInt64(frame2[0]);
    //            long vrem = DateTime.UtcNow.Ticks - 1000000; //TODO: Вынести константу

    //            if (time < vrem && vrem < time2)
    //            {
    //                //normalized = (x - min(x)) / (max(x) - min(x));
    //                double delta = 0f;
    //                while (delta < 1f)
    //                {
    //                    vrem = DateTime.UtcNow.Ticks - 1000000;
    //                    delta = (vrem - time) / (time2 - time);
    //                    me.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[1]), float.Parse(frame1[2])), new Vector2(float.Parse(frame2[1]), float.Parse(frame2[2])), (float)delta);
    //                    enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[3]), float.Parse(frame1[4])), new Vector2(float.Parse(frame2[3]), float.Parse(frame2[4])), (float)delta);

    //                    yield return null;
    //                }
    //            }
    //            else if (time > vrem) continue;

    //            DataHolder.MessageUDPget.RemoveAt(0);

    //        } else yield return null;
    //    }
    //}

    private void UpdateThread()
    {
        if (DataHolder.MessageUDPget.Count > 1)
        {
            string[] frame1 = DataHolder.MessageUDPget[0].Split(' ');
            string[] frame2 = DataHolder.MessageUDPget[1].Split(' ');

            long time = Convert.ToInt64(frame1[0]);
            long time2 = Convert.ToInt64(frame2[0]);
            long vrem = DateTime.UtcNow.Ticks - 1000000; //TODO: Вынести константу

            if (time < vrem && vrem < time2)
            {
                //normalized = (x - min(x)) / (max(x) - min(x));
                delta = (vrem - time) / (time2 - time);
                if (delta < 1f)
                {                   
                    me.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[1]), float.Parse(frame1[2])), new Vector2(float.Parse(frame2[1]), float.Parse(frame2[2])), (float)delta);
                    enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[3]), float.Parse(frame1[4])), new Vector2(float.Parse(frame2[3]), float.Parse(frame2[4])), (float)delta);
                }
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
            //DataHolder.ClientUDP.SendMessage(Math.Round(joystick.Horizontal, 2) + " " + Math.Round(joystick.Vertical, 2));
            DataHolder.ClientUDP.SendMessage($"2 {DataHolder.GameId} {DataHolder.ThisGameID} {buffX} {buffY}"); //TODO: Проверять на сервере, что число от 0 до 1
            LastSend = DateTime.UtcNow;
        }

        if ((DateTime.UtcNow - LastSend).TotalMilliseconds > 500)
        {
            DataHolder.ClientUDP.SendMessage("Y");
            LastSend = DateTime.UtcNow;
            //TODO: При получении сообщения от любого из игроков, чекнуть, когда пришло послденее сообщение от второго, и елси оно было больше секнды назад, то остановить игру
        }
            
    }

    public void CloseAll()
    {
        StopAllCoroutines();
        CancelInvoke("SendJoy");
        // Там автоматически после GameOn = false вызовется CloseClient()
        if (DataHolder.ClientUDP != null)
        {
            DataHolder.ClientUDP.GameOn = false;
            DataHolder.ClientUDP = null;
        }
    }

    public void ExitGame()
    {
        //TODO: Отправить что-то при досрочном завершении 
        if (DataHolder.ClientUDP != null)
            CloseAll();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
