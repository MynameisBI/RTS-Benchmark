using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct WorkerComponent : IComponentData
{
    public Entity targetResourceEntity;

    public enum WorkerUnitState
    {
        Idle,
        Moving,
        Building,
        Extracting,
    }
    public WorkerUnitState currentState;
}
