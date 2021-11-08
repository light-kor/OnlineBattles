using System.Collections.Generic;
using UnityEngine;

namespace Game4
{
    public class BulletCatcher : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        private EdgeCollider2D _collider;
        private List<Vector2> _points = new List<Vector2>();

        private const float _offset = 100f;

        private void Start()
        {
            _collider = GetComponent<EdgeCollider2D>();
            SetWallsCollider();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            collision.gameObject.SetActive(false);
        }

        private void SetWallsCollider()
        {
            float halfWidth = Screen.width / _canvas.scaleFactor / 2 + _offset;
            float halfHeight = Screen.height / _canvas.scaleFactor / 2 + _offset;

            _points.Add(new Vector2(halfWidth, halfHeight));
            _points.Add(new Vector2(halfWidth, -halfHeight));
            _points.Add(new Vector2(-halfWidth, -halfHeight));
            _points.Add(new Vector2(-halfWidth, halfHeight));
            _points.Add(new Vector2(halfWidth, halfHeight));

            _collider.SetPoints(_points);
        }
    }
}
