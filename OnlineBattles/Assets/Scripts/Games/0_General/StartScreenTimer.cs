using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StartScreenTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    private Coroutine _countDown = null, _fontScaler = null;
    private const float FontSize = 350f;
    private float _time = 0f;

    public void StartTimer(float time, Action setOnComplete)
    {
        _time = time + 1;
        _text.gameObject.SetActive(true);
        _countDown = StartCoroutine(CountDown(setOnComplete));
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

    private IEnumerator CountDown(Action setOnComplete)
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

        if (setOnComplete != null)
            setOnComplete();
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
