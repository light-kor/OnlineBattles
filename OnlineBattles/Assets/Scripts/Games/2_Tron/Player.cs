using GameEnumerations;
using UnityEngine;

namespace Game2
{
    public class Player : MonoBehaviour
    {
        [SerializeField] PlayerType _playerType;
        [SerializeField] private Joystick _joystick;
        [SerializeField] private TrailCollider _trailCollider;
        [SerializeField] private ParticleSystem _explosion;
        [SerializeField] private PlayerTrailRenderer _trailRenderer;

        private GameResources_2 GR;
        private Rigidbody2D _rb;
        private Vector3 _startPosition;
        private Quaternion _startRotation;       
        private PlayerControl _controlType = PlayerControl.Local;       
        private float _currentAngle = 0f;

        private const float MoveSpeed = 1.8f;
        private const float RotateSpeed = 300f;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            GR = GameResources_2.GameResources;
            SetStartSettings();
        }

        private void Update()
        {
            if (GR.GameOn)
            {
                if (_controlType != PlayerControl.Broadcast)
                {
                    _trailCollider.CreateTrailsCollider(transform.localPosition);
                    ChangeDirection();
                }             
            }
        }

        private void FixedUpdate()
        {
            if (GR.GameOn && _controlType != PlayerControl.Broadcast)
            {
                _rb.MovePosition(transform.position + gameObject.transform.up * Time.fixedDeltaTime * MoveSpeed);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_controlType != PlayerControl.Broadcast)
            {
                if (collision.gameObject.TryGetComponent(out Player player))
                {
                    _explosion.Play();
                    GR.RoundResults(null);
                }
            }               
        }

        public void LoseRound()
        {
            _explosion.Play();
            GR.RoundResults(gameObject);
        }
              
        private void ChangeDirection()
        {          
            Vector2 normalizedJoystick = Vector2.zero;

            if (_controlType == PlayerControl.Local)
            {
                if (_playerType == PlayerType.BluePlayer)
                    normalizedJoystick = new Vector2(_joystick.Horizontal, _joystick.Vertical).normalized;
                else if (_playerType == PlayerType.RedPlayer)
                    normalizedJoystick = new Vector2(-_joystick.Horizontal, -_joystick.Vertical).normalized;
            }
            else if (_controlType == PlayerControl.Remote)
            {
                if (GR.RemoteJoystick.Count > 0)
                {
                    normalizedJoystick = GR.RemoteJoystick[0];
                    GR.RemoteJoystick.RemoveAt(0);
                }
            }
                
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

        public void SetControlType(PlayerControl type)
        {
            _controlType = type;

            if (type == PlayerControl.Broadcast)
            {
                if (_playerType == PlayerType.RedPlayer)
                    _joystick.gameObject.SetActive(false);
            }
        }

        public void SetBroadcastPositions(Vector3 position, Quaternion rotation)
        {
            _rb.MovePosition(position);
            _rb.MoveRotation(rotation);
        }

        public void ResetLevel()
        {
            _trailRenderer.ClearTrail();
            _trailCollider.ClearCollider();
            _explosion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            transform.localPosition = _startPosition;
            transform.localRotation = _startRotation;
        }

        private void SetStartSettings()
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localRotation;
        }        
    }
}
