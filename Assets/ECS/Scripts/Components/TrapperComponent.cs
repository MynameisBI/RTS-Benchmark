using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct TrapperComponent : IComponentData
{
    public float secondsToFindNewTarget;
    public float secondsPerFindNewTarget;

    public enum TrapperState
    {
        Idle,
        Moving,
        SettingTrap,
    }
    public TrapperState currentState;
}
