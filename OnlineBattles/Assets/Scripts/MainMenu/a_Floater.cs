using UnityEngine;

public class a_Floater : MonoBehaviour
{
    [SerializeField] private float _amplitude = 0.2f;
    [SerializeField] private float _frequency = 0.5f;

    private Vector3 _posOffset = new Vector3();
    private Vector3 _tempPos = new Vector3();

    private void Start()
    {
        _posOffset = transform.position;
    }

    private void Update()
    {
        _tempPos = _posOffset;
        _tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * _frequency) * _amplitude;
        transform.position = _tempPos;
    }
}