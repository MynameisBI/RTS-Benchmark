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
    public Entity wandererPrefab;
    public Entity lightPrefab;
    public Entity heavyPrefab;
    public Entity rangePrefab;
    public Entity healerPrefab;
    public Entity trapperPrefab;
    public Entity workerPrefab;
    public Entity wallPrefab;
    public Entity resourcePrefab;
    public Entity buildingPrefab;
    public Entity trapPrefab;
}

public struct OccupationCellBuffer : IBufferElementData
{
    public bool isOccupied;
}

public struct UnitDataBuffer : IBufferElementData
{
    public int id;
    public int x;
    public int y;
    public int team;
}

