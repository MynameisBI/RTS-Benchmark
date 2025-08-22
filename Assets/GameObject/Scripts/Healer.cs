using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class Healer : Unit
{
    private enum HealerUnitState
    {
        Idle,
        Moving,
        Healing
    }
    private HealerUnitState currentState;

    protected new void Start()
    {
        base.Start();
        currentState = HealerUnitState.Idle;
        InvokeRepeating("FindNewTarget", 0, Random.Range(0.75f, 1.25f));
    }

    protected new void Update()
    {
        base.Update();
        switch (currentState)
        {
            case HealerUnitState.Idle:
                break;

            case HealerUnitState.Moving:
                while (currentPath.Count == 0)
                    Move(Random.Range(0, gameManager.width), Random.Range(0, gameManager.height));

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

                        if (currentTarget.IsDestroyed() || currentTarget.IsFullHealth())
                        {
                            currentTarget = null;
                            currentState = HealerUnitState.Idle;
                        }
                        else if (currentTarget != null && Vector2.Distance(new Vector2(gridX, gridY), (Vector2)currentTarget.transform.position) <= range)
                        {
                            currentState = HealerUnitState.Healing;
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

            case HealerUnitState.Healing:
                break;
        }
        if (secondsToAttack < 0)
        {
            if (currentTarget != null && Vector2.Distance(new Vector2(gridX, gridY), (Vector2)currentTarget.transform.position) <= range)
            {
                secondsToAttack = secondsPerAttack;
                Heal((TeamObject)currentTarget);
            }
        }
    }

    protected void Heal(TeamObject target)
    {
        target.ReceiveHeal(damage);
    }

    public void FindNewTarget()
    {
        if (currentState == HealerUnitState.Healing)
            return;

        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || Vector2.Distance(transform.position, currentTarget.transform.position) > range)
        {
            Unit[] units = FindObjectsOfType<Unit>();
            Unit target = null;
            float closestDistance = float.MaxValue;
            foreach (Unit unit in units)
            {
                if (unit != this && unit.team == team && !unit.IsFullHealth())
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
                Move(currentTarget.gridX, currentTarget.gridY);
                currentState = HealerUnitState.Moving;
            }
        }
    }
}
