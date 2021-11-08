using UnityEngine;
using UnityEngine.Events;

public class PingPongLinesTrigger : MonoBehaviour
{
    public static event UnityAction MyTriggerEnter;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        MyTriggerEnter?.Invoke();       
    }
}