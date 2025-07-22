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

        foreach (var (gridPositionComponent, unitComponent, unitPathBuffer, transform, entity) in
                SystemAPI.Query<RefRW<GridPositionComponent>, RefRW<UnitComponent>,
                DynamicBuffer<UnitPathBuffer>, RefRW<LocalTransform>>().WithEntityAccess())
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

            /*
            if (unitPathBuffer.Length > 0)
            {
                float2 currentPosition = new float2(transform.ValueRW.Position.x, transform.ValueRW.Position.y);
                float2 actualPosition = MoveTowards(currentPosition, unitPathBuffer[0].position,
                        unitComponent.ValueRO.speed * SystemAPI.Time.DeltaTime);
                transform.ValueRW.Position = new float3(actualPosition.x, actualPosition.y, 0);

                if (math.distance(actualPosition, unitPathBuffer[0].position) < 0.1f)
                {
                    if (GeneralUtils.IsWalkable(unitPathBuffer[0].position, gameManager, SystemAPI.GetBuffer<OccupationCellBuffer>(gameManagerEntity)))
                    {
                        gridPositionComponent.ValueRW.position = unitPathBuffer[0].position;

                        // Check for traps
                        //foreach (var (trapGridObject, trap, entity) in SystemAPI.Query<RefRO<GridEntity>, RefRW<ECSTrap>>().WithEntityAccess())
                        //{
                        //    if (gridPositionComponent.ValueRW.position == new int2(trapGridObject.ValueRO.x, trapGridObject.ValueRO.y))
                        //    {
                        //        if (trap.ValueRW.counter <= 0)
                        //            continue;

                        //        if (--trap.ValueRW.counter <= 0)
                        //        {
                        //            ecb.DestroyEntity(entity);
                        //        }
                        //        break;
                        //    }
                        //}

                        unitPathBuffer.RemoveAt(0);
                    } else
                    {
                        transform.ValueRW.Position =
                                new float3(gridPositionComponent.ValueRW.position.x, gridPositionComponent.ValueRW.position.y, 0);
                        unitPathBuffer.Clear();
                        var job = new ECSAStarPathfinder
                        {
                            start = gridPositionComponent.ValueRW.position,
                            goal = (int2)unitComponent.ValueRO.targetPosition,
                            gridSize = new int2(gameManager.width, gameManager.height),
                            occupationCells = SystemAPI.GetBuffer<OccupationCellBuffer>(gameManagerEntity),
                            pathBuffer = unitPathBuffer,
                        };
                        job.Execute();
                    }
                }
            }
            */
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
