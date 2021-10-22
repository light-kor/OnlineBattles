using GameEnumerations;
using UnityEngine;

namespace Game2
{
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] private Joystick _joystick;
        private GameResources_2 GR;
        private Rigidbody2D _rb;       
        private Player _player;
        private float _currentAngle = 0f;

        private const float MoveSpeed = 1.8f;
        private const float RotateSpeed = 300f;

        private void Awake()
        {           
            _player = GetComponent<Player>();
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            GR = GameResources_2.GameResources;
        }

        private void Update()
        {
            if (GR.GameOn)
                if (_player.ControlType == ControlTypes.Local)
                    ChangeDirectionLocal();
        }

        private void FixedUpdate()
        {
            if (GR.GameOn && _player.ControlType != ControlTypes.Broadcast)
                _rb.MovePosition(transform.position + gameObject.transform.up * Time.fixedDeltaTime * MoveSpeed);
        }

        private void ChangeDirectionLocal()
        {
            Vector2 normalizedJoystick = Vector2.zero;

            if (_player.PlayerType == PlayerTypes.BluePlayer)
                normalizedJoystick = new Vector2(_joystick.Horizontal, _joystick.Vertical).normalized;
            else if (_player.PlayerType == PlayerTypes.RedPlayer)
                normalizedJoystick = new Vector2(-_joystick.Horizontal, -_joystick.Vertical).normalized;

            ChangeDirection(normalizedJoystick);
        }

        public void ChangeDirection(Vector2 normalizedJoystick) //TODO: ���������� �� ������� ������������ ���������, ���� ���� ����� ���� ��������� ���������
        {
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

        public void SetControlType()
        {
            if (_player.ControlType == ControlTypes.Broadcast)
            {
                if (_player.PlayerType == PlayerTypes.BluePlayer)
                    _joystick.gameObject.SetActive(false);

                if (_player.PlayerType == PlayerTypes.RedPlayer)
                {
                    Vector2 joyPosition = new Vector2(-_joystick.transform.position.x, -_joystick.transform.position.y);
                    _joystick.transform.position = joyPosition;
                }
            }
            else if (_player.ControlType == ControlTypes.Remote)
                _joystick.gameObject.SetActive(false);          
        }

        public void SetBroadcastPositions(Vector3 position, Quaternion rotation)
        {
            _rb.MovePosition(position);
            transform.localRotation = rotation;
        }        
    }
}
