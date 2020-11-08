using UnityEngine;

public class Joystic_controller : MonoBehaviour
{
    public Joystick joystick;
    public GameObject me, enemy;
    public float repTime = 0.034f;
    private float buffX = 0, buffY = 0;
    private Network NetworkScript;

    private void Start()
    {
        NetworkScript = GetComponent<Network>();
        NetworkScript.CreateUDP();       
        InvokeRepeating("SendJoy", 1.0f, repTime);
        DataHolder.ClientUDP.SendMessage($"2 {DataHolder.GameId} {DataHolder.ThisGameID} {buffX} {buffY}");

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
    }

    private void FixedUpdate()
    {
        if (DataHolder.MessageUDPget.Count > 0)
        {
            //TODO: А если накопилось уже больше одного, то мб стоит удалить несколько или обработать несколько с плавным переходом
            string[] mes = DataHolder.MessageUDPget[0].Split(' ');
            me.transform.position = new Vector2(float.Parse(mes[0]), float.Parse(mes[1]));
            enemy.transform.position = new Vector2(float.Parse(mes[2]), float.Parse(mes[3]));

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
            DataHolder.ClientUDP.SendMessage($"2 {DataHolder.GameId} {DataHolder.ThisGameID} {buffX} {buffY}");
        }
    }

    public void CloseAll()
    {
        CancelInvoke("SendJoy");
        DataHolder.ClientUDP.GameOn = false;
        DataHolder.ClientUDP.CloseClient();
        DataHolder.ClientUDP = null;
    }

    public void ExitGame()
    {
        //TODO: Отправить что-то при досрочном завершении    
        CloseAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
