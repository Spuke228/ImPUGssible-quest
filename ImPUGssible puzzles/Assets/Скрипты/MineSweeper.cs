using UnityEngine;

public class Minesweeper : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public int mines = 10;

    int[,] grid;

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        grid = new int[width, height];

        for (int i = 0; i < mines; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            grid[x, y] = -1;
        }
    }
}