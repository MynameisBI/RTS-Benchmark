using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct GridFollowerSystem : ISystem
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

        foreach (var (gridObject, gridFollower, GridFollowerPathBuffer, transform) in SystemAPI.Query<RefRW<GridEntity>, RefRO<GridFollower>, DynamicBuffer<GridFollowerPathNode>, RefRW<LocalTransform>>())
        {
            ProcessGridFollower(ref state, gridObject, gridFollower, GridFollowerPathBuffer, transform);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void ProcessGridFollower(ref SystemState state, RefRW<GridEntity> gridObject, RefRO<GridFollower> gridFollower,
                DynamicBuffer<GridFollowerPathNode> gridFollowerPathBuffer, RefRW<LocalTransform> transform)
    {
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();

        while (gridFollowerPathBuffer.Length == 0)
        {
            
            int2 targetPosition = new int2(rng.NextInt(100), rng.NextInt(100));

            var job = new ECSAStarPathfinder
            {
                start = new int2(gridObject.ValueRW.x, gridObject.ValueRW.y),
                goal = targetPosition,
                gridSize = new int2(gameManager.width, gameManager.height),
                occupationCells = SystemAPI.GetBuffer<OccupationCell>(SystemAPI.GetSingletonEntity<ECSGameManager>()),
                pathBuffer = gridFollowerPathBuffer,
            };
            job.Execute();
        }

        if (gridFollowerPathBuffer.Length > 0)
        {
            float2 currentPosition = new float2(transform.ValueRW.Position.x, transform.ValueRW.Position.y);
            float2 actualPosition = MoveTowards(currentPosition, new float2(gridFollowerPathBuffer[0].x, gridFollowerPathBuffer[0].y),
                    gridFollower.ValueRO.speed * SystemAPI.Time.DeltaTime);
            transform.ValueRW.Position = new float3(actualPosition.x, actualPosition.y, 0);

            if (math.distance(actualPosition, new float2(gridFollowerPathBuffer[0].x, gridFollowerPathBuffer[0].y)) < 0.1f)
            {
                gridObject.ValueRW.x = gridFollowerPathBuffer[0].x;
                gridObject.ValueRW.y = gridFollowerPathBuffer[0].y;

                // Check for traps
                foreach (var (trapGridObject, trap, entity) in SystemAPI.Query<RefRO<GridEntity>, RefRW<ECSTrap>>().WithEntityAccess())
                {
                    if (gridObject.ValueRW.x == trapGridObject.ValueRO.x && gridObject.ValueRW.y == trapGridObject.ValueRO.y)
                    {
                        if (trap.ValueRW.counter <= 0)
                            continue;

                        if (--trap.ValueRW.counter <= 0)
                        {
                            ecb.DestroyEntity(entity);
                        }
                        break;
                    }
                }

                gridFollowerPathBuffer.RemoveAt(0);
            }
        }
    }

    public static float2 MoveTowards(float2 current, float2 target, float maxDelta)
    {
        float2 delta = target - current;
        float dist = math.length(delta);
        return dist <= maxDelta || dist == 0f
            ? target
            : current + delta / dist * maxDelta;
    }
}
