using UnityEngine;

public class PingPongLinesTrigger : MonoBehaviour
{
    public static event DataHolder.Notification MyTriggerEnter;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        MyTriggerEnter?.Invoke();       
    }
}
