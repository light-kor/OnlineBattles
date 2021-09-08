using UnityEngine;

public class a_Rotation : MonoBehaviour
{
    private float _rotationAngle = -30f;
    private float _rotateInterval = 0.15f;
    private float _time = 0f;

    private void Update()
    {
        _time += Time.deltaTime;
        if (_time > _rotateInterval)
        {
            transform.Rotate(0, 0, _rotationAngle);
            _time = 0f;
        }       
    }
}
