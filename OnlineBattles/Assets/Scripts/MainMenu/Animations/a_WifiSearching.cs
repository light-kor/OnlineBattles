using UnityEngine;
using UnityEngine.UI;

public class a_WifiSearching : MonoBehaviour
{

    [SerializeField] private GameObject _circle1, _circle2, _circle3, _circle4;
    [SerializeField] private int _speed = 10;
    private Image[] _listOfCircleImages;
    private int _select = 0;
    private bool _emergence = true;

    private void OnEnable()
    {
        _listOfCircleImages = new Image[4] { _circle1.GetComponent<Image>(), _circle2.GetComponent<Image>(), _circle3.GetComponent<Image>(), _circle4.GetComponent<Image>() };

        //Делаем все прозрачными при старте скрипта
        Color buffer = _listOfCircleImages[0].color;
        buffer.a = 0f;
        for (int i = 0; i < _listOfCircleImages.Length; i++)
        {
            _listOfCircleImages[i].color = buffer;
        }

        _select = 0;
    }

    private void Update()
    {
        if (_select == _listOfCircleImages.Length)
        {
            _select = 0;
            _emergence = !_emergence;
        }
            
        Color buffer = _listOfCircleImages[_select].color;
        if (_emergence)
        {
            if (buffer.a < 0.7f)
            {
                buffer.a += Time.deltaTime / 10 * _speed;
                _listOfCircleImages[_select].color = buffer;
            }
            else
                _select++;
        }
        else
        {
            if (buffer.a > 0f)
            {
                buffer.a -= Time.deltaTime / 10 * _speed;
                _listOfCircleImages[_select].color = buffer;
            }
            else
                _select++;
        }        
    }
}
