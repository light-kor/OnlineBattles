using UnityEngine;

namespace Game4
{
    public class BlockHitbox : MonoBehaviour
    {
        [SerializeField] Block _block;
        [SerializeField] Block.HitboxType _hitboxType;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Bullet bullet))
            {
                _block.CollisionHandler(_hitboxType, bullet);
            }
        }
    }
}
