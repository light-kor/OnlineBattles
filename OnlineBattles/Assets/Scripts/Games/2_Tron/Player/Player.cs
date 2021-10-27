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

        public ControlTypes ControlType => _controlType;
        public PlayerTypes PlayerType => _playerType;
        public bool GetPoint { get; private set; } = false;
        public PlayerMover PlayerMover { get; private set; }

        private ControlTypes _controlType = ControlTypes.Local;
        private GameResources_2 GR;        
        private Vector3 _startPosition;
        private Quaternion _startRotation;                         

        private void Start()
        {
            GR = GameResources_2.GameResources;           
            SetStartSettings();
        }

        private void Update()
        {
            if (GR.GameOn)
            {
                if (_controlType != ControlTypes.Broadcast)
                    _trailCollider.CreateTrailsCollider(transform.localPosition);           
            }
        }
      
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent(out Player player))
            {
                LoseAnimation();
                GR.RoundResults();
                // Без GetPoint тк врезались друг в друга, и очков давать не нужно.
            }
        }

        public void LoseRound()
        {
            LoseAnimation();
            GetPoint = true;
            GR.RoundResults();
        }

        public void LoseAnimation()
        {
            _explosion.Play();
        }

        public void SetControlType(ControlTypes type)
        {
            _controlType = type;
            PlayerMover = GetComponent<PlayerMover>();

            if (type == ControlTypes.Broadcast)               
                GetComponent<PolygonCollider2D>().enabled = false;

            PlayerMover.SetControlType();
        }
      
        public void ResetLevel()
        {
            _trailRenderer.ClearTrail();
            _trailCollider.ClearCollider();
            _explosion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            transform.localPosition = _startPosition;
            transform.localRotation = _startRotation;
            GetPoint = false;
        }

        private void SetStartSettings()
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localRotation;
        }
    }
}
