using System.Collections;
using TMPro;
using UnityEngine;

public class StartScreenTimer : MonoBehaviour
{
    public event DataHolder.Notification StartGame;

    [SerializeField] private TMP_Text _text;
    private Coroutine _countDown = null, _fontScaler = null;   
    private float _time = 0f;

    private const float FontSize = 350f;
    private const float CountTime = 3f;

    public void StartTimer()
    {
        _time = CountTime + 1;
        _text.gameObject.SetActive(true);

        if (_countDown != null)
            StopCoroutine(_countDown);

        _countDown = StartCoroutine(CountDown());
    }

    public void StopTimer()
    {
        StopCoroutine(_countDown);
        StopCoroutine(_fontScaler);
        _text.gameObject.SetActive(false);
    }

    private void StartScaling()
    {
        if (_fontScaler != null)
            StopCoroutine(_fontScaler);

        _fontScaler = StartCoroutine(ScaleFontSize());
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
                    StartScaling();
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
}
