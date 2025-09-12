using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Grid (use odd numbers)")]
    public int width = 21;   // odd
    public int height = 21;  // odd
    public float cellSize = 1f;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject startMarkerPrefab;
    public GameObject finishMarkerPrefab;
    public GameObject pathMarkerPrefab;

    private bool[,] open; // true = carved/open
    private readonly List<GameObject> spawned = new List<GameObject>();
    private List<Vector2Int> solutionPath = new List<Vector2Int>();

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

        // Enforce odd dimensions for 2-step carving and clamp minimum
        width = Mathf.Max(3, width | 1);
        height = Mathf.Max(3, height | 1);

        open = new bool[width, height];

        // Cells at odd indices; walls at even
        Vector2Int start = new Vector2Int(1, 1);

        GenerateFullMaze(start);

        // Choose exit: farthest open cell on perimeter via BFS distance map
        Vector2Int exit = FindFarthestPerimeter(start);

        // Solve from start to exit on open grid
        SolveMazeBFS(start, exit, out solutionPath);

        // Draw
        DrawMaze();
        PlaceStartAndFinish(start, exit);
        DrawCorrectPath(solutionPath);
    }

    // ----- Generation: stack-based recursive backtracker (spanning tree)

    void GenerateFullMaze(Vector2Int start)
    {
        var stack = new Stack<Vector2Int>();
        var visited = new bool[width, height];

        // Ensure start is open and valid (odd indices)
        start = new Vector2Int(Mathf.Clamp(start.x | 1, 1, width - 2), Mathf.Clamp(start.y | 1, 1, height - 2));
        visited[start.x, start.y] = true;
        open[start.x, start.y] = true;
        stack.Push(start);

        var dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (stack.Count > 0)
        {
            var current = stack.Peek();

            // Find unvisited neighbor "cells" two steps away (odd indices)
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
                stack.Pop();
                continue;
            }

            // Randomly pick next cell and carve wall between
            var next = neighbors[Random.Range(0, neighbors.Count)];
            var between = new Vector2Int((current.x + next.x) / 2, (current.y + next.y) / 2);

            open[between.x, between.y] = true; // corridor
            open[next.x, next.y] = true;       // next cell
            visited[next.x, next.y] = true;
            stack.Push(next);
        }
    }

    bool IsInside(int x, int y) => x > 0 && y > 0 && x < width - 1 && y < height - 1;

    // ----- Pick exit: farthest open perimeter cell from start

    Vector2Int FindFarthestPerimeter(Vector2Int start)
    {
        // BFS distance
        int[,] dist = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                dist[x, y] = -1;

        var q = new Queue<Vector2Int>();
        q.Enqueue(start);
        dist[start.x, start.y] = 0;

        var dirs = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;
                if (nx >= 0 && ny >= 0 && nx < width && ny < height && open[nx, ny] && dist[nx, ny] == -1)
                {
                    dist[nx, ny] = dist[cur.x, cur.y] + 1;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        // Scan perimeter for the farthest open cell
        Vector2Int best = start;
        int bestDist = -1;

        System.Action<int, int> consider = (x, y) =>
        {
            if (open[x, y] && dist[x, y] > bestDist)
            {
                bestDist = dist[x, y];
                best = new Vector2Int(x, y);
            }
        };

        for (int x = 1; x < width - 1; x += 1) { consider(x, 1); consider(x, height - 2); }
        for (int y = 1; y < height - 1; y += 1) { consider(1, y); consider(width - 2, y); }

        // Fallback in the unlikely event none found (shouldn’t happen)
        if (bestDist < 0) best = new Vector2Int(width - 2, height - 2);

        return best;
    }

    // ----- Solve shortest path via BFS

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
                if (nx >= 0 && ny >= 0 && nx < width && ny < height && open[nx, ny] && !visited[nx, ny])
                {
                    visited[nx, ny] = true;
                    var next = new Vector2Int(nx, ny);
                    cameFrom[next] = cur;
                    q.Enqueue(next);
                }
            }
        }

        return false;
    }

    // ----- Drawing

    void DrawMaze()
    {
        // Instantiate walls where open == false
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!open[x, y])
                {
                    var pos = new Vector3(x * cellSize, 0.5f, y * cellSize);
                    var wall = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                    spawned.Add(wall);
                }
            }
        }
    }

    void PlaceStartAndFinish(Vector2Int start, Vector2Int exit)
    {
        if (startMarkerPrefab)
            spawned.Add(Instantiate(startMarkerPrefab, new Vector3(start.x * cellSize, 0.5f, start.y * cellSize), Quaternion.identity, transform));
        if (finishMarkerPrefab)
            spawned.Add(Instantiate(finishMarkerPrefab, new Vector3(exit.x * cellSize, 0.5f, exit.y * cellSize), Quaternion.identity, transform));
    }

    void DrawCorrectPath(List<Vector2Int> path)
    {
        if (!pathMarkerPrefab || path == null) return;
        foreach (var cell in path)
        {
            var pos = new Vector3(cell.x * cellSize, 0.06f, cell.y * cellSize);
            spawned.Add(Instantiate(pathMarkerPrefab, pos, Quaternion.identity, transform));
        }
    }
}
