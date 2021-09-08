using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class a_ScoreFlicker : MonoBehaviour
{
    [SerializeField] private GameObject _leftNumber1, _leftNumber2, _rightNumber1, _rightNumber2;
    private Image _leftNumber1_image, _leftNumber2_image, _rightNumber1_image, _rightNumber2_image;
    private float _timer = 0, _range = 1f, _timer2 = 0, _range2 = 1f;

    private void Start()
    {
        _leftNumber1_image = _leftNumber1.GetComponent<Image>();
        _leftNumber2_image = _leftNumber2.GetComponent<Image>();
        _rightNumber1_image = _rightNumber1.GetComponent<Image>();
        _rightNumber2_image = _rightNumber2.GetComponent<Image>();
    }


    private void Update()
    {
        if (_timer < _range)
        {
            _timer += Time.deltaTime;
        }
        else
        {
            _range = Random.Range(3, 10);
            _timer = 0;
            StartCoroutine(SlideNumbers(_leftNumber1_image, _leftNumber2_image));
        }

        if (_timer2 < _range2)
        {
            _timer2 += Time.deltaTime;
        }
        else
        {
            _range2 = Random.Range(3, 10);
            _timer2 = 0;
            StartCoroutine(SlideNumbers(_rightNumber1_image, _rightNumber2_image));
        }
    }

    private IEnumerator SlideNumbers(Image first, Image second)
    {
        float waitTime = 0;
        while (waitTime <= 1.5f)
        {
            waitTime += Time.deltaTime;

            first.fillAmount -= waitTime;
            if (waitTime > 0.1f)
                second.fillAmount += waitTime;

            yield return null;
        }
        first.fillAmount = 1f;
        second.fillAmount = 0f;
    }
}
