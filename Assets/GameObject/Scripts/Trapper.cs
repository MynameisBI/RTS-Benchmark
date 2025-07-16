using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class Trapper : Unit
{
    public GameObject trapPrefab;

    private enum TrapperUnitState
    {
        Idle,
        Moving,
        SettingTrap,
    }
    private TrapperUnitState currentState;

    private new Vector2Int? currentTarget;

    public int maxDistanceFromPreviousTarget = 3;

    protected new void Awake()
    {
        base.Awake();
        currentState = TrapperUnitState.Idle;
    }

    protected new void Start()
    {
        base.Start();
        currentState = TrapperUnitState.Idle;
        InvokeRepeating("FindNewTarget", 0, Random.Range(0.75f, 1.25f));
    }

    protected new void Update()
    {
        base.Update();
        switch (currentState)
        {
            case TrapperUnitState.Idle:
                break;

            case TrapperUnitState.Moving:
                if (currentTarget == null)
                {
                    currentState = TrapperUnitState.Idle;
                    return;
                }

                if (currentPath.Count > 0)
                {
                    Vector2Int targetPosition = currentPath[0];
                    transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

                    // Arrive at new node
                    if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
                    {
                        bool[,] walkableMap = gameManager.GetObstaclesInBoolean();
                        // Check if new node is occupied while moving
                        if (walkableMap[targetPosition.x, targetPosition.y] == false)
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

                        if (currentTarget.HasValue)
                        {
                            if (Vector2.Distance(new Vector2(gridX, gridY), currentTarget.Value) <= range)
                            {
                                currentState = TrapperUnitState.SettingTrap;
                                secondsToAttack = secondsPerAttack;
                            }
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

                break;

            case TrapperUnitState.SettingTrap:
                if (currentTarget.HasValue)
                {
                    bool[,] walkableMap = gameManager.GetObstaclesInBoolean();
                    if (!walkableMap[currentTarget.Value.x, currentTarget.Value.y])
                    {
                        currentTarget = null;
                        currentState = TrapperUnitState.Idle;
                        return;
                    }
                }

                if (secondsToAttack <= 0)
                {
                    currentState = TrapperUnitState.Idle;
                    currentTarget = null;

                    Trap trap = Instantiate(trapPrefab, new Vector2(gridX, gridY), Quaternion.identity).GetComponent<Trap>();
                    trap.gridX = gridX;
                    trap.gridY = gridY;
                    trap.team = team;
                }
                break;
        }
    }

    public void FindNewTarget()
    {
        if (currentState == TrapperUnitState.SettingTrap || currentTarget != null)
            return;

        bool[,] walkableTiles = gameManager.GetObstaclesInBoolean();
        Vector2Int? target = null;
        for (int i = 0; i < 4; i++) {
            int x = Random.Range(Mathf.Max(0, gridX - maxDistanceFromPreviousTarget), Mathf.Min(gameManager.width, gridX + maxDistanceFromPreviousTarget));
            int y = Random.Range(Mathf.Max(0, gridY - maxDistanceFromPreviousTarget), Mathf.Min(gameManager.height, gridY + maxDistanceFromPreviousTarget));
            if (walkableTiles[x, y])
            {
                target = new Vector2Int(x, y);
                break;
            }
            else
            {
                target = null;
            }

        }

        if (target != null)
        {
            currentTarget = target.Value;
            Move(currentTarget.Value.x, currentTarget.Value.y);
            currentState = TrapperUnitState.Moving;
        }
    }
}
