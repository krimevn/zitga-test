using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    public float moveSpeed = 2f;
    private MazeManager mazeManager;
    private Vector2Int currentPos;

    public void Initialize(MazeManager mazeManager, Vector2Int startPos)
    {
        this.mazeManager = mazeManager;
        currentPos = startPos;
        transform.position = mazeManager.GridToWorldPosition(currentPos);
    }

    public void MoveAlongPath(List<Vector2Int> path)
    {
        StartCoroutine(MoveAlongPathCoroutine(path));
    }

    private IEnumerator MoveAlongPathCoroutine(List<Vector2Int> path)
    {
        for (int i = 1; i < path.Count; i++)
        {
            currentPos = path[i];
            Vector2 targetPosition = mazeManager.GridToWorldPosition(currentPos);
            while (Vector2.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    public Vector2Int GetPosition()
    {
        return currentPos;
    }
}