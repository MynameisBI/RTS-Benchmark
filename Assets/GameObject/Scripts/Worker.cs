using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

public class Worker : Unit
{
    public GameObject buildingPrefab;
    [HideInInspector]
    public Resource targetResource;

    private ResourceManager resourceManager;

    public enum WorkerUnitState
    {
        Idle,
        Moving,
        Building,
        Extracting,
    }
    public WorkerUnitState currentState;

    private new void Awake()
    {
        base.Awake();
        resourceManager = FindObjectOfType<ResourceManager>();
    }

    protected new void Update()
    {
        base.Update();

        switch (currentState)
        {
            case WorkerUnitState.Idle:
                if (resourceManager.GetResourceAmount(team) > 10)
                {
                    targetResource = null;
                    currentState = WorkerUnitState.Building;
                }
                else
                {
                    Resource[] resources = FindObjectsOfType<Resource>();
                    if (resources.Length > 0)
                    {
                        Resource target = null;
                        float closestDistance = float.MaxValue;
                        foreach (Resource resource in resources)
                        {
                            float distance = Vector2.Distance(transform.position, resource.transform.position);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                target = resource;
                            }
                        }

                        if (target != null)
                        {
                            List<Vector2Int> targetAdjacentTiles = gameManager.GetAdjacentWalkableTiles(target.gridX, target.gridY);
                            if (targetAdjacentTiles.Count >= 1)
                            {
                                targetResource = target;
                                Vector2Int targetTile = targetAdjacentTiles[Random.Range(0, targetAdjacentTiles.Count)];
                                Move(targetTile.x, targetTile.y);
                                currentState = WorkerUnitState.Moving;
                                break;
                            }
                        }
                    }
                }
                break;

            case WorkerUnitState.Moving:
                if (currentPath.Count == 0)
                    if (targetResource != null)
                        currentState = WorkerUnitState.Extracting;
                    else
                        currentState = WorkerUnitState.Idle;

                else if (currentPath.Count > 0)
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

                            if (targetResource != null && targetResource.IsDestroyed())
                            {
                                targetResource = null;
                                currentState = WorkerUnitState.Idle;
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
                }

                break;

            case WorkerUnitState.Building:
                if (resourceManager.GetResourceAmount(team) < 10)
                {
                    targetResource = null;
                    currentState = WorkerUnitState.Idle;
                    break;
                }

                List<Vector2Int> adjacentWalkableTiles = gameManager.GetAdjacentWalkableTiles(gridX, gridY);

                if (adjacentWalkableTiles.Count >= 2)
                {
                    Vector2Int adjacentWalkableTile = adjacentWalkableTiles[Random.Range(0, adjacentWalkableTiles.Count)];
                    if (gameManager.AddBuilding(adjacentWalkableTile.x, adjacentWalkableTile.y, team))
                    {
                        resourceManager.AddResource(team, -10);
                    }
                    targetResource = null;
                    currentState = WorkerUnitState.Idle;

                } else if (adjacentWalkableTiles.Count == 1)
                {
                    targetResource = null;
                    currentState = WorkerUnitState.Moving;
                    int targetX, targetY;
                    do { targetX = Random.Range(gridX - 6, gridX + 6); } while (targetX >= -2 && targetX <= 2);
                    do { targetY = Random.Range(gridY - 6, gridY + 6); } while (targetY >= -2 && targetY <= 2);
                    Move(Mathf.Clamp(targetX, 0, gameManager.width), Mathf.Clamp(targetY, 0, gameManager.height));
                }
                else
                {
                    targetResource = null;
                    currentState = WorkerUnitState.Idle;
                }

                break;

            case WorkerUnitState.Extracting:
                if (secondsToAttack <= 0)
                {
                    secondsToAttack = secondsPerAttack;
                    if (targetResource != null && targetResource.gameObject.activeInHierarchy && targetResource.Extract())
                    {
                        resourceManager.AddResource(team, 1);
                        if (resourceManager.GetResourceAmount(team) > 10)
                        {
                            currentState = WorkerUnitState.Idle;
                        }
                    } else
                    {
                        targetResource = null;
                        currentState = WorkerUnitState.Idle;
                    }
                }
                break;
        }
    }
}
