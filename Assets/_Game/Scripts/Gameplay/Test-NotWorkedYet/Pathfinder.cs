using UnityEngine;
using System.Collections.Generic;

public class Pathfinder
{
    private Maze maze;

    public Pathfinder(Maze maze)
    {
        this.maze = maze;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> costSoFar = new Dictionary<Vector2Int, float>();
        SimplePriorityQueue<Vector2Int> frontier = new SimplePriorityQueue<Vector2Int>();

        frontier.Enqueue(start, 0);
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == end)
            {
                return ReconstructPath(cameFrom, end);
            }

            foreach (Vector2Int next in GetNeighbors(current))
            {
                float newCost = costSoFar[current] + 1;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + Heuristic(next, end);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        return null; // No path found
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(current);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            Cell neighborCell = maze.GetCell(neighbor.x, neighbor.y);
            if (neighborCell != null && !neighborCell.isWall)
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }
}