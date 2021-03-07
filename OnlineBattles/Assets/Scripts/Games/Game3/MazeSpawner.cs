using System.Collections;
using UnityEngine;

public class MazeSpawner : MonoBehaviour
{
    [SerializeField] private Cell CellPrefab;
    [SerializeField] private Joystick joystick;
    [SerializeField] private GameObject me, pointPref;
    private GameObject _map;
    private float x1 = 0f, y1 = 0f;
    private Vector2 velocity;
    private Rigidbody2D rb2D;
    int first, sec;
    const int mashtab = 2;
    float seconds = 0f;

    private void Start()
    {
        ManualCreateMapButton.Click += ChangeMap;
        PointEnterHandler.Catch += GetPoint;
        rb2D = me.GetComponent<Rigidbody2D>();

        ChangeMap();
        me.transform.position = new Vector2(((-first / 2) + 0.5f) / mashtab, ((-sec / 2) + 0.5f) / mashtab);
    }

    private void Update()
    {
        x1 = joystick.Horizontal;
        y1 = joystick.Vertical;

        velocity = new Vector2(x1, y1);

        seconds += Time.deltaTime;

        if (seconds > 5f)
            ChangeMap();
    }

    void GetPoint(GameObject player)
    {
        Debug.Log(player.name);
    }

    public void ChangeMap()
    {
        if (_map != null)
            Destroy(_map);

        _map = new GameObject("Cells");
        Create(_map);
        seconds = 0f;

        float X = Random.Range(-first / 2, first / 2) + 0.5f;
        float Y = Random.Range(-sec / 2, sec / 2) + 0.5f;

        Instantiate(pointPref, new Vector2(X / mashtab, Y / mashtab), Quaternion.identity);
        //TODO: Карта меняется сама каждые 3 секунды, но каждый может изменить карту сам раз в 5 секунд. Это навык каждого
    }


    void Create(GameObject parent)
    {
        MazeGenerator generator = new MazeGenerator();
        MazeGeneratorCell[,] maze = generator.GenerateMaze();

        first = maze.GetLength(0);
        sec = maze.GetLength(1);

        for (int x = 0; x < first; x++)
        {
            for (int y = 0; y < sec; y++)
            {
                float X = (x - (float)(first / 2)) / mashtab;
                float Y = (y - (float)(sec / 2)) / mashtab;
                Cell c = Instantiate(CellPrefab, new Vector2(X, Y), Quaternion.identity, parent.transform).GetComponent<Cell>();

                c.WallLeft.SetActive(maze[x, y].WallLeft);
                c.WallBottom.SetActive(maze[x, y].WallBottom);
            }
        }
    }
    //TODO: попробовать брать не 1 длину линий, а рандомную, тогда будут появляться доп проходы

    void FixedUpdate()
    {
        rb2D.MovePosition(rb2D.position + velocity * Time.fixedDeltaTime * 3);
    }

        
}
