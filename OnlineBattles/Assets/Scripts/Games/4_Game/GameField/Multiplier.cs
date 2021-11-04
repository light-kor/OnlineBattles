using UnityEngine;

namespace Game4
{   
    public class Multiplier : MonoBehaviour
    {
        private CircleCollider2D _collider;
        private Bullet _copy;

        private const float Angel = 35f;

        private void Start()
        {
            _collider = GetComponent<CircleCollider2D>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Bullet bullet))
            {
                Vector3 dir = bullet.Direction.normalized;

                _copy = Instantiate(bullet, transform.position, Quaternion.identity);               
                _copy.ChangeDirectionAngel(-Angel, dir);

                bullet.ChangeDirectionAngel(Angel, dir);

                _collider.enabled = false;
            }
        }
    }
}
