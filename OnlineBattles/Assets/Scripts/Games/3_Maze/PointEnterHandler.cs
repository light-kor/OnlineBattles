using UnityEngine;

namespace Game3
{
    public class PointEnterHandler : MonoBehaviour
    {
        public delegate void CatchPoint(GameObject obj);
        public static event CatchPoint Catch;

        private void OnTriggerEnter2D(Collider2D player)
        {
            Catch?.Invoke(player.gameObject);
            Destroy(gameObject);
        }
    }
}