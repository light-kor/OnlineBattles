using UnityEngine;

public class MainPanelAnimation : MonoBehaviour
{
    [SerializeField]
    private GameObject _leftLine, _rightLine;
    private int _rangeX = 260, _rangeY = 240, _dir = 1, _linesX = 480;
    public Vector3 _startPosition, _targetPosition;
    public Vector3 _leftStart, _rightStart;
    private float _time = 0.5f; // тк сцена стартует с середины

    void Start()
    {
        _startPosition = transform.localPosition;
        _targetPosition = new Vector3(_rangeX * _dir, Random.Range(-_rangeY, _rangeY), 0);

        _leftStart = _leftLine.transform.localPosition;
        _rightStart = _rightLine.transform.localPosition;
    }

    void Update()
    {
        if (_time < 1f)
        {
            transform.localPosition = Vector3.Lerp(_startPosition, _targetPosition, _time);
            if (_dir == 1)
            {
                _rightLine.transform.localPosition = Vector3.Lerp(_rightStart, new Vector3(_linesX, _targetPosition.y), _time);
                _leftLine.transform.localPosition = Vector3.Lerp(_leftStart, new Vector3(_leftStart.x, Mathf.PingPong(Time.time, _rangeY * 2) - _rangeY, 0), _time);
            }
            else
            {
                _leftLine.transform.localPosition = Vector3.Lerp(_leftStart, new Vector3(-_linesX, _targetPosition.y), _time);
                _rightLine.transform.localPosition = Vector3.Lerp(_rightStart, new Vector3(_rightStart.x, Mathf.PingPong(Time.time, _rangeY * 2) - _rangeY, 0), _time);
            }

            _time += Time.deltaTime / 4;
        }
        else
        {
            _time = 0f;
            _startPosition = transform.localPosition;
            _dir *= -1;
            _targetPosition = new Vector3(_rangeX * _dir, Random.Range(-_rangeY, _rangeY), 0);
            _leftStart = _leftLine.transform.localPosition;
            _rightStart = _rightLine.transform.localPosition;
        }      
    }
}
