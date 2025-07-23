using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;
using static WorkerComponent;
using System.Resources;

[BurstCompile]
public partial struct WorkerSystem : ISystem
{
    Unity.Mathematics.Random rng;
    EntityCommandBuffer ecb;

    public void OnCreate(ref SystemState state)
    {
        rng = new Unity.Mathematics.Random(43);
        rng.NextInt();
    }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ecb = new EntityCommandBuffer(Allocator.TempJob);
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity gameManagerEntity = SystemAPI.GetSingletonEntity<ECSGameManager>();
        DynamicBuffer<TeamResourceBuffer> teamResourceBuffer = SystemAPI.GetSingletonBuffer<TeamResourceBuffer>();

        //foreach (var (gridPositionComponent, unitComponent, unitPathBuffer, workerComponent, teamComponent, transform) in
        //        SystemAPI.Query<RefRW<GridPositionComponent>, RefRW<UnitComponent>, DynamicBuffer<UnitPathBuffer>,
        //        RefRW<WorkerComponent>, RefRO<TeamComponent>, RefRW<LocalTransform>>())
        //{
        //    switch (workerComponent.ValueRO.currentState)
        //    {
        //        case WorkerUnitState.Idle:
        //            if (teamResourceBuffer[teamComponent.ValueRO.teamId].amount > 10)
        //            {
        //                workerComponent.ValueRW.targetResourceEntity = Entity.Null;
        //                workerComponent.ValueRW.currentState = WorkerUnitState.Building;
        //            }
        //            else
        //            {
        //                //foreach (var (trapGridPositionComponent, trapComponent, trapTeamComponent, trapEntity) in
        //                //SystemAPI.Query<RefRO<GridPositionComponent>, RefRW<TrapComponent>, RefRO<TeamComponent>>().WithEntityAccess())
        //                //{
        //                //    if (teamComponent.ValueRO.teamId != trapTeamComponent.ValueRO.teamId &&
        //                //        gridPositionComponent.ValueRW.position.Equals(trapGridPositionComponent.ValueRO.position))
        //                //    {
        //                //        if (trapComponent.ValueRO.counter <= 0)
        //                //            continue;

        //                //        if (--trapComponent.ValueRW.counter <= 0)
        //                //            ecb.DestroyEntity(trapEntity);

        //                //        break;
        //                //    }
        //                //}

        //                Resource[] resources = FindObjectsOfType<Resource>();
        //                if (resources.Length > 0)
        //                {
        //                    Resource target = null;
        //                    float closestDistance = float.MaxValue;
        //                    foreach (Resource resource in resources)
        //                    {
        //                        float distance = Vector2.Distance(transform.position, resource.transform.position);
        //                        if (distance < closestDistance)
        //                        {
        //                            closestDistance = distance;
        //                            target = resource;
        //                        }
        //                    }

        //                    if (target != null)
        //                    {
        //                        List<Vector2Int> targetAdjacentTiles = gameManager.GetAdjacentWalkableTiles(target.gridX, target.gridY);
        //                        if (targetAdjacentTiles.Count >= 1)
        //                        {
        //                            targetResource = target;
        //                            Vector2Int targetTile = targetAdjacentTiles[Random.Range(0, targetAdjacentTiles.Count)];
        //                            Move(targetTile.x, targetTile.y);
        //                            currentState = WorkerUnitState.Moving;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            break;

        //        case WorkerUnitState.Moving:
        //            if (currentPath.Count == 0)
        //                if (targetResource != null)
        //                    currentState = WorkerUnitState.Extracting;
        //                else
        //                    currentState = WorkerUnitState.Idle;

        //            else if (currentPath.Count > 0)
        //            {
        //                Vector2Int targetPosition = currentPath[0];
        //                transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        //                // Arrive at new node
        //                if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        //                {
        //                    // Check if new node is occupied while moving
        //                    if (!gameManager.GetObstaclesInBoolean(false)[targetPosition.x, targetPosition.y])
        //                    {
        //                        transform.position = new Vector2(gridX, gridY);
        //                        Move(targetPosition.x, targetPosition.y);
        //                    }
        //                    else
        //                    {
        //                        currentPath.RemoveAt(0);
        //                        gridX = targetPosition.x;
        //                        gridY = targetPosition.y;

        //                        if (targetResource != null && targetResource.IsDestroyed())
        //                        {
        //                            targetResource = null;
        //                            currentState = WorkerUnitState.Idle;
        //                        }

        //                        Trap[] traps = FindObjectsOfType<Trap>();
        //                        foreach (Trap trap in traps)
        //                        {
        //                            if (gridX == trap.gridX && gridY == trap.gridY && team != trap.team)
        //                            {

        //                                trap.OnHit(this);
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            break;

        //        case WorkerUnitState.Building:
        //            bool[,] walkableMap = gameManager.GetObstaclesInBoolean(false);

        //            List<Vector2Int> adjacentWalkableTiles = gameManager.GetAdjacentWalkableTiles(gridX, gridY);

        //            if (adjacentWalkableTiles.Count >= 2)
        //            {
        //                Vector2Int adjacentWalkableTile = adjacentWalkableTiles[Random.Range(0, adjacentWalkableTiles.Count)];
        //                if (gameManager.AddBuilding(adjacentWalkableTile.x, adjacentWalkableTile.y, team))
        //                {
        //                    resourceManager.AddResource(team, -10);
        //                    targetResource = null;
        //                    currentState = WorkerUnitState.Idle;
        //                }
        //            }
        //            else if (adjacentWalkableTiles.Count == 1)
        //            {
        //                targetResource = null;
        //                currentState = WorkerUnitState.Moving;
        //                int targetX, targetY;
        //                do { targetX = Random.Range(gridX - 6, gridX + 6); } while (targetX >= -2 && targetX <= 2);
        //                do { targetY = Random.Range(gridY - 6, gridY + 6); } while (targetY >= -2 && targetY <= 2);
        //                Move(Mathf.Clamp(targetX, 0, gameManager.width), Mathf.Clamp(targetY, 0, gameManager.height));
        //            }
        //            else
        //            {
        //                targetResource = null;
        //                currentState = WorkerUnitState.Idle;
        //            }

        //            break;

        //        case WorkerUnitState.Extracting:
        //            if (secondsToAttack <= 0)
        //            {
        //                secondsToAttack = secondsPerAttack;
        //                if (targetResource != null && targetResource.gameObject.activeInHierarchy && targetResource.Extract())
        //                {
        //                    resourceManager.AddResource(team, 1);
        //                    if (resourceManager.GetResourceAmount(team) > 10)
        //                    {
        //                        currentState = WorkerUnitState.Idle;
        //                    }
        //                }
        //                else
        //                {
        //                    targetResource = null;
        //                    currentState = WorkerUnitState.Idle;
        //                }
        //            }
        //            break;
        //    }
        //}

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
