using GameEnumerations;
using UnityEngine;

namespace Game4
{   
    public class Multiplier : InteractiveObject
    {
        private Bullet _copy;
        private GameResources_4 GR;

        private const float Angel = 35f;
        private const float Offset = 0.2f;

        private void Start()
        {
            GR = GameResources_4.GameResources;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Bullet bullet))
            {
                Vector3 dir = bullet.Direction.normalized;
                Vector2 bulletPos = bullet.transform.position;
                float angle = Vector2.SignedAngle(Vector2.up, dir);
                float sign = 1f;
                if (Mathf.Abs(angle) > 90f)
                    sign = -1f;
                else sign = 1f;

                Vector2 newPos = new Vector2(bulletPos.x - Offset * sign, bulletPos.y);
                Vector2 copyPos = new Vector2(bulletPos.x + Offset * sign, bulletPos.y);
                // Делаем пулям разные позиции, чтоб они не задевали друг друга. И учитываем их напрвление для правильного размещения.

                _copy = GR.Bullets.SetBullet(copyPos, PlayerTypes.Null);
                if (_copy == null)
                    return;

                _copy.ChangeDirectionAngel(-Angel, dir);

                bullet.transform.position = newPos;
                bullet.ChangeDirectionAngel(Angel, dir);

                ReleaseObject();
            }
        }
    }
}
