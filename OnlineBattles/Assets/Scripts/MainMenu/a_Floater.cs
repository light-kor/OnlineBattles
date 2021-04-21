using UnityEngine;

public class a_Floater : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.2f;
    [SerializeField] private float frequency = 0.5f;

    private Vector3 posOffset = new Vector3();
    private Vector3 tempPos = new Vector3();

    private void Start()
    {
        posOffset = transform.position;
    }

    private void Update()
    {
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        transform.position = tempPos;
    }
}