using UnityEngine;

namespace Game2
{
    public class TrailCollider : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Player player))
            {
                player.LoseRound();
            }
        }
    }
}
