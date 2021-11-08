using UnityEngine;

namespace Game4
{
    public class Block : InteractiveObject
    {     
        private const float LowerAngel = 180f;
        private const float MiddleAngel = 15f;
        private const float TopAngel = 25f;       

        public void CollisionHandler(HitboxType hitboxType, Bullet bullet)
        {
            float angel = 0;

            if (hitboxType == HitboxType.Lower)
                angel = LowerAngel;
            else if (hitboxType == HitboxType.Middle)
                angel = GetRandomSign() * MiddleAngel;
            else if (hitboxType == HitboxType.Top)
                angel = GetRandomSign() * TopAngel;


            Vector3 dir = bullet.Direction.normalized;
            bullet.ChangeDirectionAngel(angel, dir);

            if (bullet.TryReleaseFirstBlock())
                    ReleaseObject();
        }

        private int GetRandomSign()
        {
            float random = Random.Range(0, 1);
            if (random >= 0.5f)
                return 1;
            else return -1;
        }

        public enum HitboxType
        {
            Lower,
            Middle,
            Top
        }
    }
}
