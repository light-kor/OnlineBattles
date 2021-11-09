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

        private void Start()
        {
            _player = GetComponent<Player>();
            GR = GameResources_2.GameResources;
            SetControlType();
        }

        private void Update()
        {
            if (GR.GameOn)
            {
                if (DataHolder.GameType == GameTypes.Local || (DataHolder.GameType == GameTypes.WifiHost && _player.PlayerType == PlayerTypes.BluePlayer))
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

        private void SetControlType()
        {           
            if (DataHolder.GameType == GameTypes.WifiClient)
            {
                if (_player.PlayerType == PlayerTypes.BluePlayer)
                {
                    _joystick.gameObject.SetActive(false);
                }                  
                else if (_player.PlayerType == PlayerTypes.RedPlayer)
                {
                    Vector2 joyPosition = new Vector2(-_joystick.transform.position.x, -_joystick.transform.position.y);
                    _joystick.transform.position = joyPosition;
                }
            }
            else if (DataHolder.GameType == GameTypes.WifiHost)
            {
                if (_player.PlayerType == PlayerTypes.RedPlayer)
                {
                    _joystick.gameObject.SetActive(false);
                }
            }              
        }

        public void ClearJoystick()
        {
            _lastJoystick = Vector2.zero;
        }
    }
}