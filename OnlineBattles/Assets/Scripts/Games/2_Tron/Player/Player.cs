using GameEnumerations;
using UnityEngine;

namespace Game2
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerTypes _playerType;       
        [SerializeField] private TrailCollider _trailCollider;
        [SerializeField] private ParticleSystem _explosion;
        [SerializeField] private PlayerTrailRenderer _trailRenderer;

        public PlayerTypes PlayerType => _playerType;
        public bool GetPoint { get; private set; } = true;
        public PlayerMover PlayerMover { get; private set; }
        public PlayerInput PlayerInput { get; private set; }

        private GameResources_2 GR;        
        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private void Awake()
        {
            PlayerMover = GetComponent<PlayerMover>();
            PlayerInput = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            GR = GameResources_2.GameResources;           
            SetStartSettings();
        }

        private void Update()
        {
            if (GeneralController.GameOn)
            {
                if (DataHolder.GameType != GameTypes.WifiClient)
                    _trailCollider.CreateTrailsCollider(transform.position);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {         
            GetPoint = false;
            LoseAnimation();
            GR.RoundResults();
        }

        public void LoseAnimation()
        {
            _explosion.Play();
        }
      
        public void ResetLevel()
        {
            _trailRenderer.ClearTrail();
            _trailCollider.ClearCollider();
            _explosion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            PlayerInput.ClearJoystick();
            PlayerMover.ClearCurrentAngle();

            transform.position = _startPosition;
            transform.rotation = _startRotation;
            GetPoint = true;
        }

        private void SetStartSettings()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;

            if (DataHolder.GameType == GameTypes.WifiClient)
                GetComponent<PolygonCollider2D>().enabled = false;
        }
    }
}