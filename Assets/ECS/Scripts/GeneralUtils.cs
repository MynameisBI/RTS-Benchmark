using Unity.Mathematics;
using Unity.Entities;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class GeneralUtils
{
    public static bool IsWalkable(int2 pos, ECSGameManager gameManager, DynamicBuffer<OccupationCellBuffer> occupationCells)
            => !occupationCells[pos.x + pos.y * gameManager.width].isOccupied;

    public static void GetAdjacentWalkableTiles(int2 pos, ECSGameManager gameManager, DynamicBuffer<OccupationCellBuffer> occupationCells,
            ref NativeList<int2> outTiles)
    {
        if (pos.x >= 1 && IsWalkable(new int2(pos.x - 1, pos.y), gameManager, occupationCells))
                outTiles.Add(new int2(pos.x - 1, pos.y));
        if (pos.x <= gameManager.width - 2 && IsWalkable(new int2(pos.x + 1, pos.y), gameManager, occupationCells))
                outTiles.Add(new int2(pos.x + 1, pos.y));
        if (pos.y >= 1 && IsWalkable(new int2(pos.x, pos.y - 1), gameManager, occupationCells))
                outTiles.Add(new int2(pos.x, pos.y - 1));
        if (pos.y <= gameManager.height - 2 && IsWalkable(new int2(pos.x, pos.y + 1), gameManager, occupationCells))
                outTiles.Add(new int2(pos.x, pos.y + 1));
    }

    public static float2 MoveTowards(float2 current, float2 target, float maxDelta)
    {
        float2 delta = target - current;
        float dist = math.length(delta);
        return dist <= maxDelta || dist == 0f
        ? target
            : current + delta / dist * maxDelta;
    }

    public enum DamageResult
    {
        Failed,
        Success,
        SuccessAndKilled
    }

    public static DamageResult Damage(int damage, RefRW<HealthComponent> healthComponent)
    {
        if (healthComponent.ValueRW.health <= 0)
        {
            return DamageResult.Failed;
        }

        healthComponent.ValueRW.health -= damage;
        if (healthComponent.ValueRW.health <= 0)
        {
            healthComponent.ValueRW.health = 0;
            return DamageResult.SuccessAndKilled;
        }
        return DamageResult.Success;
    }

    
}
