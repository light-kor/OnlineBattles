using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameResources_1 : MonoBehaviour
{
    public Tile MainTile, MyTile, EnemyTile;
    public Tilemap Map, PlayLayer;
    [HideInInspector] public bool MyTurn = true;

    private readonly int FieldSize = 4;
    private readonly int WinRow = 3;

    private void Start()
    {
        DataHolder.CanMove = true;
    }

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0) && (DataHolder.CanMove == true))
        //{
        //    Vector3 clickWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    Vector3Int clickCellPosition = _map.WorldToCell(clickWorldPosition);
        //    if (_map.GetTile(clickCellPosition) == _mainTile)
        //    {
        //        _map.SetTile(clickCellPosition, _myTile);

        //        if (firstMove)
        //        {
        //            _x1 = clickCellPosition.x;
        //            _y1 = clickCellPosition.y;
        //            firstMove = false;
        //        }
        //        else
        //        {
        //            string mes = $"1 {_x1} {_y1} {clickCellPosition.x} {clickCellPosition.y}";
        //            DataHolder.ClientTCP.SendMessage(mes);
        //            firstMove = true;
        //            DataHolder.CanMove = false;
        //        }
        //    }
        //}
    }

    // (-3 2) (2 2)
    // (-3 -3) (2 -3)

    /// <summary>
    /// Победа в крестиках-ноликах. Возвращает: 0 - продолжить игру, 1 - победа, -1 - проигрыш, 2 - ничья 
    /// </summary>
    /// <param name="count">Размер выигрышной последовательности</param>
    /// <param name="countFull">Размер игрового поля countFullХcountFull</param>
    public string CheckWin()
    {
        string winer = null;
        int startX = -FieldSize / 2;
        int startY = startX;
        bool firstGetRow = false, secondGetRow = false;

        // Проверка победы обоих игроков, с учётом сдвигов по координатам
        for (int i = startX; i < startX + FieldSize - WinRow + 1; i++)
        {
            for (int j = startY; j < startY + FieldSize - WinRow + 1; j++)
            {
                if (CheckWinWithOffset(MyTile, WinRow, i, j))
                    firstGetRow = true;

                if (CheckWinWithOffset(EnemyTile, WinRow, i, j))
                    secondGetRow = true;
            }
        }

        if (firstGetRow == true && secondGetRow == false)
            winer = "first";
        if (firstGetRow == false && secondGetRow == true)
            winer = "second";
        if (firstGetRow == true && secondGetRow == true)
            winer = "draw";

        // Проверка поля на полную заполненность
        if (winer == null)
        {
            List<Vector3Int> emptyCoor = new List<Vector3Int>();
            int emptyPlace = 0;
            for (int i = startX; i < startX + FieldSize + 1; i++)
            {
                for (int j = startY; j < startY + FieldSize + 1; j++)
                {
                    Vector3Int check = new Vector3Int(i, j, 0);

                    if (Map.GetTile(check) == MainTile)
                    {
                        emptyPlace++;
                        emptyCoor.Add(check);
                        if (emptyPlace == 0)
                            winer = "draw";
                    }
                }
            }           
        }
        return winer;
    }

    /// <summary>
    /// Проверка отдельных квадратов. Проходит проверка заданного квадрата countХcount, 
    /// если хоть один элемент последовательности не годится, проверка переходит на следующую последовательность.
    /// </summary>
    /// <param name="symb">Проверяемый Tile</param>
    /// <param name="count">Размер выигрышной последовательности</param>
    /// <param name="offsetX">Смещение по X координате</param>
    /// <param name="offsetY">Смещение по Y координате</param>
    /// <returns>Возвращает true, если есть хоть один ряд из count штук</returns>
    private bool CheckWinWithOffset(Tile symb, int count, int offsetX, int offsetY)
    {
        bool result = true;
        for (int col = offsetX; col < offsetX + count; col++)
        {
            // Проверка по вертикали
            result = true;
            for (int row = offsetY; row < offsetY + count; row++)
            {
                Vector3Int check = new Vector3Int(col, row, 0);

                if (Map.GetTile(check) != symb)
                {
                    result = false;
                    break;
                }
            }
            if (result) return result;

            // Проверка по горизонтали
            result = true;
            for (int row = offsetY; row < offsetY + count; row++)
            {
                Vector3Int check = new Vector3Int(row, col, 0);

                if (Map.GetTile(check) != symb)
                {
                    result = false;
                    break;
                }
            }
            if (result) return result;
        }

        // Проверка диагонали с левого низа
        result = true;
        for (int i = 0; i < count; i++)
        {
            Vector3Int check = new Vector3Int(offsetX + i, offsetY + i, 0);
            if (Map.GetTile(check) != symb)
            {
                result = false;
                break;
            }
        }
        if (result) return result;

        // Проверка диагонали с правого низа
        result = true;
        for (int i = 0; i < count; i++)
        {
            Vector3Int check = new Vector3Int(offsetX + count - i - 1, offsetY + i, 0);
            if (Map.GetTile(check) != symb)
            {
                result = false;
                break;
            }
        }
        if (result) return result;

        return false;
    }
}
