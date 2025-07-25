using Unity.Mathematics;
using Unity.Entities;

public class GeneralUtils
{
    public static bool IsWalkable(int2 pos, ECSGameManager gameManager, DynamicBuffer<OccupationCellBuffer> occupationCells)
            => !occupationCells[pos.x + pos.y * gameManager.width].isOccupied;

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

    public static DamageResult Damage(int damage, HealthComponent healthComponent)
    {
        if (healthComponent.health <= 0)
        {
            return DamageResult.Failed;
        }

        healthComponent.health -= damage;
        if (healthComponent.health <= 0)
        {
            healthComponent.health = 0;
            return DamageResult.SuccessAndKilled;
        }
        return DamageResult.Success;
    }

    
}
