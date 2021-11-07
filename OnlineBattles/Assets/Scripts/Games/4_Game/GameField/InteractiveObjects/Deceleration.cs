using UnityEngine;

namespace Game4
{
    public class Deceleration : InteractiveObject
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Bullet bullet))
            {
                bullet.SpeedDown();
                ReleaseObject();
            }
        }
    }
}
