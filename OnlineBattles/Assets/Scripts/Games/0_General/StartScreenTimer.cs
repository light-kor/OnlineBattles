using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class StartScreenTimer : MonoBehaviour
{
    public event UnityAction StartGame;
    public event UnityAction RestartLevel;

    [SerializeField] private TMP_Text _text;
    [SerializeField] private GeneralUIResources _generalUI;
    [SerializeField] private bool _turnOnStartTimer = true;

    private Coroutine _countDown = null, _fontScaler = null, _backgroundTimer = null;   
    private float _time = 0f;
    private bool _firstStartDone = false;

    private const float FontSize = 350f;
    private const float CountTime = 3f;
    private const float TimeBetweenRounds = 2.5f;

    public void TryStartTimer()
    {
        if (_turnOnStartTimer)
            StartTimer();
        else
            StartGame?.Invoke();
    }

    private void StartTimer()
    {
        _time = CountTime + 1;

        if (_firstStartDone == false)
        {
            _text.gameObject.SetActive(true);
            CleanAndStartCoroutine(ref _countDown, CountDown());
        }
        else
            CleanAndStartCoroutine(ref _backgroundTimer, LevelRestarting());    
    }

    private IEnumerator LevelRestarting()
    {
        float time = 0f;
        bool restarted = false, flashed = false; ;

        while (time < TimeBetweenRounds)
        {
            time += Time.deltaTime;

            if (time > 1.5f && flashed == false)
            {
                _generalUI.EndRoundFlashAnimation();
                flashed = true;
            }

            if (time > 2f && restarted == false)
            {
                RestartLevel?.Invoke();
                restarted = true;
            }

            yield return null;
        }
        StartGame?.Invoke();
    }

    private IEnumerator CountDown()
    {
         int count = (int)_time + 1;

        while (_time > 0f)
        {
            _time -= Time.deltaTime * 1.2f;
          
            if ((int)_time < count)
            {
                count--;

                if (count != 0)
                {
                    _text.text = count.ToString();
                    CleanAndStartCoroutine(ref _fontScaler, ScaleFontSize());
                }
                else
                {
                    _text.text = "Start!";
                    StopCoroutine(_fontScaler);
                    _text.fontSize = 150f;
                }                  
            }
            yield return null;
        }               
        _text.gameObject.SetActive(false);

        _firstStartDone = true;
        StartGame?.Invoke();
    }

    private IEnumerator ScaleFontSize()
    {
        float fontSize = FontSize;
        while (true)
        {
            fontSize += Time.deltaTime * 100;
            _text.fontSize = fontSize;
            yield return null;
        }
    }

    private void CleanAndStartCoroutine(ref Coroutine coroutine, IEnumerator enumerator)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(enumerator);
    }

    public void StopTimer()
    {
        if (_countDown != null)
            StopCoroutine(_countDown);
        if (_fontScaler != null)
            StopCoroutine(_fontScaler);
        if (_backgroundTimer != null)
            StopCoroutine(_backgroundTimer);
        _text.gameObject.SetActive(false);
    }
}
