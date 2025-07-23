using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct TrapComponent : IComponentData
{
    public TrapperComponent trapper;
    public int damage;
    public int counter;
}
