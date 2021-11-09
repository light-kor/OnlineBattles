using GameEnumerations;
using UnityEngine;

namespace Game1
{
    public class Game_Host_1 : GameTemplate_WifiHost
    {
        private GameResources_1 GR;
        private bool _myTurn = true;

        private void Start()
        {
            GR = transform.parent.GetComponent<GameResources_1>();
            GR.NewMessageReceived += ProcessingTCPMessages;
            BaseStart(ConnectTypes.TCP);
        }

        private void Update()
        {
            if (GR.GameOn)
            {
                if (_myTurn && Input.GetMouseButtonDown(0))
                {
                    Vector3 clickWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3Int clickCellPosition = GR.Map.WorldToCell(clickWorldPosition);
                    if (GR.Map.GetTile(clickCellPosition) == GR.MainTile)
                    {
                        GR.Map.SetTile(clickCellPosition, GR.MyTile);
                        string mes = $"move {clickCellPosition.x} {clickCellPosition.y}";
                        WifiServer_Host.Opponent.SendTcpMessage(mes);
                        _myTurn = false;
                    }
                }
            }
        }

        private void ProcessingTCPMessages(string[] mes)
        {
            if (mes[0] == "move")
            {
                Vector3Int place = new Vector3Int(int.Parse(mes[1]), int.Parse(mes[2]), 0);
                GR.Map.SetTile(place, GR.EnemyTile);
                CheckEndOfGame();
                _myTurn = true;
            }
        }

        public void CheckEndOfGame()
        {
            string result = GR.CheckWin();

            if (result != null)
            {
                CloseAll();

                if (result == "draw")
                    EndOfGame("drawn");
                else if (result == "first")
                    EndOfGame("lose");
                else if (result == "second")
                    EndOfGame("win");
            }
        }

        private void OnDestroy()
        {
            GR.NewMessageReceived -= ProcessingTCPMessages;
        }
    }
}
