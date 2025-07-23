using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct UnitComponent : IComponentData
{
    public float speed;

    public int range;
    public int damage;

    public float attackSpeed;
    public float secondsToAttack;

    public int2? targetPosition;
    public bool hasTriedFindPath;
    public bool hasRerenderedPath;

    public int2 lastFrameGridPosition;
}

public struct UnitPathBuffer : IBufferElementData
{
    public int2 position;
}
