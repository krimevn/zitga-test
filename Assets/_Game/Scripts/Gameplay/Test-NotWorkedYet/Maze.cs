using UnityEngine;
using System.Collections.Generic;

public class Maze
{
    private Cell[,] grid;
    private int width;
    private int height;

    public Maze(int width, int height)
    {
        this.width = width;
        this.height = height;
        grid = new Cell[width, height];
        InitializeGrid();
        GenerateMaze();
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell(x, y, true);
            }
        }
    }

    private void GenerateMaze()
    {
        List<Cell> frontier = new List<Cell>();
        Cell startCell = grid[1, 1];
        startCell.isWall = false;
        frontier.Add(startCell);

        while (frontier.Count > 0)
        {
            int randomIndex = Random.Range(0, frontier.Count);
            Cell currentCell = frontier[randomIndex];
            frontier.RemoveAt(randomIndex);

            List<Cell> neighbors = GetWallNeighbors(currentCell);

            if (neighbors.Count > 0)
            {
                int neighborIndex = Random.Range(0, neighbors.Count);
                Cell neighbor = neighbors[neighborIndex];

                grid[neighbor.x, neighbor.y].isWall = false;
                grid[(currentCell.x + neighbor.x) / 2, (currentCell.y + neighbor.y) / 2].isWall = false;

                frontier.AddRange(GetFrontierNeighbors(neighbor));
            }
        }
    }

    public Cell GetCell(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return grid[x, y];
        }
        return null;
    }

    private List<Cell> GetWallNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();
        Vector2Int[] directions = { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 };

        foreach (Vector2Int dir in directions)
        {
            int x = cell.x + dir.x;
            int y = cell.y + dir.y;
            if (x >= 0 && x < width && y >= 0 && y < height && grid[x, y].isWall)
            {
                neighbors.Add(grid[x, y]);
            }
        }
        return neighbors;
    }

    private List<Cell> GetFrontierNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();
        Vector2Int[] directions = { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 };

        foreach (Vector2Int dir in directions)
        {
            int x = cell.x + dir.x;
            int y = cell.y + dir.y;
            if (x >= 0 && x < width && y >= 0 && y < height && grid[x, y].isWall)
            {
                neighbors.Add(grid[x, y]);
            }
        }
        return neighbors;
    }
}