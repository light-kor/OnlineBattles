using GameEnumerations;
using UnityEngine;

namespace Game3
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerTypes _playerType;
        [SerializeField] private Joystick _joystick;
        [SerializeField] private ManualCreateMapButton _button;
        private Rigidbody2D _rb;
        private Vector3 _lastJoystick;
        private GameResources_3 GR;
        private const float PlayersSpeed = 1.5f;

        private void Start()
        {
            GR = GameResources_3.GameResources;
            _rb = GetComponent<Rigidbody2D>();
            SetControlType();
        }

        private void Update()
        {
            if (GeneralController.GameOn)
            {
                if (DataHolder.GameType != GameTypes.WifiClient)
                {
                    ChangeDirectionLocal();
                }
            }
        }

        private void FixedUpdate()
        {
            if (DataHolder.GameType != GameTypes.WifiClient && GeneralController.GameOn)
                _rb.MovePosition(transform.position + _lastJoystick * Time.fixedDeltaTime * PlayersSpeed);
        }

        private void ChangeDirectionLocal()
        {
            _lastJoystick = new Vector2(_joystick.Horizontal, _joystick.Vertical).normalized;
        }

        public void ChangeDirectionRemote(Vector2 normalizedJoystick)
        {
            _lastJoystick = normalizedJoystick;
        }

        public void CaughtThePoint()
        {
            GR.UpdateScore(_playerType);
        }

        public void SetBroadcastPositions(Vector3 position)
        {
            _rb.MovePosition(position);
        }

        private void SetControlType()
        {
            if (DataHolder.GameType == GameTypes.WifiClient)
            {
                if (_playerType == PlayerTypes.BluePlayer)
                    DisableControl();
                else if (_playerType == PlayerTypes.RedPlayer)
                {
                    Vector2 joyPosition = new Vector2(-_joystick.transform.position.x, -_joystick.transform.position.y);
                    _joystick.transform.position = joyPosition;

                    Vector2 buttonPosition = new Vector2(-_button.transform.position.x, -_button.transform.position.y);
                    _button.transform.position = buttonPosition;

                    Vector3 eulerRotation = _button.transform.rotation.eulerAngles;
                    _button.transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
                }
            }
            else if (DataHolder.GameType == GameTypes.WifiHost)
            {
                if (_playerType == PlayerTypes.RedPlayer)
                    DisableControl();
            }
        }

        private void DisableControl()
        {
            _joystick.gameObject.SetActive(false);
            _button.gameObject.SetActive(false);
        }
    }
}
