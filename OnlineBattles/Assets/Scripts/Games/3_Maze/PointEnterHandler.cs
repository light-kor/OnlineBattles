using GameEnumerations;
using UnityEngine;

namespace Game3
{
    public class PointEnterHandler : MonoBehaviour
    {
        private GameResources_3 GR;
        private int _number;

        private void Start()
        {
            GR = GameResources_3.GameResources;

            if (DataHolder.GameType != GameTypes.WifiClient)
                SetPointPositionLocal();
        }

        public void Init(int number)
        {
            _number = number;
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (DataHolder.GameType != GameTypes.WifiClient)
            {
                if (collider.TryGetComponent(out Player player))
                {            
                    player.CaughtThePoint();
                    SetPointPositionLocal();
                }
            }                          
        }

        private void SetPointPositionLocal()
        {
            Vector2 pointPos = GR.RandomPositionOnMap();
            transform.position = pointPos;

            if (DataHolder.GameType == GameTypes.WifiHost)
            {
                myVector2 pos = new myVector2(pointPos);
                string json = JsonUtility.ToJson(pos);

                WifiServer_Host.Opponent.SendTcpMessage($"point {_number} {json}");
            }           
        }
    }
}