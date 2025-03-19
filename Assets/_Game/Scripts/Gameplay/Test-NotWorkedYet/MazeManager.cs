using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{
    public int mazeWidth = 10;
    public int mazeHeight = 13;
    public Sprite wallSprite;
    public Sprite characterSprite;
    public Sprite targetSprite;
    public float cellSize = 1f;

    private Maze maze;
    private Pathfinder pathfinder;
    private Character character;
    private GameObject target;
    private Vector2Int targetPos;

    private void Start()
    {
        GenerateMaze();
        SpawnCharacterAndTarget();
    }

    private void GenerateMaze()
    {
        maze = new Maze(mazeWidth, mazeHeight);
        pathfinder = new Pathfinder(maze);

        // Spawn Walls
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (maze.GetCell(x, y).isWall)
                {
                    Vector2 position = GridToWorldPosition(new Vector2Int(x, y));
                    GameObject wall = new GameObject("Wall_" + x + "_" + y);
                    SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
                    renderer.sprite = wallSprite;
                    wall.transform.position = position;
                    wall.transform.localScale = new Vector3(cellSize, cellSize, 1);
                    // Ensure no rotation is applied
                    wall.transform.rotation = Quaternion.identity;
                }
            }
        }
    }

    private void SpawnCharacterAndTarget()
    {
        Vector2Int startPos = new Vector2Int(1, 1);
        GameObject characterObject = new GameObject("Character");
        SpriteRenderer characterRenderer = characterObject.AddComponent<SpriteRenderer>();
        characterRenderer.sprite = characterSprite;
        characterObject.transform.localScale = new Vector3(cellSize, cellSize, 1);
        character = characterObject.AddComponent<Character>();
        character.Initialize(this, startPos);

        do
        {
            targetPos = new Vector2Int(Random.Range(1, mazeWidth - 1), Random.Range(1, mazeHeight - 1));
        } while (maze.GetCell(targetPos.x, targetPos.y).isWall);

        target = new GameObject("Target");
        SpriteRenderer targetRenderer = target.AddComponent<SpriteRenderer>();
        targetRenderer.sprite = targetSprite;
        target.transform.localScale = new Vector3(cellSize, cellSize, 1);
        target.transform.position = GridToWorldPosition(targetPos);
    }

    public void AutoMove()
    {
        List<Vector2Int> path = pathfinder.FindPath(character.GetPosition(), targetPos);
        if (path != null && path.Count > 1)
        {
            character.MoveAlongPath(path);
        }
        else
        {
            Debug.Log("No path found");
        }
    }

    public Vector2 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector2(gridPos.x * cellSize, gridPos.y * cellSize);
    }
}