using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static WorkerComponent;


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

        foreach (var (gridPositionComponent, unitComponent, unitPathBuffer, workerComponent, teamComponent, transform) in
                SystemAPI.Query<RefRW<GridPositionComponent>, RefRW<UnitComponent>, DynamicBuffer<UnitPathBuffer>,
                RefRW<WorkerComponent>, RefRO<TeamComponent>, RefRW<LocalTransform>>())
        {
            switch (workerComponent.ValueRO.currentState)
            {
                case WorkerUnitState.Idle:
                    Debug.Log("1");
                    if (teamResourceBuffer[teamComponent.ValueRO.teamId - 1].amount > 10)
                    {
                        workerComponent.ValueRW.targetResourceEntity = Entity.Null;
                        workerComponent.ValueRW.currentState = WorkerUnitState.Building;
                    }
                    else
                    {
                        Entity target = Entity.Null;
                        float closestDistance = float.MaxValue;
                        foreach (var (resourceGridPositionComponent, resourceComponent, entity) in
                                SystemAPI.Query<RefRO<GridPositionComponent>, RefRW<ResourceComponent>>().WithEntityAccess())
                        {
                            float distance = math.distance(resourceGridPositionComponent.ValueRO.position,
                                    gridPositionComponent.ValueRO.position);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                target = entity;
                            }

                            if (target != Entity.Null)
                            {
                                NativeList<int2> targetAdjacentTiles = new NativeList<int2>(4, Allocator.Temp);
                                GeneralUtils.GetAdjacentWalkableTiles(resourceGridPositionComponent.ValueRO.position,
                                        gameManager, SystemAPI.GetSingletonBuffer<OccupationCellBuffer>(), ref targetAdjacentTiles);

                                if (targetAdjacentTiles.Length >= 1)
                                {
                                    workerComponent.ValueRW.targetResourceEntity = target;
                                    unitPathBuffer.Clear();
                                    unitComponent.ValueRW.targetPosition = targetAdjacentTiles[rng.NextInt(0, targetAdjacentTiles.Length)];
                                    unitComponent.ValueRW.hasTriedFindPath = false;
                                    workerComponent.ValueRW.currentState = WorkerUnitState.Moving;
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case WorkerUnitState.Moving:
                    Debug.Log("2");
                    if (unitPathBuffer.Length == 0)
                        if (workerComponent.ValueRO.targetResourceEntity != Entity.Null)
                            workerComponent.ValueRW.currentState = WorkerUnitState.Extracting;
                        else
                            workerComponent.ValueRW.currentState = WorkerUnitState.Idle;

                    else if (unitPathBuffer.Length > 0)
                    {
                        //Debug.Log("Z");
                        float2 currentPosition = new float2(transform.ValueRW.Position.x, transform.ValueRW.Position.y);
                        float2 actualPosition = GeneralUtils.MoveTowards(currentPosition, unitPathBuffer[0].position,
                                unitComponent.ValueRO.speed * SystemAPI.Time.DeltaTime);
                        transform.ValueRW.Position = new float3(actualPosition.x, actualPosition.y, 0);

                        // Arrive at new node
                        if (math.distance(actualPosition, unitPathBuffer[0].position) < 0.1f)
                        {
                            //Debug.Log("a");
                            // Check if new node is occupied while moving
                            if (!GeneralUtils.IsWalkable(unitPathBuffer[0].position, gameManager, SystemAPI.GetBuffer<OccupationCellBuffer>(gameManagerEntity)))
                            {
                                transform.ValueRW.Position =
                                        new float3(gridPositionComponent.ValueRW.position.x, gridPositionComponent.ValueRW.position.y, 0);
                                unitPathBuffer.Clear();
                                unitComponent.ValueRW.hasTriedFindPath = false;
                            }
                            else
                            {
                                gridPositionComponent.ValueRW.position = unitPathBuffer[0].position;
                                unitPathBuffer.RemoveAt(0);

                                if (workerComponent.ValueRO.targetResourceEntity != Entity.Null &&
                                        !state.EntityManager.Exists(workerComponent.ValueRW.targetResourceEntity))
                                {
                                    workerComponent.ValueRW.targetResourceEntity = Entity.Null;
                                    workerComponent.ValueRW.currentState = WorkerUnitState.Idle;
                                }
                            }
                        }
                    }
                    break;

                case WorkerUnitState.Building:
                    Debug.Log("3");
                    if (teamResourceBuffer.ElementAt(teamComponent.ValueRO.teamId - 1).amount < 10)
                    {
                        workerComponent.ValueRW.targetResourceEntity = Entity.Null;
                        workerComponent.ValueRW.currentState = WorkerUnitState.Idle;
                        break;
                    }

                    NativeList<int2> adjacentWalkableTiles = new NativeList<int2>(4, Allocator.Temp);
                    GeneralUtils.GetAdjacentWalkableTiles( gridPositionComponent.ValueRW.position,
                        gameManager, SystemAPI.GetSingletonBuffer<OccupationCellBuffer>(), ref adjacentWalkableTiles);

                    if (adjacentWalkableTiles.Length >= 2)
                    {
                        int2 adjacentWalkableTile = adjacentWalkableTiles[rng.NextInt(0, adjacentWalkableTiles.Length)];
                        if (GeneralUtils.IsWalkable(adjacentWalkableTile, gameManager, SystemAPI.GetSingletonBuffer<OccupationCellBuffer>()))
                        {
                            teamResourceBuffer.ElementAt(teamComponent.ValueRO.teamId - 1).amount -= 10;

                            // Add building
                            Entity building = ecb.Instantiate(gameManager.buildingPrefab);

                            ecb.SetComponent(building, LocalTransform.FromPosition(adjacentWalkableTile.x, adjacentWalkableTile.y, 0));
                            ecb.AddComponent<GridPositionComponent>(building, new GridPositionComponent
                            {
                                position = adjacentWalkableTile,
                            });
                            ecb.AddComponent<HealthComponent>(building, new HealthComponent
                            {
                                health = 25,
                                maxHealth = 25,
                            });
                            //HealthBarReference.CreateHealthBar(building, 25);
                            ecb.AddComponent<ObstacleComponent>(building, new ObstacleComponent { });
                            state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>())
                                    .ElementAt(adjacentWalkableTile.x + adjacentWalkableTile.y * gameManager.width).isOccupied = true;

                            int i = 0;
                            for (int x = 0; x < gameManager.width; x++)
                                for (int y = 0; y < gameManager.height; y++)
                                    i = state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>())
                                            .ElementAt(x + y * gameManager.width).isOccupied ? i+1 : i;
                            Debug.Log($"{i}");

                            workerComponent.ValueRW.targetResourceEntity = Entity.Null;
                            workerComponent.ValueRW.currentState = WorkerUnitState.Idle;
                        }
                    }
                    else if (adjacentWalkableTiles.Length == 1)
                    {
                        workerComponent.ValueRW.targetResourceEntity = Entity.Null;
                        workerComponent.ValueRW.currentState = WorkerUnitState.Moving;
                        int targetX, targetY;
                        int gridX = gridPositionComponent.ValueRW.position.x;
                        int gridY = gridPositionComponent.ValueRW.position.y;
                        do { targetX = rng.NextInt(gridX - 6, gridX + 6); } while (targetX >= -2 && targetX <= 2);
                        do { targetY = rng.NextInt(gridY - 6, gridY + 6); } while (targetY >= -2 && targetY <= 2);
                        unitComponent.ValueRW.targetPosition =
                            new int2(Mathf.Clamp(targetX, 0, gameManager.width), Mathf.Clamp(targetY, 0, gameManager.height));
                        unitComponent.ValueRW.hasTriedFindPath = false;
                    }
                    else
                    {
                        workerComponent.ValueRW.targetResourceEntity = Entity.Null;
                        workerComponent.ValueRW.currentState = WorkerUnitState.Idle;
                    }

                    adjacentWalkableTiles.Dispose();

                    break;

                case WorkerUnitState.Extracting:
                    //Debug.Log("4");
                    if (unitComponent.ValueRW.secondsToAttack <= 0)
                    {
                        unitComponent.ValueRW.secondsToAttack = 1 / unitComponent.ValueRO.attackSpeed;
                        
                        if (workerComponent.ValueRO.targetResourceEntity != Entity.Null &&
                            state.EntityManager.Exists(workerComponent.ValueRO.targetResourceEntity))
                        {
                            RefRW<ResourceComponent> resourceComponent =
                                SystemAPI.GetComponentRW<ResourceComponent>(workerComponent.ValueRO.targetResourceEntity);
                            if (resourceComponent.ValueRO.amount > 0)
                            {
                                resourceComponent.ValueRW.amount -= 1;
                                if (resourceComponent.ValueRO.amount == 0)
                                    ecb.DestroyEntity(workerComponent.ValueRO.targetResourceEntity);
                                
                                teamResourceBuffer.ElementAt(teamComponent.ValueRO.teamId - 1).amount += 1;
                            }
                        }
                        else
                        {
                            workerComponent.ValueRW.targetResourceEntity = Entity.Null;
                            workerComponent.ValueRW.currentState = WorkerUnitState.Idle;
                        }
                    }
                    break;
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
