using System.Collections.Generic;
using UnityEngine;

namespace Game2
{
    [RequireComponent(typeof(EdgeCollider2D))]
    public class TrailCollider : MonoBehaviour
    {
        private EdgeCollider2D _collider;
        private float _timer = 0f;
        private List<Vector2> _points = new List<Vector2>();
        private List<Vector2> _bufferPoints = new List<Vector2>();

        private void Start()
        {
            _collider = GetComponent<EdgeCollider2D>();
        }

        public void CreateTrailsCollider(Vector3 localPos)
        {
            _timer += Time.deltaTime;
            if (_timer >= 0.1f)
            {
                _bufferPoints.Add(new Vector2(localPos.x, localPos.y));

                if (_bufferPoints.Count >= 3) // Чтобы пропустить первые точки, во избежание стоолкновений линии и персонажа
                {
                    _points.Add(_bufferPoints[0]);
                    _bufferPoints.RemoveAt(0);
                }

                if (_points.Count > 30)
                    _points.RemoveAt(0);

                _collider.SetPoints(_points);
                _timer = 0f;
            }
        }

        public void ClearCollider()
        {
            _collider.Reset();
            _collider.enabled = false;
            _collider.enabled = true;

            _points.Clear();
            _bufferPoints.Clear();
        }
    }
}
