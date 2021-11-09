using GameEnumerations;
using System;
using UnityEngine;

namespace Game4
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerTypes _playerType;

        public PlayerTypes PlayerType => _playerType;
        public bool HasBullet { get; private set; } = true;

        private Bullet _releasedBullet;
        private GameResources_4 GR;
        private PlayerMover _playerMover;
        
        private void Start()
        {
            _playerMover = GetComponent<PlayerMover>();
            GR = GameResources_4.GameResources;
        }

        public void ButtonClick()
        {
            if (HasBullet)
            {
                Action fire = () => _releasedBullet = GR.Bullets.SetBullet(transform.position, _playerType);

                _playerMover.StopBeforeFiring(fire);

                HasBullet = false;
                GR.TryReloadGuns();
            }
            else
            {
                _playerMover.ChangeTarget();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Bullet bullet))
            {
                if (bullet == _releasedBullet)
                {
                    _releasedBullet = null;
                }             
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Bullet bullet))
            {
                if (bullet != _releasedBullet)
                {
                    bullet.gameObject.SetActive(false);
                    Hit();
                }              
            }
        }

        public void ReloadGun()
        {
            HasBullet = true;
        }

        public void Hit()
        {
            GR.GetHit(_playerType);
        }
    }
}
