using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct FighterComponent : IComponentData
{
    public float secondsToFindNewTarget;
    public float secondsPerFindNewTarget;
    public Entity target;

    public enum FighterState
    {
        Idle,
        Moving,
        Attacking
    }
    public FighterState currentState;
}
