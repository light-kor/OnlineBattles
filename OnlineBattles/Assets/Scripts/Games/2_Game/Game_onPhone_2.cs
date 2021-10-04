using System.Collections.Generic;
using UnityEngine;

public class Game_onPhone_2 : MonoBehaviour
{
    public Joystick _firstJoystick, _secondJoystick;

    [SerializeField] private Rigidbody2D _rb, _rb2;
    [SerializeField] private EdgeCollider2D _lineCollider;

    private float _currentAngle = 0f, _currentAngle2 = 0f;
    private float timer = 0f;

    private const float RotateSpeed = 200f;
    private List<Vector2> _points = new List<Vector2>();
    private List<Vector2> _bufferPoint = new List<Vector2>();
    private Vector2 _bufferPoint1, _bufferPoint2;

    void MakeADot66()
    {
        timer += Time.deltaTime;
        if (timer >= 0.2f)
        {
            Vector2 bufferPoint = _bufferPoint1; // „тобы пропустить первыую точку, во избежание стоолкновений линии и персонажа

            Vector3 position = _rb.transform.localPosition;
            _bufferPoint1 = new Vector2(position.x, position.y);

            if (bufferPoint != null)
                _points.Add(_bufferPoint1); 
           
            if (_points.Count > 30)
                _points.RemoveAt(0);

            _lineCollider.SetPoints(_points);
            timer = 0f;
        }       
    }

    void MakeADot()
    {
        timer += Time.deltaTime;
        if (timer >= 0.1f)
        {
            Vector3 position = _rb.transform.localPosition;
            _bufferPoint.Add(new Vector2(position.x, position.y));

            if (_bufferPoint.Count >= 2) // „тобы пропустить первые точки, во избежание стоолкновений линии и персонажа
            {
                _points.Add(_bufferPoint[0]);
                _bufferPoint.RemoveAt(0);
            }
                
            if (_points.Count > 10)
                _points.RemoveAt(0);

            _lineCollider.SetPoints(_points);
            timer = 0f;
        }
    }

    private void Update()
    {
        Walls();
        ChangeDirection(_firstJoystick, _rb.transform, ref _currentAngle);
        ChangeDirection(_secondJoystick, _rb2.transform, ref _currentAngle2);
        MakeADot();
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.transform.position + _rb.gameObject.transform.up * Time.fixedDeltaTime);
        _rb2.MovePosition(_rb2.transform.position + _rb2.gameObject.transform.up * Time.fixedDeltaTime);
    }

    private void Walls()
    {
        Vector3 pos = _rb.transform.localPosition;
        if (pos.y > 1100f)
            pos.y = -1100f;
        else if (pos.y < -1100f)
            pos.y = 1100f;

        _rb.transform.localPosition = pos;
    }

    private void ChangeDirection(Joystick joy, Transform transform, ref float currentAngle)
    {
        Vector2 normalizedJoystick = new Vector2(joy.Horizontal, joy.Vertical).normalized;

        if (normalizedJoystick != Vector2.zero)
        {
            float targetAngle = Vector2.SignedAngle(Vector2.up, normalizedJoystick);

            if (currentAngle != targetAngle)
            {
                float offset = currentAngle - targetAngle;
                if (offset < -180)
                    currentAngle += 360f;
                else if (offset > 180)
                    targetAngle += 360f;

                float step = Time.deltaTime * RotateSpeed;
                if (currentAngle > targetAngle)
                {
                    currentAngle -= step;
                    transform.Rotate(0f, 0f, -step);
                }
                else
                {
                    currentAngle += step;
                    transform.Rotate(0f, 0f, step);
                }
            }
        }
    }
}