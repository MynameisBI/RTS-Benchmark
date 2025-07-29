using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

public partial struct ECSGameManagerSystem : ISystem
{
    private bool hasInitialized;

    public void OnCreate(ref SystemState state)
    {
        hasInitialized = false;
    }

    public void OnDestroy(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        ECSGameManager gameManager;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        if (!hasInitialized && SystemAPI.TryGetSingleton<ECSGameManager>(out gameManager))
        {
            DynamicBuffer<UnitDataBuffer> unitDataBuffer = SystemAPI.GetSingletonBuffer<UnitDataBuffer>();
            for (int i = 0; i < unitDataBuffer.Length; i++) {
                var unitData = unitDataBuffer[i];

                if (unitData.id < 0 || 9 < unitData.id)
                {
                    Debug.LogError($"Invalid unit id: {unitData.id}. Must be 0 to 9.");
                    continue; // Skip invalid unit
                }

                if (0 <= unitData.id && unitData.id <= 6)
                {
                    AddUnit(ref state, ref ecb, unitData.id, unitData.team, unitData.x, unitData.y);
                }

                switch (unitData.id)
                {
                    case 7: // Wall
                        AddWall(ref state, ref ecb, unitData.x, unitData.y);
                        break;
                    case 8: // Resource
                        AddResource(ref state, ref ecb, unitData.x, unitData.y);
                        break;
                    case 9: // Building
                        AddBuilding(ref state, ref ecb, unitData.team, unitData.x, unitData.y);
                        break;
                }
            }

            //for (int i = 1; i < gameManager.width - 1; i += 2)
            //    AddObstacle(ref state, i, i);
            //for (int i = 1; i < gameManager.width - 1; i += 2)
            //    AddObstacle(ref state, i, gameManager.height - i);

            //for (int i = 0; i < 2; i++)
            //{
            //    int x, y;
            //    x = UnityEngine.Random.Range(0, gameManager.width);
            //    y = UnityEngine.Random.Range(0, gameManager.height);
            //    AddResource(ref state, x, y);
            //}

            //for (int team = 1; team <= 1; team++)
            //    for (int i = 0; i < gameManager.unitCount; i++)
            //    {
            //        int x, y;

            //        x = UnityEngine.Random.Range(0, gameManager.width);
            //        y = UnityEngine.Random.Range(0, gameManager.height);
            //        AddUnit(ref state, 6, team, x, y);
            //    }

            //for (int i = 10; i < gameManager.height - 10; i++)
            //    AddTrap(ref state, 5, i);

            hasInitialized = true;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public Entity AddUnit(ref SystemState state, ref EntityCommandBuffer ecb, int id, int team, int x, int y)
    {
        if (state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>())
                .ElementAt(x + y * SystemAPI.GetSingleton<ECSGameManager>().width).isOccupied == true)
            return Entity.Null;

        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();

        Entity unit;

        switch (id)
        {
            default:
                unit = ecb.Instantiate(gameManager.wandererPrefab);
                ecb.AddComponent(unit, new TeamComponent { teamId = team });
                ecb.SetComponent(unit, LocalTransform.FromPosition(x, y, 0));
                ecb.AddComponent(unit, new GridPositionComponent { position = new int2(x, y) });
                ecb.AddComponent(unit, new UnitComponent
                {
                    speed = 5f,
                    attackSpeed = 1f,
                    secondsToAttack = 1f,
                    targetPosition = null,
                    hasTriedFindPath = true,
                });
                ecb.AddBuffer<UnitPathBuffer>(unit);
                ecb.AddComponent(unit, new HealthComponent { health = 5, maxHealth = 5 });
                ecb.AddComponent(unit, new WandererComponent { });
                break;

            case 1: // Light
                unit = ecb.Instantiate(gameManager.lightPrefab);
                ecb.AddComponent(unit, new TeamComponent { teamId = team });
                ecb.SetComponent(unit, LocalTransform.FromPosition(x, y, 0));
                ecb.AddComponent(unit, new GridPositionComponent { position = new int2(x, y) });
                ecb.AddComponent(unit, new UnitComponent
                {
                    speed = 10f,
                    range = 2,
                    damage = 1,
                    attackSpeed = 1f,
                    secondsToAttack = 0f,
                    targetPosition = null,
                    hasTriedFindPath = true,
                });
                ecb.AddBuffer<UnitPathBuffer>(unit);
                ecb.AddComponent(unit, new HealthComponent { health = 5, maxHealth = 5 });
                ecb.AddComponent(unit, new FighterComponent
                {
                    currentState = FighterComponent.FighterState.Idle,
                    target = Entity.Null,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(0.75f, 1.25f),
                });
                break;

            case 2: // Heavy
                unit = ecb.Instantiate(gameManager.heavyPrefab);
                ecb.AddComponent(unit, new TeamComponent { teamId = team });
                ecb.SetComponent(unit, LocalTransform.FromPosition(x, y, 0));
                ecb.AddComponent(unit, new GridPositionComponent { position = new int2(x, y) });
                ecb.AddComponent(unit, new UnitComponent
                {
                    speed = 3f,
                    range = 2,
                    damage = 1,
                    attackSpeed = 1f,
                    secondsToAttack = 0f,
                    targetPosition = null,
                    hasTriedFindPath = true,
                });
                ecb.AddBuffer<UnitPathBuffer>(unit);
                ecb.AddComponent(unit, new HealthComponent { health = 20, maxHealth = 20 });
                ecb.AddComponent(unit, new FighterComponent
                {
                    currentState = FighterComponent.FighterState.Idle,
                    target = Entity.Null,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(1.15f, 1.5f),
                });
                break;

            case 3: // Range
                unit = ecb.Instantiate(gameManager.rangePrefab);
                ecb.AddComponent(unit, new TeamComponent { teamId = team });
                ecb.SetComponent(unit, LocalTransform.FromPosition(x, y, 0));
                ecb.AddComponent(unit, new GridPositionComponent { position = new int2(x, y) });
                ecb.AddComponent(unit, new UnitComponent
                {
                    speed = 5f,
                    range = 6,
                    damage = 1,
                    attackSpeed = 1f,
                    secondsToAttack = 0f,
                    targetPosition = null,
                    hasTriedFindPath = true,
                });
                ecb.AddBuffer<UnitPathBuffer>(unit);
                ecb.AddComponent(unit, new HealthComponent { health = 5, maxHealth = 5 });
                ecb.AddComponent(unit, new FighterComponent
                {
                    currentState = FighterComponent.FighterState.Idle,
                    target = Entity.Null,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(0.75f, 1.25f),
                });
                break;

            case 4: // Trapper
                unit = ecb.Instantiate(gameManager.trapperPrefab);
                ecb.AddComponent(unit, new TeamComponent { teamId = team });
                ecb.SetComponent(unit, LocalTransform.FromPosition(x, y, 0));
                ecb.AddComponent(unit, new GridPositionComponent { position = new int2(x, y) });
                ecb.AddComponent(unit, new UnitComponent
                {
                    speed = 5f,
                    range = 1,
                    damage = 10,
                    attackSpeed = 1f,
                    secondsToAttack = 0f,
                    targetPosition = null,
                    hasTriedFindPath = true,
                });
                ecb.AddBuffer<UnitPathBuffer>(unit);
                ecb.AddComponent(unit, new HealthComponent { health = 5, maxHealth = 5 });
                ecb.AddComponent(unit, new TrapperComponent
                {
                    currentState = TrapperComponent.TrapperState.Idle,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(0.75f, 1.25f),
                });
                break;

            case 5: // Healer
                unit = ecb.Instantiate(gameManager.healerPrefab);
                ecb.AddComponent(unit, new TeamComponent { teamId = team });
                ecb.SetComponent(unit, LocalTransform.FromPosition(x, y, 0));
                ecb.AddComponent(unit, new GridPositionComponent { position = new int2(x, y) });
                ecb.AddComponent(unit, new UnitComponent
                {
                    speed = 5f,
                    range = 5,
                    damage = 1,
                    attackSpeed = 0.5f,
                    secondsToAttack = 0f,
                    targetPosition = null,
                    hasTriedFindPath = true,
                });
                ecb.AddBuffer<UnitPathBuffer>(unit);
                ecb.AddComponent(unit, new HealthComponent { health = 5, maxHealth = 5 });
                ecb.AddComponent(unit, new HealerComponent
                {
                    currentState = HealerComponent.HealerState.Idle,
                    target = Entity.Null,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(0.75f, 1.25f),
                });
                break;

            case 6: // Worker
                unit = ecb.Instantiate(gameManager.workerPrefab);
                ecb.AddComponent(unit, new TeamComponent { teamId = team });
                ecb.SetComponent(unit, LocalTransform.FromPosition(x, y, 0));
                ecb.AddComponent(unit, new GridPositionComponent { position = new int2(x, y) });
                ecb.AddComponent(unit, new UnitComponent
                {
                    speed = 5f,
                    range = 6,
                    damage = 1,
                    attackSpeed = 1f,
                    secondsToAttack = 0f,
                    targetPosition = null,
                    hasTriedFindPath = true,
                });
                ecb.AddBuffer<UnitPathBuffer>(unit);
                ecb.AddComponent(unit, new HealthComponent { health = 5, maxHealth = 5 });
                ecb.AddComponent(unit, new WorkerComponent
                {
                    currentState = WorkerComponent.WorkerUnitState.Idle,
                });
                break;
        }


        return unit;
    }

    public Entity AddWall(ref SystemState state, ref EntityCommandBuffer ecb, int x, int y)
    {
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity obstacle = ecb.Instantiate(gameManager.wallPrefab);

        ecb.SetComponent(obstacle, LocalTransform.FromPosition(x, y, 0));
        ecb.AddComponent<GridPositionComponent>(obstacle, new GridPositionComponent
        {
            position = new int2(x, y),
        });
        ecb.AddComponent<ObstacleComponent>(obstacle, new ObstacleComponent { });

        state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>())
                .ElementAt(x + y * gameManager.width).isOccupied = true;

        return obstacle;
    }

    public Entity AddResource(ref SystemState state, ref EntityCommandBuffer ecb, int x, int y)
    {
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity resource = ecb.Instantiate(gameManager.resourcePrefab);

        ecb.SetComponent(resource, LocalTransform.FromPosition(x, y, 0));
        ecb.AddComponent(resource, new GridPositionComponent
        {
            position = new int2(x, y),
        });
        ecb.AddComponent(resource, new ResourceComponent
        {
            amount = 10,
        });
        ecb.AddComponent(resource, new ObstacleComponent { });

        state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>())
                .ElementAt(x + y * gameManager.width).isOccupied = true;

        return resource;
    }

    public Entity AddBuilding(ref SystemState state, ref EntityCommandBuffer ecb, int team, int x, int y)
    {
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity building = ecb.Instantiate(gameManager.buildingPrefab);

        ecb.SetComponent(building, LocalTransform.FromPosition(x, y, 0));
        ecb.AddComponent(building, new TeamComponent
        {
            teamId = team,
        });
        ecb.AddComponent(building, new GridPositionComponent
        {
            position = new int2(x, y),
        });
        ecb.AddComponent(building, new HealthComponent
        {
            health = 25,
            maxHealth = 25,
        });
        ecb.AddComponent(building, new ObstacleComponent { });

        state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>())
                .ElementAt(x + y * gameManager.width).isOccupied = true;
        return building;
    }

    //public Entity AddTrap(ref SystemState state, int x, int y)
    //{
    //    ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
    //    Entity trap = state.EntityManager.Instantiate(gameManager.trapPrefab);

    //    state.EntityManager.SetComponentData(trap, LocalTransform.FromPosition(x, y, 0));
    //    state.EntityManager.AddComponentData<GridEntity>(trap, new GridEntity
    //    {
    //        x = x,
    //        y = y,
    //        isObstacle = false
    //    });
    //    state.EntityManager.AddComponentData<ECSTrap>(trap, new ECSTrap
    //    {
    //        damage = 1,
    //        counter = 1,
    //    });

    //    return trap;
    //}
}
