using System.Collections.Generic;
using UnityEngine;

public class Player: MonoBehaviour
{
    [SerializeField] GameResourcesTemplate.PlayerType _playerType;
    [SerializeField] private Joystick _joystick;
    [SerializeField] private EdgeCollider2D _trailCollider;
    [SerializeField] private ParticleSystem _explosion;
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private float _trailTime = 3.2f;

    private Rigidbody2D _rb;
    private GameResources_2 GR;
    private List<Vector2> _points = new List<Vector2>();
    private List<Vector2> _bufferPoints = new List<Vector2>();
    private float _timer = 0f;
    private float _currentAngle = 0f;

    private const float MoveSpeed = 1.8f;
    private const float RotateSpeed = 300f;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        GR = GameResources_2.GameResources;
        GR.StopTrail += StopTrail;
        GR.ResumeTrail += ResumeTrail;
    }   

    private void Update()
    {
        if (GR.GameOn)
        {
            CreateTrailsCollider();
            ChangeDirection();
        }       
    }

    private void FixedUpdate()
    {
        if (GR.GameOn)
        {
            _rb.MovePosition(transform.position + gameObject.transform.up * Time.fixedDeltaTime * MoveSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
        {
            _explosion.Play();
            GR.RoundResults(null);
        }
    }

    public void LoseRound()
    {
        _explosion.Play();
        GR.RoundResults(gameObject);    
    }

    public void StopTrail()
    {
        _trail.time = Mathf.Infinity;
    }

    public void ResumeTrail()
    {
        _trail.time = _trailTime;
    }

    private void CreateTrailsCollider()
    {
        _timer += Time.deltaTime;
        if (_timer >= 0.1f)
        {
            Vector3 position = transform.localPosition;
            _bufferPoints.Add(new Vector2(position.x, position.y));

            if (_bufferPoints.Count >= 3) // Чтобы пропустить первые точки, во избежание стоолкновений линии и персонажа
            {
                _points.Add(_bufferPoints[0]);
                _bufferPoints.RemoveAt(0);
            }

            if (_points.Count > 30)
                _points.RemoveAt(0);

            _trailCollider.SetPoints(_points);
            _timer = 0f;
        }
    }

    private void ChangeDirection()
    {
        Vector2 normalizedJoystick = Vector2.zero;
        if (_playerType == GameResourcesTemplate.PlayerType.BluePlayer)
            normalizedJoystick = new Vector2(_joystick.Horizontal, _joystick.Vertical).normalized;
        else if (_playerType == GameResourcesTemplate.PlayerType.RedPlayer)
            normalizedJoystick = new Vector2(-_joystick.Horizontal, -_joystick.Vertical).normalized;

        if (normalizedJoystick != Vector2.zero)
        {
            float targetAngle = Vector2.SignedAngle(Vector2.up, normalizedJoystick);

            if (_currentAngle != targetAngle)
            {
                float offset = _currentAngle - targetAngle;
                if (offset < -180)
                    _currentAngle += 360f;
                else if (offset > 180)
                    targetAngle += 360f;

                float step = Time.deltaTime * RotateSpeed;
                if (_currentAngle > targetAngle)
                {
                    _currentAngle -= step;
                    transform.Rotate(0f, 0f, -step);
                }
                else
                {
                    _currentAngle += step;
                    transform.Rotate(0f, 0f, step);
                }
            }
        }
    }

    private void OnDestroy()
    {
        GR.StopTrail -= StopTrail;
        GR.ResumeTrail -= ResumeTrail;
    }
}
