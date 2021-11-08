using System.Collections.Generic;
using UnityEngine;

namespace Game2
{
    public class Walls : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        private EdgeCollider2D _collider;
        private List<Vector2> _points = new List<Vector2>();
        private const float Offset = 20f;

        private void Start()
        {
            _collider = GetComponent<EdgeCollider2D>();
            SetWallsCollider();
        }

        private void SetWallsCollider()
        {
            float halfWidth = Screen.width / _canvas.scaleFactor / 2 + Offset;
            float halfHeight = Screen.height / _canvas.scaleFactor / 2 + Offset;

            _points.Add(new Vector2(halfWidth, halfHeight));
            _points.Add(new Vector2(halfWidth, -halfHeight));
            _points.Add(new Vector2(-halfWidth, -halfHeight));
            _points.Add(new Vector2(-halfWidth, halfHeight));
            _points.Add(new Vector2(halfWidth, halfHeight));

            _collider.SetPoints(_points);
        }
    }
}
