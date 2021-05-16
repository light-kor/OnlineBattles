using UnityEngine;

public class Game_Online_1 : GameTemplate_Online
{
    private GameResources_1 GR;

    private void Start()
    {
        GR = transform.parent.GetComponent<GameResources_1>();
        BaseStart(null);
    }

    protected override void Update()
    {
        if (_gameOn)
        {
            if (GR.MyTurn && Input.GetMouseButtonDown(0))
            {
                Vector3 clickWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickCellPosition = GR.Map.WorldToCell(clickWorldPosition);
                if (GR.Map.GetTile(clickCellPosition) == GR.MainTile)
                {
                    GR.Map.SetTile(clickCellPosition, GR.MyTile);
                    string mes = $"move {clickCellPosition.x} {clickCellPosition.y}";
                    DataHolder.ClientTCP.SendMessage(mes);
                    GR.MyTurn = false;
                }
            }

            if (DataHolder.MessageTCPforGame.Count > 0)
            {
                string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');
                if (mes[0] == "move")
                {
                    Vector3Int place = new Vector3Int(int.Parse(mes[1]), int.Parse(mes[2]), 0);
                    GR.Map.SetTile(place, GR.EnemyTile);
                    GR.MyTurn = true;
                }
                DataHolder.MessageTCPforGame.RemoveAt(0);
            }
        }
    }
}