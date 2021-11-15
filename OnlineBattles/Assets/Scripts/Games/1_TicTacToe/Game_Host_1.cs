using GameEnumerations;
using UnityEngine;

namespace Game1
{
    public class Game_Host_1 : GameTemplate_WifiHost
    {
        private GameResources_1 GR;
        private bool _blueTurn = true;

        private void Start()
        {
            GR = GameResources_1.GameResources;
            GR.NewMessageReceived += ProcessingTCPMessages;
        }

        private void Update()
        {
            if (GeneralController.GameOn)
            {
                if (_blueTurn && Input.GetMouseButtonDown(0))
                {
                    Vector3 clickWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    if (GR.TrySetTile(clickWorldPosition, PlayerTypes.BluePlayer, out Vector3Int clickCellPosition))
                    {
                        string mes = $"move {clickCellPosition.x} {clickCellPosition.y}";
                        WifiServer_Host.Opponent.SendTcpMessage(mes);
                        _blueTurn = false;
                    }                   
                }
            }
        }

        private void ProcessingTCPMessages(string[] mes)
        {
            if (mes[0] == "move")
            {
                Vector3Int place = new Vector3Int(int.Parse(mes[1]), int.Parse(mes[2]), 0);
                GR.SetTile(place, PlayerTypes.RedPlayer);               
                _blueTurn = true;
            }
        }

        private void OnDestroy()
        {
            GR.NewMessageReceived -= ProcessingTCPMessages;
        }
    }
}
