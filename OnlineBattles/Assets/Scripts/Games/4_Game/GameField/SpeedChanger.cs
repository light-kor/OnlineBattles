using UnityEngine;

namespace Game4
{
    public class SpeedChanger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Bullet bullet))
            {
                int option = Random.Range(1, 10);

                if (option % 2 == 0)
                    bullet.AddSpeed(true);
                else
                    bullet.AddSpeed(false);
            }
        }
    }
}
