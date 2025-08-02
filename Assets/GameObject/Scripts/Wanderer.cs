using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Wanderer : Unit
{
    private ResourceManager resourceManager;

    private new Vector2Int? currentTarget;

    private new void Awake()
    {
        base.Awake();
    }

    protected new void Update()
    {
        base.Update();

        if (currentPath.Count > 0)
        {
            Vector2Int targetPosition = currentPath[0];
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Arrive at new node
            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                // Check if new node is occupied while moving
                if (!gameManager.GetObstaclesInBoolean(false)[targetPosition.x, targetPosition.y])
                {
                    transform.position = new Vector2(gridX, gridY);
                    Move(targetPosition.x, targetPosition.y);
                }
                else
                {
                    currentPath.RemoveAt(0);
                    gridX = targetPosition.x;
                    gridY = targetPosition.y;
                }

                Trap[] traps = FindObjectsOfType<Trap>();
                foreach (Trap trap in traps)
                {
                    if (gridX == trap.gridX && gridY == trap.gridY && team != trap.team)
                    {

                        trap.OnHit(this);
                        break;
                    }
                }
            }
        }

        if (currentPath.Count == 0 && currentTarget == null)
            Move(Random.Range(0, gameManager.width), Random.Range(0, gameManager.height));
    }
}
