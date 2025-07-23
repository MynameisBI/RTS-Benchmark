using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using TMPro;
using UnityEngine;

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

        foreach (var (gridPositionComponent, unitComponent, unitPathBuffer, teamComponent, transform, entity) in
                SystemAPI.Query<RefRW<GridPositionComponent>, RefRW<UnitComponent>,
                DynamicBuffer<UnitPathBuffer>, RefRO<TeamComponent>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            unitComponent.ValueRW.secondsToAttack -= SystemAPI.Time.DeltaTime;

            if (unitComponent.ValueRO.targetPosition != null && !unitComponent.ValueRO.hasTriedFindPath)
            {
                unitPathBuffer.Clear();
                var job = new ECSAStarPathfinder
                {
                    start = gridPositionComponent.ValueRW.position,
                    goal = (int2)unitComponent.ValueRO.targetPosition,
                    gridSize = new int2(gameManager.width, gameManager.height),
                    occupationCells = SystemAPI.GetBuffer<OccupationCellBuffer>(gameManagerEntity),
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
                        Debug.Log($"Unit {entity.Index} stepped on a trap at position {trapGridPositionComponent.ValueRO.position}");

                        if (trapComponent.ValueRO.counter <= 0)
                            continue;

                        if (--trapComponent.ValueRW.counter <= 0)
                            ecb.DestroyEntity(trapEntity);

                        break;
                    }
                }
            }
            unitComponent.ValueRW.lastFrameGridPosition = gridPositionComponent.ValueRO.position;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
