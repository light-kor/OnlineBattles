using UnityEngine;

namespace Game1
{
    public class Game_onPhone_1 : MonoBehaviour
    {
        private GameResources_1 GR;
        private bool _firstPlayerTurn = true;

        void Start()
        {
            GR = transform.parent.GetComponent<GameResources_1>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 clickWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickCellPosition = GR.Map.WorldToCell(clickWorldPosition);
                if (GR.Map.GetTile(clickCellPosition) == GR.MainTile)
                {
                    if (_firstPlayerTurn)
                        GR.Map.SetTile(clickCellPosition, GR.MyTile);
                    else
                        GR.Map.SetTile(clickCellPosition, GR.EnemyTile);

                    if (!_firstPlayerTurn)
                        CheckEndOfGame();

                    _firstPlayerTurn = !_firstPlayerTurn;
                }
            }
        }

        public void CheckEndOfGame()
        {
            string result = GR.CheckWin();

            if (result != null)
            {
                string notifText = null;
                if (result == "draw")
                    notifText = "Ничья";
                else if (result == "first")
                    notifText = "Синий победил";
                else if (result == "second")
                    notifText = "Красный победил";

                new Notification(notifText, Notification.ButtonTypes.MenuButton);
            }
        }
    }
}
