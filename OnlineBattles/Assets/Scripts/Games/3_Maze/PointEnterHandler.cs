using GameEnumerations;
using UnityEngine;

namespace Game3
{
    public class PointEnterHandler : MonoBehaviour
    {
        private GameResources_3 GR;

        private void Start()
        {
            GR = GameResources_3.GameResources;
            SetPointPosition();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (DataHolder.GameType != GameTypes.WifiClient)
            {
                if (collider.TryGetComponent(out Player player))
                {            
                    player.CaughtThePoint();
                    SetPointPosition();
                }
            }                          
        }

        private void SetPointPosition()
        {
            Vector2 pointPos = GR.RandomPositionOnMap();
            transform.position = pointPos;

            if (DataHolder.GameType == GameTypes.WifiHost)
                WifiServer_Host.Opponent.SendTcpMessage($"point {pointPos.x} {pointPos.y}");
        }
    }
}