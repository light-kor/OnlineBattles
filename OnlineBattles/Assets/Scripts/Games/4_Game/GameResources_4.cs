using GameEnumerations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game4
{
    public class GameResources_4 : GeneralController
    {
        [SerializeField] private Player _blue, _red;
        [SerializeField] private Bullet _bulletPrefab;

        public Bullet BulletPrefab => _bulletPrefab;

        protected override void Awake()
        {
            base.Awake();
            _blue.Init(this);
            _red.Init(this);
        }

        public void TryReloadGuns()
        {
            if (_blue.HasBullet == false && _red.HasBullet == false)
            {
                //TODO: Сделать задержку
                _blue.ReloadGun();
                _red.ReloadGun();
            }
        }
    }
}
