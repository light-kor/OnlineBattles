using GameEnumerations;
using UnityEngine;

namespace Game2
{
    public class PlayerMover : MonoBehaviour
    {      
        private GameResources_2 GR;
        private Rigidbody2D _rb;       
        private PlayerInput _playerInput;
        private float _currentAngle = 0f;       

        private const float MoveSpeed = 1.8f;
        private const float RotateSpeed = 300f;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _playerInput = GetComponent<PlayerInput>();
            GR = GameResources_2.GameResources;
        }

        private void Update()
        {
            if (GR.GameOn)
                RotatePlayer();
        }

        private void FixedUpdate()
        {
            if (GR.GameOn && DataHolder.GameType != GameTypes.WifiClient)
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
      
        public void SetBroadcastPositions(Vector3 position, Quaternion rotation)
        {
            _rb.MovePosition(position);
            _rb.MoveRotation(rotation);
            //transform.rotation = rotation;
        }        
    }
}
