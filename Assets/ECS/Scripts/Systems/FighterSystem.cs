using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;

[BurstCompile]
public partial struct FighterSystem : ISystem
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
            fighterComponent.ValueRW.secondsToFindNewTarget -= SystemAPI.Time.DeltaTime;

            if (fighterComponent.ValueRW.secondsToFindNewTarget < 0f)
            {
                fighterComponent.ValueRW.secondsToFindNewTarget = fighterComponent.ValueRW.secondsPerFindNewTarget;

                if (fighterComponent.ValueRO.currentState != FighterComponent.FighterState.Attacking)
                {
                    Entity newTarget = FindNewTarget(ref state, fighterComponent, unitComponent, gridPositionComponent, teamComponent);

                    if (newTarget != Entity.Null)
                    {
                        fighterComponent.ValueRW.target = newTarget;
                        fighterComponent.ValueRW.currentState = FighterComponent.FighterState.Moving;

                        unitPathBuffer.Clear();
                        unitComponent.ValueRW.targetPosition = state.EntityManager.GetComponentData<GridPositionComponent>(newTarget).position;
                        unitComponent.ValueRW.hasTriedFindPath = false;
                    }
                }
            }

            switch (fighterComponent.ValueRW.currentState)
            {
                case FighterComponent.FighterState.Idle:
                    break;

                case FighterComponent.FighterState.Moving:
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
                                unitPathBuffer.Clear();
                                unitComponent.ValueRW.hasTriedFindPath = false;
                            }
                            else
                            {
                                gridPositionComponent.ValueRW.position = unitPathBuffer[0].position;
                                unitPathBuffer.RemoveAt(0);
                            }

                            if (!state.EntityManager.Exists(fighterComponent.ValueRO.target) ||
                                    state.EntityManager.GetComponentData<HealthComponent>(fighterComponent.ValueRO.target).health <= 0)
                            {
                                fighterComponent.ValueRW.target = Entity.Null;
                                unitComponent.ValueRW.targetPosition = null;
                                fighterComponent.ValueRW.currentState = FighterComponent.FighterState.Idle;
                            }
                            else
                            {
                                float2 targetPos = new float2(state.EntityManager.GetComponentData<LocalTransform>(fighterComponent.ValueRO.target).Position.x,
                                        state.EntityManager.GetComponentData<LocalTransform>(fighterComponent.ValueRO.target).Position.y);
                                if (fighterComponent.ValueRO.target != Entity.Null &&
                                     math.distance(gridPositionComponent.ValueRW.position, targetPos) <= unitComponent.ValueRO.range)
                                {
                                    fighterComponent.ValueRW.currentState = FighterComponent.FighterState.Attacking;
                                }
                            }
                        }
                    }

                    break;

                case FighterComponent.FighterState.Attacking:
                    if (fighterComponent.ValueRW.target == Entity.Null || !state.EntityManager.Exists(fighterComponent.ValueRW.target))
                    {
                        fighterComponent.ValueRW.target = Entity.Null;
                        unitComponent.ValueRW.targetPosition = null;
                        fighterComponent.ValueRW.currentState = FighterComponent.FighterState.Idle;
                        break;
                    }

                    if (unitComponent.ValueRW.secondsToAttack <= 0)
                    {
                        unitComponent.ValueRW.secondsToAttack = 1 / unitComponent.ValueRW.attackSpeed;

                        // Side effect moment
                        RefRW<HealthComponent> hc = SystemAPI.GetComponentRW<HealthComponent>(fighterComponent.ValueRW.target);
                        hc.ValueRW.health -= unitComponent.ValueRO.damage;
                        if (hc.ValueRW.health <= 0)
                        {
                            ecb.DestroyEntity(fighterComponent.ValueRW.target);
                        }

                        if (hc.ValueRW.health <= 0 || 
                                math.distance(gridPositionComponent.ValueRO.position,
                                        SystemAPI.GetComponent<GridPositionComponent>(fighterComponent.ValueRW.target).position) > unitComponent.ValueRO.range)
                        {
                            fighterComponent.ValueRW.target = Entity.Null;
                            unitComponent.ValueRW.targetPosition = null;
                            fighterComponent.ValueRW.currentState = FighterComponent.FighterState.Idle;
                        }
                    }
                    break;
            }
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
