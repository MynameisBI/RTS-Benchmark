using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ECSGameManagerSystem : ISystem
{
    private bool hasInitialized;

    public void OnCreate(ref SystemState state)
    {
        hasInitialized = false;
    }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ECSGameManager gameManager;
        if (!hasInitialized && SystemAPI.TryGetSingleton<ECSGameManager>(out gameManager))
        {
            for (int i = 0; i < gameManager.unitCount; i++)
            {
                int x = Random.Range(0, gameManager.width);
                int y = Random.Range(0, gameManager.height);
                AddUnit(ref state, x, y);
            }

            for (int i = 1; i < gameManager.width - 1; i += 2)
                AddObstacle(ref state, i, i);
            for (int i = 1; i < gameManager.width - 1; i += 2)
                AddObstacle(ref state, i, gameManager.height - i);

            for (int i = 10; i < gameManager.height - 10; i++)
                AddTrap(ref state, 5, i);
            
            hasInitialized = true;
        }
    }

    public Entity AddUnit(ref SystemState state, int x, int y)
    {
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();

        Entity unit = state.EntityManager.Instantiate(gameManager.unitPrefab);

        state.EntityManager.SetComponentData(unit, LocalTransform.FromPosition(x, y, 0));
        state.EntityManager.AddComponentData<GridEntity>(unit, new GridEntity
        {
            x = x,
            y = y,
            isObstacle = false
        });
        state.EntityManager.AddComponentData<GridFollower>(unit, new GridFollower
        {
            speed = 10f,
        });
        state.EntityManager.AddBuffer<GridFollowerPathNode>(unit);

        return unit;
    }

    public Entity AddObstacle(ref SystemState state, int x, int y)
    {
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity obstacle = state.EntityManager.Instantiate(gameManager.obstaclePrefab);

        state.EntityManager.SetComponentData(obstacle, LocalTransform.FromPosition(x, y, 0));
        state.EntityManager.AddComponentData<GridEntity>(obstacle, new GridEntity
        {
            x = x,
            y = y,
            isObstacle = true
        });

        state.EntityManager.GetBuffer<OccupationCell>(SystemAPI.GetSingletonEntity<ECSGameManager>())
                .ElementAt(x + y * gameManager.width).isOccupied = 1;

        return obstacle;
    }

    public Entity AddTrap(ref SystemState state, int x, int y)
    {
        ECSGameManager gameManager = SystemAPI.GetSingleton<ECSGameManager>();
        Entity trap = state.EntityManager.Instantiate(gameManager.trapPrefab);

        state.EntityManager.SetComponentData(trap, LocalTransform.FromPosition(x, y, 0));
        state.EntityManager.AddComponentData<GridEntity>(trap, new GridEntity
        {
            x = x,
            y = y,
            isObstacle = false
        });
        state.EntityManager.AddComponentData<ECSTrap>(trap, new ECSTrap
        {
            damage = 1,
            counter = 1,
        });

        return trap;
    }
}
