using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;

public partial struct WandererSystem : ISystem
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

        foreach (var (gridPositionComponent, unitComponent, unitPathBuffer, wandererComponent, transform) in 
                SystemAPI.Query<RefRW<GridPositionComponent>, RefRW<UnitComponent>, DynamicBuffer<UnitPathBuffer>,
                RefRW<WandererComponent>, RefRW<LocalTransform>>())
        {
            if (unitPathBuffer.Length > 0)
            {
                float2 currentPosition = new float2(transform.ValueRW.Position.x, transform.ValueRW.Position.y);
                float2 actualPosition = GeneralUtils.MoveTowards(currentPosition, unitPathBuffer[0].position,
                        unitComponent.ValueRO.speed * SystemAPI.Time.DeltaTime);
                transform.ValueRW.Position = new float3(actualPosition.x, actualPosition.y, 0);

                // Arrive at new node
                if (math.distance(actualPosition, unitPathBuffer[0].position) < 0.1f)
                {
                    // Check if new node is occupied while moving
                    if (!GeneralUtils.IsWalkable(unitPathBuffer[0].position, gameManager, SystemAPI.GetBuffer<OccupationCellBuffer>(gameManagerEntity)))
                    {
                        transform.ValueRW.Position =
                                new float3(gridPositionComponent.ValueRW.position.x, gridPositionComponent.ValueRW.position.y, 0);
                        unitComponent.ValueRW.hasTriedFindPath = false;
                    }
                    else
                    {
                        gridPositionComponent.ValueRW.position = unitPathBuffer[0].position;
                        unitPathBuffer.RemoveAt(0);
                    }
                }
            }
            else if (unitPathBuffer.Length == 0)
            {
                int2 randomPosition = new int2(rng.NextInt(0, gameManager.width), rng.NextInt(0, gameManager.height));
                if (GeneralUtils.IsWalkable(randomPosition, gameManager, SystemAPI.GetBuffer<OccupationCellBuffer>(gameManagerEntity)))
                {
                    unitComponent.ValueRW.targetPosition = randomPosition;
                    unitComponent.ValueRW.hasTriedFindPath = false;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

}
