using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPGame : MonoBehaviour
{
    public GameObject enemy;
    private Network NetworkScript;
    bool startGame = false;

    public float repTime = 0.04f;

    private void Start()
    {
        NetworkScript = GetComponent<Network>();
        NetworkScript.CreateUDP();
        InvokeRepeating("SendAndGetPos", 1.0f, repTime);
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Moved:
                    var pos = Camera.main.ScreenToWorldPoint(touch.position);
                    pos.z = transform.position.z;
                    transform.position = pos;
                    break;
            }
        }

        if (DataHolder.MessageUDPget.Count > 0)
        {
            //TODO: А если накопилось уже больше одного, то мб стоит удалить несколько или обработать несколько с плавным переходом
            string[] mes = DataHolder.MessageUDPget[0].Split(' ');
            if (mes[0] != "" && mes[1] != "")
            {
                enemy.transform.position = new Vector3(float.Parse(mes[0]), float.Parse(mes[1]), 0);
            }

            DataHolder.MessageUDPget.RemoveAt(0);
        }

    }

    void SendAndGetPos()
    {
        string xpos = "", ypos = "";
        if (startGame)
        {
            if ((xpos != (float)Math.Round((0 - transform.position.x), 2) + "") || (ypos != (float)Math.Round(transform.position.y, 2) + ""))
            {
                xpos = (float)Math.Round((transform.position.x), 2) + "";
                ypos = (float)Math.Round(0 - transform.position.y, 2) + "";
                //TODO: Пока отправляем pos, но потом надо будет поменять на значения джостика и сделать на сервере проверку, что значение меньшн 1
                DataHolder.ClientUDP.SendMessage($"2 {DataHolder.GameId} {DataHolder.ThisGameID} {xpos} {ypos}");
            }           
        }
    }


    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

}
