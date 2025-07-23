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
        if (!hasInitialized && SystemAPI.TryGetSingleton<ECSGameManager>(out gameManager))
        {
            for (int i = 1; i < gameManager.width - 1; i += 2)
                AddObstacle(ref state, i, i);
            for (int i = 1; i < gameManager.width - 1; i += 2)
                AddObstacle(ref state, i, gameManager.height - i);

            for (int team = 1; team <= 2; team++)
                for (int i = 0; i < gameManager.unitCount; i++)
                {
                    int x, y;

                    x = UnityEngine.Random.Range(0, gameManager.width);
                    y = UnityEngine.Random.Range(0, gameManager.height);
                    AddUnit(ref state, 5, team, x, y);

                    //x = UnityEngine.Random.Range(0, gameManager.width);
                    //y = UnityEngine.Random.Range(0, gameManager.height);
                    //AddUnit(ref state, 3, team, x, y);
                }

            // Wanderer test
            //for (int team = 1; team <= 2; team++) {
            //    for (int i = 0; i < gameManager.unitCount; i++)
            //    {
            //        int x = UnityEngine.Random.Range(0, gameManager.width);
            //        int y = UnityEngine.Random.Range(0, gameManager.height);
            //        AddUnit(ref state, -1, team, x, y);
            //    }
            //}

            // Fighter test
            //for (int team = 1; team <= 2; team++)
            //{
            //    for (int i = 0; i < gameManager.unitCount; i++)
            //    {
            //        for (int id = 0; id < 3; id++)
            //        {
            //            int x = UnityEngine.Random.Range(0, gameManager.width);
            //            int y = UnityEngine.Random.Range(0, gameManager.height);
            //            AddUnit(ref state, id, team, x, y);
            //        }
            //    }
            //}

            // Healer test
            //for (int team = 1; team <= 2; team++)
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        int x = UnityEngine.Random.Range(0, gameManager.width);
            //        int y = UnityEngine.Random.Range(0, gameManager.height);
            //        AddUnit(ref state, 1, team, x, y);
            //    }

            //    for (int i = 0; i < 1; i++)
            //    {
            //        int x = UnityEngine.Random.Range(0, gameManager.width);
            //        int y = UnityEngine.Random.Range(0, gameManager.height);
            //        AddUnit(ref state, 4, team, x, y);
            //    }
            //}

            // Trapper test

            // Worker test

            // Moment of truth

            //for (int i = 10; i < gameManager.height - 10; i++)
            //    AddTrap(ref state, 5, i);

            hasInitialized = true;
        }
    }

    public Entity AddUnit(ref SystemState state, int id, int team, int x, int y)
    {
        if (state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>())
                .ElementAt(x + y * SystemAPI.GetSingleton<ECSGameManager>().width).isOccupied == true)
            return Entity.Null;

        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();

        Entity unit;

        switch (id)
        {
            default:
                unit = state.EntityManager.Instantiate(gameManager.unitPrefab);
                state.EntityManager.AddComponentData<TeamComponent>(unit, new TeamComponent
                {
                    teamId = team,
                });
                state.EntityManager.SetComponentData(unit, LocalTransform.FromPosition(x, y, 0));
                state.EntityManager.AddComponentData<GridPositionComponent>(unit, new GridPositionComponent
                { 
                    position = new int2(x, y)
                });
                state.EntityManager.AddComponentData<UnitComponent>(unit, new UnitComponent
                {
                    speed = 10f,
                    attackSpeed = 1f,
                    secondsToAttack = 1f,
                    targetPosition = null,
                    hasTriedFindPath = true,
                });
                state.EntityManager.AddBuffer<UnitPathBuffer>(unit);
                state.EntityManager.AddComponentData<HealthComponent>(unit, new HealthComponent
                {
                    health = 5,
                    maxHealth = 5,
                });
                HealthBarReference.CreateHealthBar(unit, 5);
                break;

            //case 0: // Wanderer
            //    break;

            case 1: // Light
                unit = state.EntityManager.Instantiate(gameManager.lightPrefab);
                state.EntityManager.AddComponentData<TeamComponent>(unit, new TeamComponent
                {
                    teamId = team,
                });
                state.EntityManager.SetComponentData(unit, LocalTransform.FromPosition(x, y, 0));
                state.EntityManager.AddComponentData<GridPositionComponent>(unit, new GridPositionComponent
                {
                    position = new int2(x, y)
                });
                state.EntityManager.AddComponentData<UnitComponent>(unit, new UnitComponent
                {
                    speed = 5f,
                    range = 2, damage = 1,
                    attackSpeed = 1f, secondsToAttack = 0f,
                    targetPosition = null, hasTriedFindPath = true,

                });
                PathRendererReference.CreatePathRenderer(unit);
                state.EntityManager.AddBuffer<UnitPathBuffer>(unit);
                state.EntityManager.AddComponentData<HealthComponent>(unit, new HealthComponent
                {
                    health = 5, maxHealth = 5,
                });
                HealthBarReference.CreateHealthBar(unit, 5);
                state.EntityManager.AddComponentData<FighterComponent>(unit, new FighterComponent
                {
                    currentState = FighterComponent.FighterState.Idle,
                    target = Entity.Null,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(0.75f, 1.25f),
                });
                break;
			
			case 2: // Heavy
                unit = state.EntityManager.Instantiate(gameManager.heavyPrefab);
                state.EntityManager.AddComponentData<TeamComponent>(unit, new TeamComponent
                {
                    teamId = team,
                });
                state.EntityManager.SetComponentData(unit, LocalTransform.FromPosition(x, y, 0));
                state.EntityManager.AddComponentData<GridPositionComponent>(unit, new GridPositionComponent
                {
                    position = new int2(x, y)
                });
                state.EntityManager.AddComponentData<UnitComponent>(unit, new UnitComponent
                {
                    speed = 3f,
                    range = 2, damage = 1,
                    attackSpeed = 1f, secondsToAttack = 0f,
                    targetPosition = null, hasTriedFindPath = true,

                });
                PathRendererReference.CreatePathRenderer(unit);
                state.EntityManager.AddBuffer<UnitPathBuffer>(unit);
                state.EntityManager.AddComponentData<HealthComponent>(unit, new HealthComponent
                {
                    health = 20, maxHealth = 20,
                });
                HealthBarReference.CreateHealthBar(unit, 5);
                state.EntityManager.AddComponentData<FighterComponent>(unit, new FighterComponent
                {
                    currentState = FighterComponent.FighterState.Idle,
                    target = Entity.Null,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(1.15f, 1.5f),
                });
                break;
				
			case 3: // Range
                unit = state.EntityManager.Instantiate(gameManager.rangePrefab);
                state.EntityManager.AddComponentData<TeamComponent>(unit, new TeamComponent
                {
                    teamId = team,
                });
                state.EntityManager.SetComponentData(unit, LocalTransform.FromPosition(x, y, 0));
                state.EntityManager.AddComponentData<GridPositionComponent>(unit, new GridPositionComponent
                {
                    position = new int2(x, y)
                });
                state.EntityManager.AddComponentData<UnitComponent>(unit, new UnitComponent
                {
                    speed = 5f,
                    range = 6, damage = 1,
                    attackSpeed = 1f, secondsToAttack = 0f,
                    targetPosition = null, hasTriedFindPath = true,

                });
                PathRendererReference.CreatePathRenderer(unit);
                state.EntityManager.AddBuffer<UnitPathBuffer>(unit);
                state.EntityManager.AddComponentData<HealthComponent>(unit, new HealthComponent
                {
                    health = 5, maxHealth = 5,
                });
                HealthBarReference.CreateHealthBar(unit, 5);
                state.EntityManager.AddComponentData<FighterComponent>(unit, new FighterComponent
                {
                    currentState = FighterComponent.FighterState.Idle,
                    target = Entity.Null,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(0.75f, 1.25f),
                });
                break;
				
			case 4: // Healer
                unit = state.EntityManager.Instantiate(gameManager.lightPrefab);
                state.EntityManager.AddComponentData<TeamComponent>(unit, new TeamComponent
                {
                    teamId = team,
                });
                state.EntityManager.SetComponentData(unit, LocalTransform.FromPosition(x, y, 0));
                state.EntityManager.AddComponentData<GridPositionComponent>(unit, new GridPositionComponent
                {
                    position = new int2(x, y)
                });
                state.EntityManager.AddComponentData<UnitComponent>(unit, new UnitComponent
                {
                    speed = 5f,
                    range = 5, damage = 1,
                    attackSpeed = .5f, secondsToAttack = 0f,
                    targetPosition = null, hasTriedFindPath = true,

                });
                PathRendererReference.CreatePathRenderer(unit);
                state.EntityManager.AddBuffer<UnitPathBuffer>(unit);
                state.EntityManager.AddComponentData<HealthComponent>(unit, new HealthComponent
                {
                    health = 5, maxHealth = 5,
                });
                HealthBarReference.CreateHealthBar(unit, 5);
                state.EntityManager.AddComponentData<HealerComponent>(unit, new HealerComponent
                {
                    currentState = HealerComponent.HealerState.Idle,
                    target = Entity.Null,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(0.75f, 1.25f),
                });
                break;
			
			case 5: // Trapper
                unit = state.EntityManager.Instantiate(gameManager.lightPrefab);
                state.EntityManager.AddComponentData<TeamComponent>(unit, new TeamComponent
                {
                    teamId = team,
                });
                state.EntityManager.SetComponentData(unit, LocalTransform.FromPosition(x, y, 0));
                state.EntityManager.AddComponentData<GridPositionComponent>(unit, new GridPositionComponent
                {
                    position = new int2(x, y)
                });
                state.EntityManager.AddComponentData<UnitComponent>(unit, new UnitComponent
                {
                    speed = 5f,
                    range = 1, damage = 1,
                    attackSpeed = 1f, secondsToAttack = 0f,
                    targetPosition = null, hasTriedFindPath = true,
                });
                PathRendererReference.CreatePathRenderer(unit);
                state.EntityManager.AddBuffer<UnitPathBuffer>(unit);
                state.EntityManager.AddComponentData<HealthComponent>(unit, new HealthComponent
                {
                    health = 5, maxHealth = 5,
                });
                HealthBarReference.CreateHealthBar(unit, 5);
                state.EntityManager.AddComponentData<TrapperComponent>(unit, new TrapperComponent
                {
                    currentState = TrapperComponent.TrapperState.Idle,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(0.75f, 1.25f),
                });
                break;

			case 6: // Worker
                unit = state.EntityManager.Instantiate(gameManager.lightPrefab);
                state.EntityManager.AddComponentData<TeamComponent>(unit, new TeamComponent
                {
                    teamId = team,
                });
                state.EntityManager.SetComponentData(unit, LocalTransform.FromPosition(x, y, 0));
                state.EntityManager.AddComponentData<GridPositionComponent>(unit, new GridPositionComponent
                {
                    position = new int2(x, y)
                });
                state.EntityManager.AddComponentData<UnitComponent>(unit, new UnitComponent
                {
                    speed = 5f,
                    range = 6, damage = 1,
                    attackSpeed = 1f, secondsToAttack = 0f,
                    targetPosition = null, hasTriedFindPath = true,
                });
                PathRendererReference.CreatePathRenderer(unit);
                state.EntityManager.AddBuffer<UnitPathBuffer>(unit);
                state.EntityManager.AddComponentData<HealthComponent>(unit, new HealthComponent
                {
                    health = 5, maxHealth = 5,
                });
                HealthBarReference.CreateHealthBar(unit, 5);
                state.EntityManager.AddComponentData<FighterComponent>(unit, new FighterComponent
                {
                    currentState = FighterComponent.FighterState.Idle,
                    target = Entity.Null,
                    secondsToFindNewTarget = 0f,
                    secondsPerFindNewTarget = UnityEngine.Random.Range(0.75f, 1.25f),
                });
                break;


        }

        return unit;
    }

    public Entity AddObstacle(ref SystemState state, int x, int y)
    {
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity obstacle = state.EntityManager.Instantiate(gameManager.obstaclePrefab);

        state.EntityManager.SetComponentData(obstacle, LocalTransform.FromPosition(x, y, 0));
        state.EntityManager.AddComponentData<GridPositionComponent>(obstacle, new GridPositionComponent
        {
            position = new int2(x, y),
        });

        state.EntityManager.AddComponentData<ObstacleComponent>(obstacle, new ObstacleComponent { });

        state.EntityManager.GetBuffer<OccupationCellBuffer>(SystemAPI.GetSingletonEntity<ECSGameManager>())
                .ElementAt(x + y * gameManager.width).isOccupied = true;

        return obstacle;
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
