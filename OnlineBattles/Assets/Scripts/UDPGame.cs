using System;
using UnityEngine;

public class UDPGame : MonoBehaviour
{
    private const int delay = 100 * 10000; // 100ms для интерполяции
    public Joystick joystick;
    public GameObject me, enemy;
    public float UpdateRate = 0.05f; //TODO: Как часто клиенты должны слать свои изменения. Надо  как-то чекать это на стороне сервра. Чтоб нельзя было так читерить.
    private float buffX = 0, buffY = 0;
    private bool _finishTheGame = false;
    private bool _gameOn = true;

    private void Start()
    {
        DataHolder.game = this;
        Network.CreateUDP();       
        InvokeRepeating("SendJoy", 1.0f, UpdateRate);
        DataHolder.ClientTCP.SendMessage("start");
        DataHolder.ClientUDP.SendMessage("start"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
    }

    private void Update()
    {
        if (DataHolder.MessageTCPforGame.Count > 0 && _gameOn)
        {
            string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');
            Debug.Log($"game {DataHolder.MessageTCPforGame[0]}");
            if (mes[0] == "info")
            {
                me.transform.position = new Vector2(float.Parse(mes[2]), float.Parse(mes[3]));
                enemy.transform.position = new Vector2(float.Parse(mes[4]), float.Parse(mes[5]));
            }
                       
            DataHolder.MessageTCPforGame.RemoveAt(0);
        }

        if (_finishTheGame)
        {
            CloseAll();
            _finishTheGame = false;
        }
            

        UpdateThread();
    }

    public void FinishTheGame()
    {
        _finishTheGame = true;
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
        if (DataHolder.MessageUDPget.Count > 1 && _gameOn)
        {
            string[] frame1 = DataHolder.MessageUDPget[0].Split(' ');
            string[] frame2 = DataHolder.MessageUDPget[1].Split(' ');

            long time = Convert.ToInt64(frame1[0]);
            long time2 = Convert.ToInt64(frame2[0]);
            long vrem = DateTime.UtcNow.Ticks + DataHolder.TimeDifferenceWithServer - delay;

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

    private void CloseAll()
    {
        _gameOn = false;
        CancelInvoke("SendJoy");
        // Там автоматически после GameOn = false вызовется CloseClient()
        if (DataHolder.ClientUDP != null)
        {
            DataHolder.ClientUDP.GameOn = false;
            DataHolder.ClientUDP.CloseClient();
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
