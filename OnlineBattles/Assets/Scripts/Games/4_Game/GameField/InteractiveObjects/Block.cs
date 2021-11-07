using UnityEngine;

namespace Game4
{
    public class Block : InteractiveObject
    {
        private const float Angel = 20f;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Bullet bullet))
            {
                Vector3 dir = bullet.Direction.normalized;
                bullet.ChangeDirectionAngel(Angel, dir);
                bullet.SpeedUp();

                if (bullet.TryReleaseBlock())
                    ReleaseObject();
            }
        }
    }
}
