using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game4
{
    public class BulletsInit : MonoBehaviour
    {
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private GameObject _bulletsContainer;

        private const int BulletCount = 5; 
        private List<GameObject> _pool = new List<GameObject>();

        private void Start()
        {
            InstantiateNewBullets(_bulletPrefab.gameObject, BulletCount);
        }

        private void InstantiateNewBullets(GameObject prefab, int maxCount)
        {
            for (int i = 0; i < maxCount; i++)
            {
                GameObject spawned = Instantiate(prefab, _bulletsContainer.transform);
                spawned.SetActive(false);
                _pool.Add(spawned);
            }
        }

        private bool TryGetObject(out GameObject result)
        {
            result = _pool.First(p => p.gameObject.activeSelf == false);
            return result != null;
        }

        //private void SetObject()
        //{
        //    int num;
        //    if (_freePoints.Count > 0)
        //    {
        //        num = Random.Range(0, _freePoints.Count - 1);
        //    }
        //    else return;

        //    if (TryGetObject(out InteractiveObject obj))
        //    {
        //        obj.SetObject(_freePoints[num], ReleasePoint);
        //        _busyPoints.Add(_freePoints[num]);
        //        _freePoints.RemoveAt(num);
        //    }
        //    else return;
        //}
    }
}
