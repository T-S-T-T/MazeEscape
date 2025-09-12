using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Grid (use odd numbers)")]
    public int width = 21;   // odd
    public int height = 21;  // odd

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject startMarkerPrefab;
    public GameObject finishMarkerPrefab;
    public GameObject pathMarkerPrefab;

    private bool[,] open; // true = carved/open
    private List<Vector2Int> path = new List<Vector2Int>();
    private readonly List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        GenerateAndDraw();
    }

    public void GenerateAndDraw()
    {
        // Clear previous
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
        path.Clear();

        // Enforce odd dimensions for 2-step carving
        width = Mathf.Max(3, width | 1);
        height = Mathf.Max(3, height | 1);

        open = new bool[width, height];

        // Start on (0,0) cell (must be even indices in this scheme)
        var start = new Vector2Int(0, 0);
        var exit = new Vector2Int(width - 1, height - 1);

        // Carve perimeter cells to be consistent
        Carve(start);

        // Generate with DFS that returns true when exit found
        DFSFindPath(start.x, start.y, exit);

        DrawMaze();
        PlaceStartAndFinish(start, exit);
        DrawCorrectPath();
    }

    void Carve(Vector2Int p) => open[p.x, p.y] = true;

    bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

    bool DFSFindPath(int x, int y, Vector2Int exit)
    {
        // Mark current cell open and add to path if not present
        if (!open[x, y]) open[x, y] = true;
        path.Add(new Vector2Int(x, y));

        if (x == exit.x && y == exit.y)
            return true;

        var dirs = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }
            .OrderBy(_ => Random.value).ToList();

        foreach (var d in dirs)
        {
            // We carve walls between cells; step size 2 ensures “cell-wall-cell” pattern
            int nx = x + d.x * 2;
            int ny = y + d.y * 2;

            if (!IsInside(nx, ny) || open[nx, ny]) continue;

            // Carve corridor cell between current and next
            int bx = x + d.x;
            int by = y + d.y;
            open[bx, by] = true;

            // Add corridor to path so visuals are continuous
            path.Add(new Vector2Int(bx, by));

            // Carve next cell and recurse
            open[nx, ny] = true;
            if (DFSFindPath(nx, ny, exit))
                return true;

            // Backtrack: remove corridor and continue trying other directions
            path.RemoveAt(path.Count - 1); // remove corridor
        }

        // Backtrack current cell if dead end
        path.RemoveAt(path.Count - 1);
        return false;
    }

    void DrawMaze()
    {
        // Instantiate walls where open == false
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!open[x, y])
                {
                    var pos = new Vector3(x, 0.5f, y);
                    spawned.Add(Instantiate(wallPrefab, pos, Quaternion.identity, transform));
                }
            }
        }
    }

    void PlaceStartAndFinish(Vector2Int start, Vector2Int exit)
    {
        var s = Instantiate(startMarkerPrefab, new Vector3(start.x, 0.5f, start.y), Quaternion.identity, transform);
        var f = Instantiate(finishMarkerPrefab, new Vector3(exit.x, 0.5f, exit.y), Quaternion.identity, transform);
        spawned.Add(s);
        spawned.Add(f);
    }

    void DrawCorrectPath()
    {
        if (pathMarkerPrefab == null) return;

        foreach (var cell in path)
        {
            // Slightly above ground to avoid z-fighting
            var pos = new Vector3(cell.x, 0.06f, cell.y);
            var marker = Instantiate(pathMarkerPrefab, pos, Quaternion.identity, transform);
            spawned.Add(marker);
        }
    }
}
