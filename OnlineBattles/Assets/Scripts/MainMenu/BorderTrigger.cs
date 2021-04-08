using UnityEngine;

public class BorderTrigger : MonoBehaviour
{
    public static event DataHolder.Notification MyTriggerEnter;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        MyTriggerEnter?.Invoke();       
    }
}
