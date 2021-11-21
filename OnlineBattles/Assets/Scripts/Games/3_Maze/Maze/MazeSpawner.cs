using GameEnumerations;
using UnityEngine;

namespace Game3
{
    public class MazeSpawner : MonoBehaviour
    {
        [SerializeField] private Cell _cellPrefab;

        private int _scale = GameResources_3.Scale;
        private int _width = GameResources_3.Width;
        private int _height = GameResources_3.Height;
        private float _lastChangeMazeTime = 0f;
        private bool _mazeBuilt = false;
        private Cell[,] _field = new Cell[GameResources_3.Width, GameResources_3.Height];

        private const float MazeUpdateTime = 8f;

        private void Start()
        {
            if (DataHolder.GameType != GameTypes.WifiClient)
                ManualCreateMapButton.Click += CreateMaze;
        }

        private void Update()
        {
            if (GeneralController.GameOn)
            {
                if (DataHolder.GameType != GameTypes.WifiClient)
                {
                    _lastChangeMazeTime += Time.deltaTime;

                    if (_lastChangeMazeTime > MazeUpdateTime)
                        CreateMaze();
                }
            }            
        }       

        public void CreateMaze()
        {
            MazeGenerator generator = new MazeGenerator(_width, _height);
            MazeGeneratorCell[,] maze = generator.GenerateMaze();

            if (DataHolder.GameType == GameTypes.WifiHost)
                Serializer<MazeGeneratorCell[,]>.SendMessage(maze, ConnectTypes.TCP);                

            if (_mazeBuilt == false)
                BuildMaze(maze);
            else
                RefreshMaze(maze);

            _lastChangeMazeTime = 0f;
        }

        public void CreateMazeRemote()
        {
            if (Network.BigMessagesTCP.Count > 0)
            {
                MazeGeneratorCell[,] maze = Serializer<MazeGeneratorCell[,]>.GetMessage(Network.BigMessagesTCP[0]);
                Network.BigMessagesTCP.RemoveAt(0);

                if (_mazeBuilt == false)
                    BuildMaze(maze);
                else
                    RefreshMaze(maze);
            }          
        }

        public void MazeColliderSwitch(bool enabled)
        {
            _cellPrefab.WallLeft.GetComponent<EdgeCollider2D>().enabled = enabled;
            _cellPrefab.WallBottom.GetComponent<EdgeCollider2D>().enabled = enabled;
        }

        private void BuildMaze(MazeGeneratorCell[,] maze)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    float X = (x - (float)(_width / 2)) / _scale;
                    float Y = (y - (float)(_height / 2)) / _scale;
                    Cell cell = Instantiate(_cellPrefab, new Vector2(X, Y), Quaternion.identity, transform).GetComponent<Cell>();

                    cell.WallLeft.SetActive(maze[x, y].WallLeft);
                    cell.WallBottom.SetActive(maze[x, y].WallBottom);

                    _field[x, y] = cell;
                }
            }
            _mazeBuilt = true;
        }

        private void RefreshMaze(MazeGeneratorCell[,] maze)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _field[x, y].WallLeft.SetActive(maze[x, y].WallLeft);
                    _field[x, y].WallBottom.SetActive(maze[x, y].WallBottom);
                }
            }
        }       

        private void OnDestroy()
        {
            if (DataHolder.GameType != GameTypes.WifiClient)
                ManualCreateMapButton.Click -= CreateMaze;
        }
    }
}
