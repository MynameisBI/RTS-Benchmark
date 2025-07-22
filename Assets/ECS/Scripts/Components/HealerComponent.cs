using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct HealerComponent : IComponentData
{
    public float secondsToFindNewTarget;
    public float secondsPerFindNewTarget;
    public Entity target;

    public enum HealerState
    {
        Idle,
        Moving,
        Healing
    }
    public HealerState currentState;
}
