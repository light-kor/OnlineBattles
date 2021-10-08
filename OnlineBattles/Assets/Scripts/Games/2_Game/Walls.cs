using System.Collections.Generic;
using UnityEngine;

public class Walls : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    private EdgeCollider2D _collider;
    private List<Vector2> _points = new List<Vector2>();
    private float _offset = 20f;

    private void Start()
    {
        _collider = GetComponent<EdgeCollider2D>();
        SetWallsCollider();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Player player))
        {
            player.LoseRound();
        }
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
