using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Fighter : Unit
{
    private enum FighterUnitState
    {
        Idle,
        Moving,
        Attacking
    }
    private FighterUnitState currentState;

    protected new void Awake()
    {
        base.Awake();
    }

    protected new void Start()
    {
        base.Start();
        currentState = FighterUnitState.Idle;
        InvokeRepeating("FindNewTarget", 0, Random.Range(0.75f, 1.25f));
    }

    protected new void Update()
    {
        base.Update();

        switch (currentState)
        {
            case FighterUnitState.Idle:
                break;

            case FighterUnitState.Moving:
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

                        if (currentTarget.IsDestroyed())
                        {
                            currentTarget = null;
                            currentState = FighterUnitState.Idle;
                        } else if (currentTarget != null && Vector2.Distance(new Vector2(gridX, gridY), (Vector2)currentTarget.transform.position) <= range)
                        {
                            currentState = FighterUnitState.Attacking;
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

                //while (currentPath.Count == 0 && currentTarget == null)
                //    Move(Random.Range(0, gameManager.width), Random.Range(0, gameManager.height));

                break;

            case FighterUnitState.Attacking:
                if (currentTarget == null || currentTarget.IsDestroyed())
                {
                    currentTarget = null;
                    currentState = FighterUnitState.Idle;
                    return;
                }
                if (secondsToAttack < 0)
                { 
                    secondsToAttack = secondsPerAttack;
                    // Side effect moment
                    Attack(currentTarget, damage);

                    if (Vector2.Distance(new Vector2(gridX, gridY), (Vector2)currentTarget.transform.position) > range || currentTarget == null || currentTarget.IsDestroyed())
                    {
                        currentTarget = null;
                        currentState = FighterUnitState.Idle;
                    }
                }
                break;
        }
    }

    public void FindNewTarget()
    {
        if (currentState == FighterUnitState.Attacking)
            return;

        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || Vector2.Distance(transform.position, currentTarget.transform.position) > range)
        {
            TeamObject[] units = FindObjectsOfType<TeamObject>();

            TeamObject target = null;
            float closestDistance = float.MaxValue;
            foreach (TeamObject unit in units)
            {
                if (unit.team != team)
                {
                    float distance = Vector2.Distance(transform.position, unit.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        target = unit;
                    }
                }
            }

            if (target != null)
            {
                currentTarget = target;
                List<Vector2Int> adjacentTiles = gameManager.GetAdjacentWalkableTiles(currentTarget.gridX, currentTarget.gridY);
                Vector2Int targetPosition = adjacentTiles[Random.Range(0, adjacentTiles.Count)];
                Move(targetPosition.x, targetPosition.y);
                //Debug.Log("New path to target found: " + (currentTarget != null ? currentTarget.gameObject.name : "None") + ". Path count: " + currentPath.Count);
                
                currentState = FighterUnitState.Moving;
            }
        }
    }
}
