using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game3
{
    public class PointsSpawner : MonoBehaviour
    {
        [SerializeField] private PointEnterHandler _pointPref;

        private List<PointEnterHandler> _pool = new List<PointEnterHandler>();

        private const int PointsMaxCount = 3;
        private const float SpawnDelay = 15f;

        private void Start()
        {
            CreatePoints();
            StartCoroutine(SpawnPointsWithDelay());
        }

        private IEnumerator SpawnPointsWithDelay()
        {
            float time = 0f;
            int number = 1;

            _pool[0].gameObject.SetActive(true);

            while (number < PointsMaxCount)
            {
                if (GeneralController.GameOn)
                {
                    time += Time.deltaTime;

                    if (time >= SpawnDelay)
                    {
                        _pool[number].gameObject.SetActive(true);
                        time = 0f;
                        number++;
                    }
                }

                yield return null;
            }
        }

        private void CreatePoints()
        {
            for (int i = 0; i < PointsMaxCount; i++)
            {
                PointEnterHandler spawned = Instantiate(_pointPref, transform);
                spawned.gameObject.SetActive(false);
                _pool.Add(spawned);
            }
        }
    }
}
