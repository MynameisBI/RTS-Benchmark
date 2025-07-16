using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct GridEntity: IComponentData
{
    public int x;
    public int y;
    public bool isObstacle;
}
