using GameEnumerations;
using UnityEngine;

namespace Game2
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Joystick _joystick;
        public Vector2 LastJoystick => _lastJoystick;
        private GameResources_2 GR;
        private Vector2 _lastJoystick = Vector2.zero;
        private Player _player;

        private void Awake()
        {
            _player = GetComponent<Player>();
        }

        private void Start()
        {
            GR = GameResources_2.GameResources;                 
        }

        private void Update()
        {
            if (GR.GameOn)
            {
                if (_player.ControlType == ControlTypes.Local)
                {
                    ChangeDirectionLocal();
                }
            }               
        }

        private void ChangeDirectionLocal()
        {
            Vector2 normalizedJoystick = Vector2.zero;

            if (_player.PlayerType == PlayerTypes.BluePlayer)
                normalizedJoystick = new Vector2(_joystick.Horizontal, _joystick.Vertical).normalized;
            else if (_player.PlayerType == PlayerTypes.RedPlayer)
                normalizedJoystick = new Vector2(-_joystick.Horizontal, -_joystick.Vertical).normalized;

            if (normalizedJoystick != _lastJoystick)
            {
                _lastJoystick = normalizedJoystick;
            }
        }

        public void ChangeDirectionRemote(Vector2 normalizedJoystick)
        {
            if (normalizedJoystick != _lastJoystick)
            {
                _lastJoystick = normalizedJoystick;
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

        public void ClearJoystick()
        {
            _lastJoystick = Vector2.zero;
        }
    }
}
