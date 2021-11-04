using UnityEngine;

namespace Game4
{
    public class Reflector : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent(out Bullet bullet))
            {
                ContactPoint2D[] contacts = new ContactPoint2D[2];
                collision.GetContacts(contacts);
                Vector3 inNormal = contacts[0].normal;

                bullet.ReflectDirection(inNormal);
            }
        }
    }
}
