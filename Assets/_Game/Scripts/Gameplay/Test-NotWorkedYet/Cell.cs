using UnityEngine;

public class Cell
{
    public int x;
    public int y;
    public bool isWall;
    public Vector2Int wallDirection = Vector2Int.zero;

    public Cell(int x, int y, bool isWall)
    {
        this.x = x;
        this.y = y;
        this.isWall = isWall;
    }
}