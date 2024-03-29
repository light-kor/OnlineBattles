using GameEnumerations;
using UnityEngine;

namespace Game2
{
    public class PlayerMover : MonoBehaviour
    {      
        private Rigidbody2D _rb;       
        private PlayerInput _playerInput;
        private float _currentAngle = 0f;       

        private const float MoveSpeed = 1.8f;
        private const float RotateSpeed = 300f;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _playerInput = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            if (GeneralController.GameOn)
                RotatePlayer();
        }

        private void FixedUpdate()
        {
            if (DataHolder.GameType != GameTypes.WifiClient && GeneralController.GameOn)
                _rb.MovePosition(transform.position + gameObject.transform.up * Time.fixedDeltaTime * MoveSpeed);
        }

        private void RotatePlayer() 
        {
            if (_playerInput.LastJoystick != Vector2.zero)
            {
                float targetAngle = Vector2.SignedAngle(Vector2.up, _playerInput.LastJoystick);

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

        public void SetBroadcastTransforms(Vector3 position, Quaternion rotation)
        {
            _rb.MovePosition(position);
            transform.rotation = rotation;
        }

        public void ClearCurrentAngle()
        {
            _currentAngle = 0f;
        }
    }
}
