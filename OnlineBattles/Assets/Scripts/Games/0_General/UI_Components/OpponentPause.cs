using TMPro;
using UnityEngine;

public class OpponentPause : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private int _sign = 1;
    private float _fontSize, _currentFontSize;

    private const float AnimSpeed = 7f;

    private void Start()
    {
        _fontSize = _text.fontSize;
        _currentFontSize = _fontSize;
    }

    private void Update()
    {
        _currentFontSize += Time.deltaTime * _sign * AnimSpeed;
        _text.fontSize = _currentFontSize;

        if (_currentFontSize > _fontSize * 1.05f)
            _sign = -1;
        else if (_currentFontSize < _fontSize * 0.95f)
            _sign = 1;
    }
}
