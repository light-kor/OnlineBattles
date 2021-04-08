using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainPanelAnimation : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] private GameObject _mainButtons, _flyingBall;
    [SerializeField] private GameObject _leftLine, _rightLine;
    private BoxCollider2D _leftBox, _rightBox;
    private readonly int _rangeX = 500, _rangeY = 240, _linesX = 480;
    private Vector2 _startPosition, _targetPosition;
    private Vector2 _leftStart, _rightStart;
    private float _time = 0f, _speed = 1;
    private int _dir = 1;
    private bool _zipMenu = false;
    private Coroutine _zoomAnimation = null;

    
    void Start()
    {
        MainButtonTrigger.MyTriggerEnter += ChangeDirection;
        _leftBox = _leftLine.GetComponent<BoxCollider2D>();
        _rightBox = _rightLine.GetComponent<BoxCollider2D>();
        _mainButtons.SetActive(true);
        _flyingBall.SetActive(false);

        _startPosition = _mainButtons.transform.localPosition;
        _targetPosition = new Vector2(_rangeX * _dir, Random.Range(-_rangeY, _rangeY));
        _leftStart = _leftLine.transform.localPosition;
        _rightStart = _rightLine.transform.localPosition;
    }

    
    void Update()
    {
        if (_time < 1f)
        {
            _mainButtons.transform.localPosition = Vector3.Lerp(_startPosition, _targetPosition, _time);
            _flyingBall.transform.localPosition = _mainButtons.transform.localPosition;
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

            _time += Time.deltaTime / 4 * _speed;
        }
        else
            ChangeDirection();

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_zoomAnimation != null)
            StopCoroutine(_zoomAnimation);

        _zipMenu = !_zipMenu;
        _zoomAnimation = StartCoroutine(ZoomAnimation());
    }

    private void ChangeDirection()
    {
        StartCoroutine(DisableTriggerForAWhile());
        _time = 0f;
        _startPosition = _mainButtons.transform.localPosition;
        _dir *= -1;
        _targetPosition = new Vector3(_rangeX * _dir, Random.Range(-_rangeY, _rangeY), 0);
        _leftStart = _leftLine.transform.localPosition;
        _rightStart = _rightLine.transform.localPosition;
    }

    private IEnumerator ZoomAnimation()
    {
        if (!_zipMenu)
        {
            _mainButtons.SetActive(!_zipMenu);
            _flyingBall.SetActive(_zipMenu);
        }

        Vector3 startScale = _mainButtons.transform.localScale;
        float animTime = 0;
        while (animTime < 1f)
        {
            animTime += Time.deltaTime;

            if (!_zipMenu)
            {
                _mainButtons.transform.localScale = Vector3.Lerp(startScale, new Vector3(1f, 1f, 1f), animTime);
                if (_speed > 1)
                    _speed -= Time.deltaTime;
            }
            else
            {
                _mainButtons.transform.localScale = Vector3.Lerp(startScale, new Vector3(0.1f, 0.1f, 1f), animTime);
                if (_speed < 2)
                    _speed += Time.deltaTime;
            }
            yield return null;
        }

        if (_zipMenu)
        {
            _mainButtons.SetActive(!_zipMenu);
            _flyingBall.SetActive(_zipMenu);
        }
    }

    private IEnumerator DisableTriggerForAWhile()
    {
        _leftBox.enabled = false;
        _rightBox.enabled = false;

        float waitTime = 0;
        while (waitTime < 1f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        _leftBox.enabled = true;
        _rightBox.enabled = true;
    }
}
