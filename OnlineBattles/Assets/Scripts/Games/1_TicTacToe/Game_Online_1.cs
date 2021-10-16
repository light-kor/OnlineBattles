using UnityEngine;

namespace Game1
{
    public class Game_Online_1 : GameTemplate_Online
    {
        private GameResources_1 GR;
        private bool _myTurn = false;

        private void Start()
        {
            GR = transform.parent.GetComponent<GameResources_1>();
            BaseStart(DataHolder.ConnectType.TCP);
        }

        protected override void Update()
        {
            base.Update();

            if (_gameOn)
            {
                if (_myTurn && Input.GetMouseButtonDown(0))
                {
                    Vector3 clickWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3Int clickCellPosition = GR.Map.WorldToCell(clickWorldPosition);
                    if (GR.Map.GetTile(clickCellPosition) == GR.MainTile)
                    {
                        GR.Map.SetTile(clickCellPosition, GR.MyTile);
                        string mes = $"move {clickCellPosition.x} {clickCellPosition.y}";
                        Network.ClientTCP.SendMessage(mes);
                        _myTurn = false;
                    }
                }

                if (Network.TCPMessagesForGames.Count > 0)
                {
                    string[] mes = Network.TCPMessagesForGames[0].Split(' ');
                    if (mes[0] == "move")
                    {
                        Vector3Int place = new Vector3Int(int.Parse(mes[1]), int.Parse(mes[2]), 0);
                        GR.Map.SetTile(place, GR.EnemyTile);
                        _myTurn = true;
                    }
                    Network.TCPMessagesForGames.RemoveAt(0);
                }
            }
        }
    }
}
