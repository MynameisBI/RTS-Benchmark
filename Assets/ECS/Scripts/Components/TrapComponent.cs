using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct TrapComponent : IComponentData
{
    public UnitComponent trapper;
    public int counter;
}
