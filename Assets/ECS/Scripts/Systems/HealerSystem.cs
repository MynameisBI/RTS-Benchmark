using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;

[BurstCompile]
public partial struct HealerSystem : ISystem
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

        foreach (var (gridPositionComponent, unitComponent, unitPathBuffer, healerComponent, teamComponent, transform, healerUnitEntity) in
                SystemAPI.Query<RefRW<GridPositionComponent>, RefRW<UnitComponent>, DynamicBuffer<UnitPathBuffer>,
                RefRW<HealerComponent>, RefRO<TeamComponent>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            healerComponent.ValueRW.secondsToFindNewTarget -= SystemAPI.Time.DeltaTime;

            if (healerComponent.ValueRW.secondsToFindNewTarget < 0f)
            {
                healerComponent.ValueRW.secondsToFindNewTarget = healerComponent.ValueRW.secondsPerFindNewTarget;

                if (healerComponent.ValueRO.currentState != HealerComponent.HealerState.Healing)
                {
                    Entity newTarget = FindNewTarget(ref state, healerComponent, unitComponent, gridPositionComponent, teamComponent, healerUnitEntity);

                    if (newTarget != Entity.Null)
                    {
                        healerComponent.ValueRW.target = newTarget;
                        healerComponent.ValueRW.currentState = HealerComponent.HealerState.Moving;

                        unitPathBuffer.Clear();
                        unitComponent.ValueRW.targetPosition = state.EntityManager.GetComponentData<GridPositionComponent>(newTarget).position;
                        unitComponent.ValueRW.hasTriedFindPath = false;
                    }
                }
            }

            switch (healerComponent.ValueRW.currentState)
            {
                case HealerComponent.HealerState.Idle:
                    break;

                case HealerComponent.HealerState.Moving:
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
                            else
                            {
                                gridPositionComponent.ValueRW.position = unitPathBuffer[0].position;
                                unitPathBuffer.RemoveAt(0);
                            }

                            HealthComponent targetHealthComponent = state.EntityManager.GetComponentData<HealthComponent>(healerComponent.ValueRO.target);
                            if (!state.EntityManager.Exists(healerComponent.ValueRO.target) || targetHealthComponent.health >= targetHealthComponent.maxHealth)
                            {
                                healerComponent.ValueRW.target = Entity.Null;
                                unitComponent.ValueRW.targetPosition = null;
                                healerComponent.ValueRW.currentState = HealerComponent.HealerState.Idle;
                            }
                            else
                            {
                                float2 targetPos = new float2(state.EntityManager.GetComponentData<LocalTransform>(healerComponent.ValueRO.target).Position.x,
                                        state.EntityManager.GetComponentData<LocalTransform>(healerComponent.ValueRO.target).Position.y);
                                if (healerComponent.ValueRO.target != Entity.Null &&
                                     math.distance(gridPositionComponent.ValueRW.position, targetPos) <= unitComponent.ValueRO.range)
                                {
                                    healerComponent.ValueRW.currentState = HealerComponent.HealerState.Healing;
                                }
                            }
                            
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

                        }
                    }

                    break;

                case HealerComponent.HealerState.Healing:
                    break;
            }

            if (unitComponent.ValueRW.secondsToAttack < 0)
            {
                if (healerComponent.ValueRW.target != Entity.Null)
                {
                    unitComponent.ValueRW.secondsToAttack = 1 / unitComponent.ValueRW.attackSpeed;

                    Entity target = healerComponent.ValueRW.target;
                    RefRW<HealthComponent> targetHealthComponent = SystemAPI.GetComponentRW<HealthComponent>(target);
                    targetHealthComponent.ValueRW.health = math.max(targetHealthComponent.ValueRW.health + unitComponent.ValueRW.damage,
                            targetHealthComponent.ValueRO.maxHealth);
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private Entity FindNewTarget(ref SystemState state, RefRW<HealerComponent> healerComponent,
        RefRW<UnitComponent> unitComponent, RefRW<GridPositionComponent> gridPositionComponent, RefRO<TeamComponent> teamComponent, Entity healerEntity)
    {
        if (healerComponent.ValueRW.currentState == HealerComponent.HealerState.Healing)
            return Entity.Null;

        Entity currentTarget = healerComponent.ValueRW.target;
        if (currentTarget == Entity.Null || !state.EntityManager.Exists(currentTarget) ||
                math.distance(gridPositionComponent.ValueRW.position,
                        state.EntityManager.GetComponentData<GridPositionComponent>(currentTarget).position) > unitComponent.ValueRW.range)
        {
            float minDistance = float.MaxValue;
            Entity closestEnemy = Entity.Null;
            foreach (var (otherGridPosition, otherUnit, otherHealth, otherTeam, otherEntity) in
                    SystemAPI.Query<RefRO<GridPositionComponent>, RefRW<UnitComponent>, RefRO<HealthComponent>, RefRO<TeamComponent>>().WithEntityAccess())
            {
                if (healerEntity != otherEntity &&
                        otherTeam.ValueRO.teamId == teamComponent.ValueRO.teamId && otherHealth.ValueRO.health < otherHealth.ValueRO.maxHealth)
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
