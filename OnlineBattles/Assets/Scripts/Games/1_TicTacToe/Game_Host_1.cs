using UnityEngine;

public class Game_Host_1 : GameTemplate_WifiHost
{
    private GameResources_1 GR;
    private bool _myTurn = true;

    private void Start()
    {
        GR = transform.parent.GetComponent<GameResources_1>();
        BaseStart(null);
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
                    WifiServer_Host.SendTcpMessage(mes);
                    _myTurn = false;
                }
            }

            if (WifiServer_Host.Opponent.MessageTCPforGame.Count > 0)
            {
                string[] mes = WifiServer_Host.Opponent.MessageTCPforGame[0].Split(' ');
                if (mes[0] == "move")
                {
                    Vector3Int place = new Vector3Int(int.Parse(mes[1]), int.Parse(mes[2]), 0);
                    GR.Map.SetTile(place, GR.EnemyTile);
                    CheckEndOfGame();
                    _myTurn = true;
                }
                WifiServer_Host.Opponent.MessageTCPforGame.RemoveAt(0);
            }
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
}
