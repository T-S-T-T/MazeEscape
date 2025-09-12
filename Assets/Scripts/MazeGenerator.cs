using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Grid (use odd numbers)")]
    public int width = 21;
    public int height = 21;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject startMarkerPrefab;
    public GameObject finishMarkerPrefab;
    public GameObject pathMarkerPrefab;

    private bool[,] open; // true = carved/open
    private List<Vector2Int> solutionPath = new List<Vector2Int>();
    private readonly List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        GenerateAndDraw();
    }

    public void GenerateAndDraw()
    {
        // Clear old
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
        solutionPath.Clear();

        // Enforce odd sizes
        width = Mathf.Max(3, width | 1);
        height = Mathf.Max(3, height | 1);

        open = new bool[width, height];

        var start = new Vector2Int(0, 0);
        var exit = new Vector2Int(width - 1, height - 1);

        // Phase 1: generate full maze
        GenerateFullMaze(start.x, start.y);

        // Phase 2: solve on carved grid
        SolveMazeBFS(start, exit, out solutionPath);

        // Draw
        DrawMaze();
        PlaceStartAndFinish(start, exit);
        DrawCorrectPath(solutionPath);
    }

    // -------- Phase 1: full maze generation (recursive backtracker, no early stop)

    void GenerateFullMaze(int sx, int sy)
    {
        // Initialize all as walls (open=false), then carve via stack-based DFS
        var stack = new Stack<Vector2Int>();
        var visited = new bool[width, height];

        Vector2Int start = new Vector2Int(sx, sy);
        visited[start.x, start.y] = true;
        open[start.x, start.y] = true;
        stack.Push(start);

        var dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (stack.Count > 0)
        {
            var current = stack.Peek();

            // Find unvisited neighbors two steps away
            var neighbors = new List<Vector2Int>();
            foreach (var d in dirs)
            {
                int nx = current.x + d.x * 2;
                int ny = current.y + d.y * 2;
                if (IsInside(nx, ny) && !visited[nx, ny])
                    neighbors.Add(new Vector2Int(nx, ny));
            }

            if (neighbors.Count == 0)
            {
                stack.Pop(); // backtrack
                continue;
            }

            // Pick a random neighbor and carve the wall between
            var next = neighbors[Random.Range(0, neighbors.Count)];
            var between = new Vector2Int((current.x + next.x) / 2, (current.y + next.y) / 2);

            open[between.x, between.y] = true; // corridor
            open[next.x, next.y] = true;       // next cell
            visited[next.x, next.y] = true;
            stack.Push(next);
        }
    }

    bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

    // -------- Phase 2: shortest path via BFS on open cells

    bool SolveMazeBFS(Vector2Int start, Vector2Int exit, out List<Vector2Int> path)
    {
        path = new List<Vector2Int>();
        var q = new Queue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new bool[width, height];

        q.Enqueue(start);
        visited[start.x, start.y] = true;

        var dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == exit)
            {
                // Reconstruct
                var p = cur;
                path.Add(p);
                while (cameFrom.ContainsKey(p))
                {
                    p = cameFrom[p];
                    path.Add(p);
                }
                path.Reverse();
                return true;
            }

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;
                if (IsInside(nx, ny) && !visited[nx, ny] && open[nx, ny])
                {
                    visited[nx, ny] = true;
                    var next = new Vector2Int(nx, ny);
                    cameFrom[next] = cur;
                    q.Enqueue(next);
                }
            }
        }

        return false; // no path (should not happen in a carved maze)
    }

    // -------- Drawing

    void DrawMaze()
    {
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

    void DrawCorrectPath(List<Vector2Int> path)
    {
        if (pathMarkerPrefab == null || path == null) return;

        foreach (var cell in path)
        {
            var pos = new Vector3(cell.x, 0.06f, cell.y); // slightly above floor
            spawned.Add(Instantiate(pathMarkerPrefab, pos, Quaternion.identity, transform));
        }
    }
}
