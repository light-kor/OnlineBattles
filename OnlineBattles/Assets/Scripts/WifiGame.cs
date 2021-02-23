using System;
using UnityEngine;

public class WifiGame : MonoBehaviour
{
    private const int _delay = 100 * 10000; // 100ms для интерполяции
    private bool _finishTheGame = false;
    private bool _gameOn = true;
    public Joystick joystick;
    public GameObject me, enemy;
    public float UpdateRate = 0.05f; //TODO: Как часто клиенты должны слать свои изменения. Надо  как-то чекать это на стороне сервра. Чтоб нельзя было так читерить.
    private float buffX = 0, buffY = 0;

    private void Start()
    {
        Network.EndOfGame += FinishTheGame;
        Network.CreateUDP();
        DataHolder.MessageUDPget.Clear();
        DataHolder.ClientUDP.SendMessage("start"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
        InvokeRepeating("SendJoy", 1.0f, UpdateRate);
    }

    private void Update()
    {
        //if (DataHolder.MessageTCPforGame.Count > 0 && _gameOn)
        //{
        //    string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');
        //    if (mes[0] == "info")
        //    {
        //        me.transform.position = new Vector2(float.Parse(mes[2]), float.Parse(mes[3]));
        //        enemy.transform.position = new Vector2(float.Parse(mes[4]), float.Parse(mes[5]));
        //    }

        //    DataHolder.MessageTCPforGame.RemoveAt(0);
        //}

        //if (_finishTheGame)
        //{
        //    CloseAll();
        //    _finishTheGame = false;
        //}

        //UpdateThread();

        if (DataHolder.MessageUDPget.Count > 0 && _gameOn)
        {
            Debug.Log(DataHolder.MessageUDPget[0]);
            DataHolder.MessageUDPget.RemoveAt(0);
        }
    }


    private void UpdateThread()
    {
        if (DataHolder.MessageUDPget.Count > 1 && _gameOn)
        {
            string[] frame1 = DataHolder.MessageUDPget[0].Split(' ');
            string[] frame2 = DataHolder.MessageUDPget[1].Split(' ');

            long time = Convert.ToInt64(frame1[0]);
            long time2 = Convert.ToInt64(frame2[0]);
            long vrem = DateTime.UtcNow.Ticks + DataHolder.TimeDifferenceWithServer - _delay;

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
            Debug.Log("Send");
            DataHolder.ClientUDP.SendMessage($"{buffX} {buffY}", true); //TODO: Проверять на сервере, что число от 0 до 1
        }
    }

    private void BaseOnlineFunctions()
    {
        Network.ConnectionLifeSupport();
    }

    private void FinishTheGame()
    {
        _finishTheGame = true;
    }

    private void GiveUp()
    {
        DataHolder.ClientTCP.SendMessage("leave");
    }

    private void CloseAll()
    {
        _gameOn = false;
        CancelInvoke(); //TODO: Временное решение для первой игры
        Network.EndOfGame -= FinishTheGame;

        // Там автоматически после GameOn = false вызовется CloseClient()
        Network.CloseUdpConnection();
    }


    private void ExitGame()
    {
        if (DataHolder.ClientUDP != null)
            CloseAll();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
