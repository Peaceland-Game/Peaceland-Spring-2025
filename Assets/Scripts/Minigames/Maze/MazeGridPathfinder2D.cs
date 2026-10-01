using System.Collections.Generic;
using UnityEngine;

public static class MazeGridPathfinder2D
{
    private const int MaxSearchNodes = 4096;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.right,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.down
    };

    public static bool TryBuildPath(
        Vector2 startWorld,
        Vector2 targetWorld,
        float cellSize,
        Vector2 collisionSize,
        LayerMask blockingMask,
        out List<Vector2> path)
    {
        path = new List<Vector2>();
        float safeCellSize = Mathf.Max(0.01f, cellSize);
        Vector2Int start = ToCell(startWorld, safeCellSize);
        Vector2Int target = ToCell(targetWorld, safeCellSize);
        if (start == target || IsBlocked(target, safeCellSize, collisionSize, blockingMask))
        {
            return false;
        }

        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        queue.Enqueue(start);

        bool found = false;
        int inspected = 0;
        while (queue.Count > 0 && inspected++ < MaxSearchNodes)
        {
            Vector2Int current = queue.Dequeue();
            foreach (Vector2Int direction in Directions)
            {
                Vector2Int next = current + direction;
                if (visited.Contains(next)
                    || IsBlocked(next, safeCellSize, collisionSize, blockingMask))
                {
                    continue;
                }

                visited.Add(next);
                cameFrom[next] = current;
                if (next == target)
                {
                    found = true;
                    queue.Clear();
                    break;
                }

                queue.Enqueue(next);
            }
        }

        if (!found)
        {
            return false;
        }

        for (Vector2Int cursor = target; cursor != start; cursor = cameFrom[cursor])
        {
            path.Add(new Vector2(cursor.x * safeCellSize, cursor.y * safeCellSize));
        }

        path.Reverse();
        return path.Count > 0;
    }

    private static Vector2Int ToCell(Vector2 worldPosition, float cellSize)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.y / cellSize));
    }

    private static bool IsBlocked(
        Vector2Int cell,
        float cellSize,
        Vector2 collisionSize,
        LayerMask blockingMask)
    {
        Vector2 worldPosition = new Vector2(cell.x * cellSize, cell.y * cellSize);
        return Physics2D.OverlapBox(worldPosition, collisionSize, 0f, blockingMask) != null;
    }
}
