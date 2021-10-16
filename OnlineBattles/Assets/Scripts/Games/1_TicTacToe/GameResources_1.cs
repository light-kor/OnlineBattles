using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game1
{
    public class GameResources_1 : GameResourcesTemplate
    {
        public Tile MainTile, MyTile, EnemyTile;
        public Tilemap Map, PlayLayer;

        private readonly int FieldSize = 4;
        private readonly int WinRow = 3;

        public string CheckWin()
        {
            string winer = null;
            int startX = -FieldSize / 2;
            int startY = startX;
            bool firstGetRow = false, secondGetRow = false;

            // �������� ������ ����� �������, � ������ ������� �� �����������
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

            // �������� ���� �� ������ �������������
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
        /// �������� ��������� ���������. �������� �������� ��������� �������� count�count, 
        /// ���� ���� ���� ������� ������������������ �� �������, �������� ��������� �� ��������� ������������������.
        /// </summary>
        /// <param name="symb">����������� Tile</param>
        /// <param name="count">������ ���������� ������������������</param>
        /// <param name="offsetX">�������� �� X ����������</param>
        /// <param name="offsetY">�������� �� Y ����������</param>
        /// <returns>���������� true, ���� ���� ���� ���� ��� �� count ����</returns>
        private bool CheckWinWithOffset(Tile symb, int count, int offsetX, int offsetY)
        {
            bool result = true;
            for (int col = offsetX; col < offsetX + count; col++)
            {
                // �������� �� ���������
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

                // �������� �� �����������
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

            // �������� ��������� � ������ ����
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

            // �������� ��������� � ������� ����
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
}
