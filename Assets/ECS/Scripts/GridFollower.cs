using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct GridFollower : IComponentData
{
    public float speed;
}

public struct GridFollowerPathNode : IBufferElementData
{
    public int x;
    public int y;
}
