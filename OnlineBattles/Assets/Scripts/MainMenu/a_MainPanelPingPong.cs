using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class a_MainPanelPingPong : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject _mainButtons, _flyingBall;
    [SerializeField] private GameObject _leftLine, _rightLine;
    [SerializeField] private float _rangeX = 420f, _rangeY = 240f;
    private Vector2 _startPosition, _targetPosition;
    private Vector2 _leftStart, _rightStart;
    private float _time = 0f, _speed = 1;
    private int _dir = 1;
    private bool _zipMenu = false;
    private Coroutine _zoomAnimation = null;
    private CanvasGroup _buttonsGroup;

    void Start()
    {
        PingPongLinesTrigger.MyTriggerEnter += ChangeDirection;
        _buttonsGroup = _mainButtons.GetComponent<CanvasGroup>();
        _mainButtons.SetActive(true);
        _flyingBall.SetActive(false);

        _startPosition = _mainButtons.transform.localPosition;
        _targetPosition = new Vector2(_rangeX * _dir, Random.Range(-_rangeY, _rangeY));
        _leftStart = _leftLine.transform.localPosition;
        _rightStart = _rightLine.transform.localPosition;
    }

    
    void FixedUpdate()
    {
        if (_time < 1f)
        {
            _mainButtons.transform.localPosition = Vector3.Lerp(_startPosition, _targetPosition, _time);
            _flyingBall.transform.localPosition = _mainButtons.transform.localPosition;
            if (_dir == 1)
            {
                _rightLine.transform.localPosition = Vector3.Lerp(_rightStart, new Vector3(_rangeX, _targetPosition.y), _time);
                _leftLine.transform.localPosition = Vector3.Lerp(_leftStart, new Vector3(_leftStart.x, Mathf.PingPong(Time.time, _rangeY * 2) - _rangeY, 0), _time);
            }
            else
            {
                _leftLine.transform.localPosition = Vector3.Lerp(_leftStart, new Vector3(-_rangeX, _targetPosition.y), _time);
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
        if (_time > 0.2f) // Костыль, чтоб при увеличении не ударился несколько раз за мгновение
        {
            _time = 0f;
            _startPosition = _mainButtons.transform.localPosition;
            _dir *= -1;
            _targetPosition = new Vector3(_rangeX * _dir, Random.Range(-_rangeY, _rangeY), 0);
            _leftStart = _leftLine.transform.localPosition;
            _rightStart = _rightLine.transform.localPosition;
        }
    }

    private IEnumerator ZoomAnimation()
    {
        _buttonsGroup.interactable = false;
        if (!_zipMenu)
        {
            _mainButtons.SetActive(true);
            _flyingBall.SetActive(false);
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
            _mainButtons.SetActive(false);
            _flyingBall.SetActive(true);
        }
        _buttonsGroup.interactable = true;
    }

    private void OnDestroy()
    {
        PingPongLinesTrigger.MyTriggerEnter -= ChangeDirection;
    }
}
