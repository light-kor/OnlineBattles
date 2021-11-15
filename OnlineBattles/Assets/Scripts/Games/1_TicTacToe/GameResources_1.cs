using GameEnumerations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game1
{
    public class GameResources_1 : GeneralController
    {
        [SerializeField] private Tile _mainSquare, _blueCircle, _redCross;
        [SerializeField] private Tilemap _playLayer;

        public static GameResources_1 GameResources;

        private bool _firstPlayerTurn = true;
        private const int FieldSize = 3;
        private const int WinRow = 3;

        private int[,] _field = new int[3, 3];

        private void Awake()
        {
            GameResources = this;
            SetArrayStart();
        }

        private void Update()
        {
            if (DataHolder.GameType == GameTypes.Local)
                LocalMove();
        }

        private void LocalMove()
        {
            if (GameOn && Input.GetMouseButtonDown(0))
            {
                Vector3 clickWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                PlayerTypes type;

                if (_firstPlayerTurn)
                    type = PlayerTypes.BluePlayer;
                else
                    type = PlayerTypes.RedPlayer;

                if (TrySetTile(clickWorldPosition, type, out Vector3Int clickCellPosition))
                    _firstPlayerTurn = !_firstPlayerTurn;
            }
        }

        public bool TrySetTile(Vector3 clickWorldPosition, PlayerTypes playerType, out Vector3Int clickCellPosition)
        {
            clickCellPosition = _playLayer.WorldToCell(clickWorldPosition);

            if (_playLayer.GetTile(clickCellPosition) == _mainSquare)
            {
                SetTile(clickCellPosition, playerType);
                return true;
            }
            else return false;
        }

        public void SetTile(Vector3Int clickCellPosition, PlayerTypes playerType)
        {
            Tile tile = null;

            if (playerType == PlayerTypes.BluePlayer)
            {
                tile = _blueCircle;
                _field[clickCellPosition.x + 2, clickCellPosition.y + 1] = 2;
            }
            else if (playerType == PlayerTypes.RedPlayer)
            {
                tile = _redCross;
                _field[clickCellPosition.x + 2, clickCellPosition.y + 1] = 3;
            }

            _playLayer.SetTile(clickCellPosition, tile);

            if (DataHolder.GameType != GameTypes.WifiClient)
                CheckEndGame3x3();
        }

        private void SetArrayStart()
        {
            for (int i = 0; i < FieldSize; i++)
            {
                for (int j = 0; j < FieldSize; j++)
                {
                    _field[i, j] = 1;
                }
            }
        }

        private void CheckEndGame3x3()
        {
            bool blueWin = false, redWin = false;
            int row = 1, column = 1, diag = 1, backDiag = 1;
            int emptyCount = 0;

            for (int i = 0; i < FieldSize; i++)
            {
                for (int j = 0; j < FieldSize; j++)
                {
                    row *= _field[i, j];
                    column *= _field[j, i];

                    if (_field[i, j] == 1)
                        emptyCount++;
                }

                if (row == 8 || column == 8)
                    blueWin = true;
                if (row == 27 || column == 27)
                    redWin = true;

                row = 1;
                column = 1;

                diag *= _field[i, i];
                backDiag *= _field[i, FieldSize - i - 1];
            }

            if (diag == 8 || backDiag == 8)
                blueWin = true;
            if (diag == 27 || backDiag == 27)
                redWin = true;

            if (blueWin == true && redWin == false)
                UpdateScoreAndCheckGameState(PlayerTypes.BluePlayer, GameResults.Win, 1, false);
            else if (blueWin == false && redWin == true)
                UpdateScoreAndCheckGameState(PlayerTypes.RedPlayer, GameResults.Win, 1, false);
            else if (blueWin == true && redWin == true)
                UpdateScoreAndCheckGameState(PlayerTypes.Both, GameResults.Draw, 1, false);
            else if (emptyCount == 0)
                UpdateScoreAndCheckGameState(PlayerTypes.Both, GameResults.Draw, 1, false);
        }

        public void CheckEndGame4x4()
        {
            int startX = -FieldSize / 2;
            int startY = startX;
            bool firstGetRow = false, secondGetRow = false;

            // Проверка победы обоих игроков, с учётом сдвигов по координатам
            for (int i = startX; i < startX + FieldSize - WinRow + 1; i++)
            {
                for (int j = startY; j < startY + FieldSize - WinRow + 1; j++)
                {
                    if (CheckWinWithOffset(_blueCircle, WinRow, i, j))
                        firstGetRow = true;

                    if (CheckWinWithOffset(_redCross, WinRow, i, j))
                        secondGetRow = true;
                }
            }

            if (firstGetRow == true && secondGetRow == false)
                UpdateScoreAndCheckGameState(PlayerTypes.BluePlayer, GameResults.Win, 1, false);
            else if (firstGetRow == false && secondGetRow == true)
                UpdateScoreAndCheckGameState(PlayerTypes.RedPlayer, GameResults.Win, 1, false);
            else if (firstGetRow == true && secondGetRow == true)
                UpdateScoreAndCheckGameState(PlayerTypes.Both, GameResults.Draw, 1, false);
            else
            {
                int emptyPlace = 0;
                for (int i = startX; i < startX + FieldSize + 1; i++)
                {
                    for (int j = startY; j < startY + FieldSize + 1; j++)
                    {
                        Vector3Int check = new Vector3Int(i, j, 0);

                        if (_playLayer.GetTile(check) == _mainSquare)
                        {
                            emptyPlace++;                      
                        }
                    }
                }

                if (emptyPlace == 0)
                    UpdateScoreAndCheckGameState(PlayerTypes.Both, GameResults.Draw, 1, false);
                else return;
            }
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

                    if (_playLayer.GetTile(check) != symb)
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

                    if (_playLayer.GetTile(check) != symb)
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
                if (_playLayer.GetTile(check) != symb)
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
                if (_playLayer.GetTile(check) != symb)
                {
                    result = false;
                    break;
                }
            }
            if (result) return result;

            return false;
        }
    }
}
