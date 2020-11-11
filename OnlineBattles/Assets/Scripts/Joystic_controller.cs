using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joystic_controller : MonoBehaviour
{
    public Joystick joystick;
    public GameObject me, enemy;
    public float UpdateRate = 0.05f;
    private float buffX = 0, buffY = 0;
    private Network NetworkScript;
    DateTime LastSend;
    int FrameCount = 0;

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

        StartCoroutine(DoMove());
    }

    private void Update()
    {
        
    }

    //private void UpdateWorld()
    //{
    //    //TODO: А если накопилось уже больше одного, то мб стоит удалить несколько или обработать несколько с плавным переходом
    //    string[] frame1 = DataHolder.MessageUDPget[0].Split(' ');
    //    string[] frame2 = DataHolder.MessageUDPget[1].Split(' ');
    //    me.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[0]), float.Parse(frame1[1])), new Vector2(float.Parse(frame2[0]), float.Parse(frame2[1])), 1);
    //    enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[2]), float.Parse(frame1[3])), new Vector2(float.Parse(frame2[2]), float.Parse(frame2[3])), 1);

        
    //}


    private IEnumerator DoMove() 
    {
        while (true) // TODO: Добавить флаг
        {
            if (DataHolder.MessageUDPget.Count >= 3)
            {
                float time = UpdateRate;
                string[] frame1 = DataHolder.MessageUDPget[0].Split(' ');
                string[] frame2 = DataHolder.MessageUDPget[1].Split(' ');
                FrameCount = Convert.ToInt32(frame1[0]);

                if (Convert.ToInt32(frame2[0]) != FrameCount + 1)
                {
                    frame2 = DataHolder.MessageUDPget[2].Split(' ');
                    time *= 2;
                }

                float startTime = Time.realtimeSinceStartup;
                float fraction = 0f;
                while (fraction < 1f)
                {
                    fraction = Mathf.Clamp01((Time.realtimeSinceStartup - startTime) / time);
                    me.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[1]), float.Parse(frame1[2])), new Vector2(float.Parse(frame2[1]), float.Parse(frame2[2])), fraction);
                    enemy.transform.position = Vector2.Lerp(new Vector2(float.Parse(frame1[3]), float.Parse(frame1[4])), new Vector2(float.Parse(frame2[3]), float.Parse(frame2[4])), fraction);
                    yield return null;
                }

                DataHolder.MessageUDPget.RemoveAt(0);
            }
            else yield return null;
        }
    }


    void SendJoy()
    {
        buffX = joystick.Horizontal;
        buffY = joystick.Vertical;
        if (buffX != 0 && buffY != 0)
        {
            //DataHolder.ClientUDP.SendMessage(Math.Round(joystick.Horizontal, 2) + " " + Math.Round(joystick.Vertical, 2));
            DataHolder.ClientUDP.SendMessage($"2 {DataHolder.GameId} {DataHolder.ThisGameID} {buffX} {buffY}");
            LastSend = DateTime.UtcNow;
        }

        if ((DateTime.UtcNow - LastSend).TotalMilliseconds > 500)
        {
            DataHolder.ClientUDP.SendMessage("Y");
            LastSend = DateTime.UtcNow;
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

    public void ExitGame()
    {
        //TODO: Отправить что-то при досрочном завершении 
        if (DataHolder.ClientUDP != null)
            CloseAll();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
