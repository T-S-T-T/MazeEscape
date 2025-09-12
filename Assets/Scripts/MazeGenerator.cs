using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    private bool[,] maze;
    private List<Vector2Int> correctPath = new List<Vector2Int>();

    public GameObject wallPrefab;
    public GameObject startMarkerPrefab;
    public GameObject finishMarkerPrefab;
    public GameObject pathMarkerPrefab;

    void Start()
    {
        maze = new bool[width, height];
        GenerateMaze(0, 0);
        DrawMaze();
        PlaceStartAndFinish();
        DrawCorrectPath();
    }

    void GenerateMaze(int x, int y)
    {
        maze[x, y] = true;
        correctPath.Add(new Vector2Int(x, y));

        if (x == width - 1 && y == height - 1)
            return; // Exit reached

        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }.OrderBy(d => Random.value).ToList();

        foreach (var dir in directions)
        {
            int nx = x + dir.x * 2;
            int ny = y + dir.y * 2;

            if (nx >= 0 && ny >= 0 && nx < width && ny < height && !maze[nx, ny])
            {
                maze[x + dir.x, y + dir.y] = true;
                GenerateMaze(nx, ny);

                if (correctPath.Last() == new Vector2Int(width - 1, height - 1))
                    return; // Stop once exit is reached
            }
        }

        //correctPath.RemoveAt(correctPath.Count - 1); // Backtrack
    }


    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!maze[x, y])
                {
                    Vector3 pos = new Vector3(x, 0.5f, y);
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }
            }
        }
    }

    void PlaceStartAndFinish()
    {
        Vector3 startPos = new Vector3(0, 0.5f, 0); // Entrance
        Vector3 finishPos = new Vector3(width - 1, 0.5f, height - 1); // Exit

        Instantiate(startMarkerPrefab, startPos, Quaternion.identity);
        Instantiate(finishMarkerPrefab, finishPos, Quaternion.identity);
    }

    void DrawCorrectPath()
    {
        Debug.Log(correctPath);
        foreach (Vector2Int pos in correctPath)
        {
            Debug.Log("Path");
            Vector3 worldPos = new Vector3(pos.x, 0.1f, pos.y);
            Instantiate(pathMarkerPrefab, worldPos, Quaternion.identity);
        }
    }



}
