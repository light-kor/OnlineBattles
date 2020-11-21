using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TicTacToe : MonoBehaviour
{
    public Tile my, main, enemy, both;
    private Tilemap map;
    private Camera mainCam;
    private int x1, y1;
    private bool firstMove = true;

    private void Start()
    {
        Application.runInBackground = true;

        map = GameObject.FindGameObjectWithTag("TileMap").GetComponent<Tilemap>();
        mainCam = Camera.main;

        // Начинаем
        DataHolder.CanMove = true;
        DataHolder.TimerT = GameObject.FindGameObjectWithTag("Timer");
        DataHolder.TimerT.GetComponent<timer>().StartTimer();
        
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && (DataHolder.CanMove == true) && DataHolder.TimerT.GetComponent<timer>().TimeLeftCheck())
        {
            Vector3 clickWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickCellPosition = map.WorldToCell(clickWorldPosition);
            if (map.GetTile(clickCellPosition) == main)
            {
                map.SetTile(clickCellPosition, my);

                if (firstMove)
                {
                    x1 = clickCellPosition.x;
                    y1 = clickCellPosition.y;
                    firstMove = false;
                }
                else
                {
                    string mes = $"1 {x1} {y1} {clickCellPosition.x} {clickCellPosition.y}";
                    DataHolder.ClientTCP.SendMassage(mes);
                    firstMove = true;
                    DataHolder.CanMove = false;
                    DataHolder.TimerT.GetComponent<timer>().StopTimer();
                }
            }
        }

        while (DataHolder.MessageTCPforGame.Count > 0)
        {
            // Обработка полученных от сервера сообщений
            string[] mes = DataHolder.MessageTCPforGame[0].Split(' ');
            switch (mes[0])
            {
                case "1":                    
                    for (int i = 1; i < mes.Length; i += 2) // Начинаем с первого тк первый элемент массива - это номер игры
                    {
                        Vector3Int place = new Vector3Int(int.Parse(mes[i]), int.Parse(mes[i + 1]), 0);
                        if (map.GetTile(place) == main)
                            map.SetTile(place, enemy);
                        else if (map.GetTile(place) == my)
                            map.SetTile(place, both);
                    }
                    break;

                case "N":
                    DataHolder.WinFlag = CheckWin(4, 6, -3, -3);
                    if (DataHolder.WinFlag == 0)
                    {
                        DataHolder.CanMove = true;
                        DataHolder.TimerT.GetComponent<timer>().StartTimer();
                    }                    
                    break;
            }
            DataHolder.MessageTCPforGame.RemoveAt(0);
        }
    }

    // (-3 2) (2 2)
    // (-3 -3) (2 -3)

    /// <summary>
    /// Победа в крестиках-ноликах. Возвращает: 0 - продолжить игру, 1 - победа, -1 - проигрыш, 2 - ничья 
    /// </summary>
    /// <param name="count">Размер выигрышной последовательности</param>
    /// <param name="countFull">Размер игрового поля countFullХcountFull</param>
    /// <param name="startX">Координата X нулевого тайла (левый нижний угол)</param>
    /// <param name="startY">Координата Y нулевого тайла (левый нижний угол)</param>
    private int CheckWin(int count, int countFull, int startX, int startY)
    {
        int win = 0;
        bool win1 = false, win2 = false;

        // Проверка победы обоих игроков, с учётом сдвигов по координатам
        for (int i = startX; i < startX + countFull - count + 1; i++)
        {
            for (int j = startY; j < startY + countFull - count + 1; j++)
            {
                if (!win1)
                {
                    if (CheckWinWithOffset(my, count, i, j))
                        win1 = true;
                }
                if (!win2)
                {
                    if (CheckWinWithOffset(enemy, count, i, j))
                        win2 = true;
                }
            }
        }
          
        if (win1 == true && win2 == false)
            win = 1;
        if (win1 == false && win2 == true)
            win = -1;
        if (win1 == true && win2 == true)
            win = 2;

        // Проверка поля на полную заполненность
        if (win == 0)
        {
            List<Vector3Int> emptyCoor = new List<Vector3Int>();
            int emptyPlace = 0;
            for (int i = startX; i < startX + countFull + 1; i++)
            {
                for (int j = startY; j < startY + countFull + 1; j++)
                {
                    Vector3Int check = new Vector3Int(i, j, 0);

                    if (map.GetTile(check) == main)
                    {
                        emptyPlace++;
                        emptyCoor.Add(check);
                        if (emptyPlace > 2)
                            goto SkipDrawCheck;
                    }
                }
            }
            win = 2;
            // Перекрашиваем оставшиеся 1 или 2 клетки в общий
            while (emptyCoor.Count > 0)
            {
                map.SetTile(emptyCoor[0], both);
                emptyCoor.RemoveAt(0);
            }
        }

        SkipDrawCheck:
        return win;
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
                
                if (map.GetTile(check) != symb)
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

                if (map.GetTile(check) != symb)
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
            if (map.GetTile(check) != symb)
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
            if (map.GetTile(check) != symb)
            {
                result = false;
                break;
            }
        }
        if (result) return result;

        return false;
    }

    
}
