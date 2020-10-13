using UnityEngine;
using UnityEngine.UI;

public class timer : MonoBehaviour
{
    private Text timerT;
    private sbyte goTimer = 0;
    [SerializeField]
    private float time = 30f;
    private float timeLeft;

    void Start()
    {      
        timerT = GetComponent<Text>();
        timeLeft = time;
    }
    void Update()
    {
        if (goTimer == 1)
        {
            timeLeft -= Time.deltaTime;
            timerT.text = System.Convert.ToInt32(timeLeft).ToString();
            if (timeLeft < 0)
            {
                goTimer = 0;
                timerT.text = "Время вышло!";
                timeLeft = time;
            }
            
        }

        if (goTimer == -1)
        {
            goTimer = 0;
            timerT.text = "Ожидание противника";
            timeLeft = time;
        }

        if (goTimer == 3)
        {
            goTimer = 0;
            timerT.text = "Игра окончена";
            timeLeft = time;
        }
    }

    /// <summary>
    /// Проверка времени на положительность
    /// </summary>
    /// <returns></returns>
    public bool TimeLeftCheck()
    {
        if (goTimer == 0)
            return false;
        else return true;
    }

    /// <summary>
    /// Старт таймера
    /// </summary>
    public void StartTimer()
    {
        goTimer = 1;
    }

    /// <summary>
    /// Ожидание хода противника
    /// </summary>
    public void StopTimer()
    {
        goTimer = -1;
    }

    /// <summary>
    /// Окончание игры, остановка таймера
    /// </summary>
    public void StopGameTimer()
    {
        goTimer = 3;
    }
}
