using UnityEngine;

public class TrailCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Player player))
        {
            player.PlayExplosionAnim();
        }
    }
}
