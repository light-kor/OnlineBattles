using UnityEngine;

public class LoadingAnim : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = -1f;
    private void Update()
    {
        transform.Rotate(0, 0, _rotationSpeed);
    }
}
