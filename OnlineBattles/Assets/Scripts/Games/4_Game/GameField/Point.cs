using UnityEngine;

public class Point : MonoBehaviour
{
    public int Number = -1;

    public Vector2 TakePosition()
    {
        return transform.position;
    }
}
