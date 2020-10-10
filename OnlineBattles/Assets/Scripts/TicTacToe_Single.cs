using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TicTacToe_Single : MonoBehaviour
{
    public Tile my, main, enemy, both;
    private Tilemap map;
    private Camera mainCam;
    private bool canMove = true, firstMove = true, firstPlayer = true;
    private Vector3Int fir, sec;

    public GameObject panel;
    public Text endText;

    private void Start()
    {
        Application.runInBackground = true;

        map = GameObject.FindGameObjectWithTag("TileMap").GetComponent<Tilemap>();
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && (canMove == true))
        {
            Vector3 clickWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickCellPosition = map.WorldToCell(clickWorldPosition);
            // Выбираем, какой игрок ходит            
            if (map.GetTile(clickCellPosition) == main)
            {

                if (firstPlayer)
                {
                    if (firstMove)
                    {
                        map.SetTile(clickCellPosition, my);
                        fir = clickCellPosition;
                        firstMove = false;
                    }
                    else
                    {
                        map.SetTile(clickCellPosition, my);
                        sec = clickCellPosition;
                        firstMove = true;
                        firstPlayer = false;
                        map.SetTile(fir, main);
                        map.SetTile(sec, main);
                    }
                }
                else
                {
                    if (firstMove)
                    {
                        map.SetTile(clickCellPosition, enemy);
                        firstMove = false;
                    }
                    else
                    {
                        map.SetTile(clickCellPosition, enemy);
                        firstMove = true;
                        firstPlayer = true;
                        if (map.GetTile(fir) == main)
                            map.SetTile(fir, my);
                        else map.SetTile(fir, both);

                        if (map.GetTile(sec) == main)
                            map.SetTile(sec, my);
                        else map.SetTile(sec, both);
                    }
                }
            }

            // Проверяем состояние игры после хода второго
            if (firstPlayer && firstMove)
            {
                canMove = false;
                GameOver();                
            }                
        }       
    }

    private void GameOver()
    {
        int win = CheckWin(4, 6, -3, -3);
        if (win == 1 || win == -1 || win == 2)
        {
            if (win == 1)
            {
                endText.text = "Player 1 WIN";
            }
            else if (win == -1)
            {
                endText.text = "Player 2 WIN";
            }
            else if (win == 2)
            {
                endText.text = "DRAWN";
            }
            panel.SetActive(true);
        }
        else canMove = true;
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
    public int CheckWin(int count, int countFull, int startX, int startY)
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

    public void RestartLvl()
    {       
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

