using GameEnumerations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game4
{
    public class BulletContainer : MonoBehaviour
    {
        [SerializeField] private Bullet _bulletPrefab;

        private const int BulletCount = 5; 
        private List<Bullet> _pool = new List<Bullet>();

        private void Start()
        {
            InstantiateNewBullets(_bulletPrefab, BulletCount);
        }

        private void InstantiateNewBullets(Bullet prefab, int maxCount)
        {
            for (int i = 0; i < maxCount; i++)
            {
                Bullet spawned = Instantiate(prefab, transform);
                spawned.gameObject.SetActive(false);
                _pool.Add(spawned);
            }
        }

        private bool TryGetBullet(out Bullet result)
        {
            result = _pool.First(p => p.gameObject.activeSelf == false);
            return result != null;
        }

        public Bullet SetBullet(Vector3 position, PlayerTypes playerType)
        {
            if (TryGetBullet(out Bullet bullet))
            {
                bullet.gameObject.SetActive(true);
                bullet.gameObject.transform.position = position;
                bullet.SetStartBullet(playerType);
                return bullet;
            }
            else
            {
                Debug.Log("Bullet limit");
                return null;
            }                 
        }
    }
}
