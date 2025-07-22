using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;

[BurstCompile]
public partial struct TrapperSystem : ISystem
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

        foreach (var (gridPositionComponent, unitComponent, unitPathBuffer, fighterComponent, teamComponent, transform) in
                SystemAPI.Query<RefRW<GridPositionComponent>, RefRW<UnitComponent>, DynamicBuffer<UnitPathBuffer>,
                RefRW<FighterComponent>, RefRO<TeamComponent>, RefRW<LocalTransform>>())
        {
            
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private Entity FindNewTarget(ref SystemState state, RefRW<FighterComponent> fighterComponent,
        RefRW<UnitComponent> unitComponent, RefRW<GridPositionComponent> gridPositionComponent, RefRO<TeamComponent> teamComponent)
    {
        if (fighterComponent.ValueRW.currentState == FighterComponent.FighterState.Attacking)
            return Entity.Null;

        Entity currentTarget = fighterComponent.ValueRW.target;
        if (currentTarget == Entity.Null || !state.EntityManager.Exists(currentTarget) ||
                math.distance(gridPositionComponent.ValueRW.position,
                        state.EntityManager.GetComponentData<GridPositionComponent>(currentTarget).position) > unitComponent.ValueRW.range)
        {
            float minDistance = float.MaxValue;
            Entity closestEnemy = Entity.Null;
            foreach (var (otherGridPosition, otherUnit, otherHealth, otherTeam, otherEntity) in
                    SystemAPI.Query<RefRO<GridPositionComponent>, RefRW<UnitComponent>, RefRO<HealthComponent>, RefRO<TeamComponent>>().WithEntityAccess())
            {
                if (otherTeam.ValueRO.teamId != teamComponent.ValueRO.teamId && otherHealth.ValueRO.health > 0)
                {
                    float distance = math.distance(otherGridPosition.ValueRO.position, gridPositionComponent.ValueRO.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestEnemy = otherEntity;
                    }
                }
            }
            currentTarget = closestEnemy;
        }
        return currentTarget;
    }
}
