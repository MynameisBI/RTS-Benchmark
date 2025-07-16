using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct ECSTrap : IComponentData
{
    public int damage;
    public int counter;
}
