using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ECSResourceManager : IComponentData
{
    public int teamNum;
}

public struct TeamResourceBuffer : IBufferElementData
{
    public int amount;
}
