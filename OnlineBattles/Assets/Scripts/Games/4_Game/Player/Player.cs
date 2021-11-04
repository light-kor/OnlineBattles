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
        }

        public void Init(GameResources_4 res)
        {
            GR = res;
        }
         
        //TODO: Сделать остановку перед выстрелом
        public void ButtonClick()
        {
            if (HasBullet)
            {
                Action fire = () => _releasedBullet = Instantiate(GR.BulletPrefab, transform.position, Quaternion.identity);
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
                    Destroy(bullet.gameObject);
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
            Debug.Log("Hit");
        }
    }
}
