using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;

public partial struct TrapperSystem : ISystem
{
    Unity.Mathematics.Random rng;
    EntityCommandBuffer ecb;
    int maxDistanceFromPreviousTarget;
    

    public void OnCreate(ref SystemState state)
    {
        rng = new Unity.Mathematics.Random(43);
        rng.NextInt();
        maxDistanceFromPreviousTarget = 6;
    }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        ecb = new EntityCommandBuffer(Allocator.TempJob);
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity gameManagerEntity = SystemAPI.GetSingletonEntity<ECSGameManager>();

        foreach (var (gridPositionComponent, unitComponent, unitPathBuffer, trapperComponent, teamComponent, transform) in
                SystemAPI.Query<RefRW<GridPositionComponent>, RefRW<UnitComponent>, DynamicBuffer<UnitPathBuffer>,
                RefRW<TrapperComponent>, RefRO<TeamComponent>, RefRW<LocalTransform>>())
        {
            trapperComponent.ValueRW.secondsToFindNewTarget -= SystemAPI.Time.DeltaTime;

            if (trapperComponent.ValueRW.secondsToFindNewTarget < 0f)
            {
                trapperComponent.ValueRW.secondsToFindNewTarget = trapperComponent.ValueRW.secondsPerFindNewTarget;

                if (trapperComponent.ValueRO.currentState != TrapperComponent.TrapperState.SettingTrap && unitComponent.ValueRO.targetPosition == null)
                {
                    int2? newTarget = FindNewTarget(ref state, trapperComponent, unitComponent, gridPositionComponent, teamComponent);

                    if (newTarget != null)
                    {
                        trapperComponent.ValueRW.currentState = TrapperComponent.TrapperState.Moving;

                        unitComponent.ValueRW.targetPosition = (int2)newTarget;
                        unitComponent.ValueRW.hasTriedFindPath = false;
                    }
                }
            }

            switch (trapperComponent.ValueRO.currentState)
            {
                case TrapperComponent.TrapperState.Idle:
                    break;

                case TrapperComponent.TrapperState.Moving:
                    if (unitComponent.ValueRO.targetPosition == null)
                    {
                        trapperComponent.ValueRW.currentState = TrapperComponent.TrapperState.Idle;
                        break;
                    }

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

                            if (unitComponent.ValueRO.targetPosition.HasValue &&
                                    math.distance((int2)unitComponent.ValueRO.targetPosition, gridPositionComponent.ValueRO.position) <= unitComponent.ValueRO.range)
                            {
                                trapperComponent.ValueRW.currentState = TrapperComponent.TrapperState.SettingTrap;
                                unitComponent.ValueRW.secondsToAttack = 1 / unitComponent.ValueRO.attackSpeed;
                            }
                        }
                    }

                    break;

                case TrapperComponent.TrapperState.SettingTrap:
                    if (unitComponent.ValueRO.targetPosition.HasValue)
                    {
                        if (!GeneralUtils.IsWalkable((int2)unitComponent.ValueRO.targetPosition, gameManager, SystemAPI.GetBuffer<OccupationCellBuffer>(gameManagerEntity)))
                        {
                            unitComponent.ValueRW.targetPosition = null;
                            trapperComponent.ValueRW.currentState = TrapperComponent.TrapperState.Idle;
                            break;
                        }
                    }

                    if (unitComponent.ValueRW.secondsToAttack <= 0)
                    {
                        unitComponent.ValueRW.targetPosition = null;
                        trapperComponent.ValueRW.currentState = TrapperComponent.TrapperState.Idle;

                        Entity trap = ecb.Instantiate(gameManager.trapPrefab);
                        ecb.AddComponent<TeamComponent>(trap, new TeamComponent
                        {
                            teamId = teamComponent.ValueRO.teamId,
                        });
                        ecb.SetComponent(trap, LocalTransform.FromPosition(gridPositionComponent.ValueRO.position.x, gridPositionComponent.ValueRO.position.y, 0));
                        ecb.AddComponent<GridPositionComponent>(trap, new GridPositionComponent
                        {
                            position = new int2(gridPositionComponent.ValueRO.position.x, gridPositionComponent.ValueRO.position.y)
                        });
                        ecb.AddComponent<TrapComponent>(trap, new TrapComponent
                        {
                            trapper = unitComponent.ValueRO,
                            counter = 1,
                        });
                    }
                    break;
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private int2? FindNewTarget(ref SystemState state, RefRW<TrapperComponent> trapperComponent,
        RefRW<UnitComponent> unitComponent, RefRW<GridPositionComponent> gridPositionComponent, RefRO<TeamComponent> teamComponent)
    {
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity gameManagerEntity = SystemAPI.GetSingletonEntity<ECSGameManager>();
        int2? target = null;
        for (int i = 0; i < 4; i++)
        {
            int x = UnityEngine.Random.Range(Mathf.Max(0, gridPositionComponent.ValueRW.position.x - maxDistanceFromPreviousTarget),
                    Mathf.Min(gameManager.width, gridPositionComponent.ValueRW.position.y + maxDistanceFromPreviousTarget));
            int y = UnityEngine.Random.Range(Mathf.Max(0, gridPositionComponent.ValueRW.position.y - maxDistanceFromPreviousTarget),
                    Mathf.Min(gameManager.height, gridPositionComponent.ValueRW.position.y + maxDistanceFromPreviousTarget));
            int2 potentialTarget = new int2(x, y);
            if (GeneralUtils.IsWalkable(potentialTarget, gameManager, SystemAPI.GetBuffer<OccupationCellBuffer>(gameManagerEntity)))
            {
                target = potentialTarget;
                break;
            }
        }
        return target;
    }
}
