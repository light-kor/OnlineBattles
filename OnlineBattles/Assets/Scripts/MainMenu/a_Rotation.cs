using UnityEngine;

public class a_Rotation : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = -1f;
    private void Update()
    {
        transform.Rotate(0, 0, _rotationSpeed);
    }
}
