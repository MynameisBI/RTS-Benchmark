using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static GeneralUtils;
using System.Collections.Generic;
using UnityEngine;

[BurstCompile]
public partial struct UnitSystem : ISystem
{
    Unity.Mathematics.Random rng;
    EntityCommandBuffer ecb;

    public void OnCreate(ref SystemState state)
    {
        rng = new Unity.Mathematics.Random(43);
        rng.NextInt();
    }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        ecb = new EntityCommandBuffer(Allocator.TempJob);
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity gameManagerEntity = SystemAPI.GetSingletonEntity<ECSGameManager>();
        List<(RefRW<GridPositionComponent>, RefRW<UnitComponent>, DynamicBuffer<UnitPathBuffer>)> toBeFindPathEntities =
                    new List<(RefRW<GridPositionComponent>, RefRW<UnitComponent>, DynamicBuffer<UnitPathBuffer>)>();

        foreach (var (gridPositionComponent, unitComponent, unitPathBuffer, teamComponent, transform, healthComponent, entity) in
                SystemAPI.Query<RefRW<GridPositionComponent>, RefRW<UnitComponent>,
                DynamicBuffer<UnitPathBuffer>, RefRO<TeamComponent>, RefRW<LocalTransform>, RefRW<HealthComponent>>().WithEntityAccess())
        {
            unitComponent.ValueRW.secondsToAttack -= SystemAPI.Time.DeltaTime;

            if (unitComponent.ValueRO.targetPosition != null && !unitComponent.ValueRO.hasTriedFindPath)
            {
                toBeFindPathEntities.Add((gridPositionComponent, unitComponent, unitPathBuffer));

                unitPathBuffer.Clear();
                var job = new ECSAStarPathfinder
                {
                    start = gridPositionComponent.ValueRW.position,
                    goal = (int2)unitComponent.ValueRO.targetPosition,
                    gridSize = new int2(gameManager.width, gameManager.height),
                    occupationCells = state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>()),
                    pathBuffer = unitPathBuffer,
                };
                if (job.Execute())
                    unitComponent.ValueRW.hasRerenderedPath = false;

                unitComponent.ValueRW.hasTriedFindPath = true;
            }

            if (!unitComponent.ValueRO.lastFrameGridPosition.Equals(gridPositionComponent.ValueRO.position))
            {
                foreach (var (trapGridPositionComponent, trapComponent, trapTeamComponent, trapEntity) in
                        SystemAPI.Query<RefRO<GridPositionComponent>, RefRW<TrapComponent>, RefRO<TeamComponent>>().WithEntityAccess())
                {
                    if (teamComponent.ValueRO.teamId != trapTeamComponent.ValueRO.teamId &&
                        gridPositionComponent.ValueRW.position.Equals(trapGridPositionComponent.ValueRO.position))
                    {
                        if (trapComponent.ValueRO.counter > 0)
                            if (--trapComponent.ValueRW.counter <= 0)
                                ecb.DestroyEntity(trapEntity);

                        DamageResult result = Damage(trapComponent.ValueRO.trapper.damage, healthComponent);
                        if (result == DamageResult.SuccessAndKilled)
                            ecb.DestroyEntity(entity);

                        break;
                    }
                }
            }
            unitComponent.ValueRW.lastFrameGridPosition = gridPositionComponent.ValueRO.position;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        foreach (var (gridPositionComponent, unitComponent, unitPathBuffer) in toBeFindPathEntities)
        {
            //unitPathBuffer.Clear();

            //bool[,] walkableMap = new bool[gameManager.width, gameManager.height];
            //for (int i = 0; i < gameManager.width; i++)
            //    for (int j = 0; j < gameManager.height; j++)
            //        walkableMap[i, j] = IsWalkable(new int2(i, j), gameManager, state.EntityManager.GetBuffer<OccupationCellBuffer>(gameManagerEntity));

            //AStarPathfinder pathfinder = new AStarPathfinder(walkableMap);
            //List<Vector2Int> currentPath = pathfinder.FindPath(
            //    new Vector2Int(gridPositionComponent.ValueRO.position.x, gridPositionComponent.ValueRO.position.x),
            //    new Vector2Int(unitComponent.ValueRO.targetPosition.Value.x, unitComponent.ValueRO.targetPosition.Value.y));
            //if (currentPath == null)
            //    currentPath = new List<Vector2Int>();

            //foreach (Vector2Int node in currentPath)
            //{
            //    unitPathBuffer.Add(new UnitPathBuffer
            //    {
            //        position = new int2(node.x, node.y)
            //    });
            //}

            //unitComponent.ValueRW.hasRerenderedPath = false;

            //var job = new ECSAStarPathfinder
            //{
            //    start = gridPositionComponent.ValueRW.position,
            //    goal = (int2)unitComponent.ValueRO.targetPosition,
            //    gridSize = new int2(gameManager.width, gameManager.height),
            //    occupationCells = state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>()),
            //    pathBuffer = unitPathBuffer,
            //};
            //if (job.Execute())
            //    unitComponent.ValueRW.hasRerenderedPath = false;
        }
    }
}
