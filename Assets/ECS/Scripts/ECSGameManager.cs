using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ECSGameManager : IComponentData
{
    public int width;
    public int height;

    public int unitCount;

    public Entity unitPrefab;
    public Entity obstaclePrefab;
    public Entity trapPrefab;
}

public struct OccupationCell : IBufferElementData
{
    public byte isOccupied;
}

