using UnityEngine;

public class MazeSpawner : MonoBehaviour
{
    public Cell CellPrefab;
    [SerializeField] private Joystick joystick;
    [SerializeField] private GameObject me;
    //private float x1 = -1.5f, y1 = -3.5f;
    private float x1 = 0f, y1 = 0f;
    public Vector2 velocity;
    public Rigidbody2D rb2D;


    private void Start()
    {
        rb2D = me.GetComponent<Rigidbody2D>();
        MazeGenerator generator = new MazeGenerator();
        MazeGeneratorCell[,] maze = generator.GenerateMaze();

        int first = maze.GetLength(0);
        int sec = maze.GetLength(1);

        for (int x = 0; x < first; x++)
        {
            for (int y = 0; y < sec; y++)
            {
                Cell c = Instantiate(CellPrefab, new Vector2(x - first / 2, y - sec / 2), Quaternion.identity).GetComponent<Cell>();

                c.WallLeft.SetActive(maze[x, y].WallLeft);
                c.WallBottom.SetActive(maze[x, y].WallBottom);
            }
        }

        me.transform.position = new Vector2(-first / 2 + 0.5f, -sec / 2 + 0.5f);
    }

    private void Update()
    {
        x1 = joystick.Horizontal;
        y1 = joystick.Vertical;

        velocity = new Vector2(x1, y1);

    }

    void FixedUpdate()
    {

        rb2D.MovePosition(rb2D.position + velocity * Time.fixedDeltaTime * 3);
    }

        
}
