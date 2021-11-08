using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game4
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private Multiplier _multiplierPrefab;
        [SerializeField] private Deceleration _speedChangerPrefab;
        [SerializeField] private Block _blockPrefab;
        [SerializeField] private GameObject _objectsContainer;
        [SerializeField] private GameObject _pointsContainer;

        private const int MaxShownObjectsCount = 7;
        private const int MaxMultiplierCount = 1;
        private const int MaxSpeedChangerCount = 1;

        private List<Point> _freePoints = new List<Point>();
        private List<Point> _busyPoints = new List<Point>();
        private List<InteractiveObject> _pool = new List<InteractiveObject>();

        private int _objectsCount = 0;

        private void Start()
        {
            _pointsContainer.GetComponentsInChildren(_freePoints);
            InitializationPool();
            
            for (int i = 0; i < MaxShownObjectsCount; i++)
            {
                SetObject();
            }
        }        

        private void InitializationPool()
        {
            AddNewObjects(_multiplierPrefab.gameObject, MaxMultiplierCount);
            AddNewObjects(_speedChangerPrefab.gameObject, MaxSpeedChangerCount);
            AddNewObjects(_blockPrefab.gameObject, _freePoints.Count - _objectsCount);
        }

        private void AddNewObjects(GameObject prefab, int maxCount)
        {
            for (int i = 0; i < maxCount; i++)
            {
                GameObject spawned = Instantiate(prefab, _objectsContainer.transform);
                spawned.SetActive(false);
                _pool.Add(spawned.GetComponent<InteractiveObject>());
                _objectsCount++;
            }
        }

        private bool TryGetObject(out InteractiveObject result)
        {
            result = _pool.First(p => p.gameObject.activeSelf == false);
            return result != null;
        }

        private void SetObject()
        {
            int num;
            if (_freePoints.Count > 0)
            {
                num = Random.Range(0, _freePoints.Count - 1);
            }
            else return;

            if (TryGetObject(out InteractiveObject obj))
            {
                obj.SetObject(_freePoints[num], ReleasePoint);
                _busyPoints.Add(_freePoints[num]);
                _freePoints.RemoveAt(num);
            }
            else return;
        }

        private void ReleasePoint(Point releasePoint) // Метод передаётся как аргумент
        {
            _busyPoints.Remove(releasePoint);
            _freePoints.Add(releasePoint);

            SetObject();
        }
    }
}
