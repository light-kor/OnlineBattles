using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Joystic_controller : MonoBehaviour
{
    public Joystick joystick;
    public GameObject me, enemy;


    private bool startGame = false;
    private int myNum, enNum;

    private void Start()
    {
        DataHolder.CreateUDP();
        if (DataHolder.thisGameID == 1)
        {
            myNum = 0;
            enNum = 2;
        }            
        else
        {
            myNum = 2;
            enNum = 0;
        }
        TimerCallback SendUdp = new TimerCallback(SendJoy);
        Timer timer = new Timer(SendUdp, null, 0, 33);
    }

    //private void FixedUpdate()
    //{
    //    X += joystick.Horizontal / 10;
    //    Y += joystick.Vertical / 10;
    //    transform.position = new Vector2(X, Y);

    //}

    private void Update()
    {
        while (DataHolder.messageUDPget.Count > 0)
        {
            //TODO: А если накопилось уже больше одного, то мб стоит удалить несколько или обработать несколько с плавным переходом
            string[] mes = DataHolder.messageUDPget[0].Split(' ');

            transform.position = new Vector2(float.Parse(mes[myNum]), float.Parse(mes[myNum + 1]));
            enemy.transform.position = new Vector2(float.Parse(mes[enNum]), float.Parse(mes[enNum + 1]));
            
            DataHolder.messageUDPget.RemoveAt(0);
        }

    }

    void SendJoy(object ey)
    {
        if (startGame)
        {           
            DataHolder.ClientUDP.SendMessage(Math.Round(joystick.Horizontal, 2) + " " + Math.Round(joystick.Vertical, 2));
        }
    }

    public void ExitGame()
    {
        // Нормально завершить поток, а потом очистить экземпляр класса
        DataHolder.ClientUDP.GameOn = false;
    }
}
