using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ECSGameManagerAuthoring : MonoBehaviour
{
    public int width;
    public int height;

    public int unitCount;

    public GameObject unitPrefab;
    public GameObject wandererPrefab;
    public GameObject lightPrefab;
    public GameObject heavyPrefab;
    public GameObject rangePrefab;
    public GameObject healerPrefab;
    public GameObject trapperPrefab;
    public GameObject workerPrefab;
    public GameObject obstaclePrefab;
    public GameObject trapPrefab;

    private void Start()
    {

    }

    public class Baker : Baker<ECSGameManagerAuthoring>
    {
        public override void Bake(ECSGameManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSGameManager
            {
                width = authoring.width,
                height = authoring.height,
                unitCount = authoring.unitCount,
                unitPrefab = GetEntity(authoring.unitPrefab, TransformUsageFlags.Dynamic),
                wandererPrefab = GetEntity(authoring.wandererPrefab, TransformUsageFlags.Dynamic),
                lightPrefab = GetEntity(authoring.lightPrefab, TransformUsageFlags.Dynamic),
                heavyPrefab = GetEntity(authoring.heavyPrefab, TransformUsageFlags.Dynamic),
                rangePrefab = GetEntity(authoring.rangePrefab, TransformUsageFlags.Dynamic),
                healerPrefab = GetEntity(authoring.healerPrefab, TransformUsageFlags.Dynamic),
                trapperPrefab = GetEntity(authoring.trapperPrefab, TransformUsageFlags.Dynamic),
                workerPrefab = GetEntity(authoring.workerPrefab, TransformUsageFlags.Dynamic),
                obstaclePrefab = GetEntity(authoring.obstaclePrefab, TransformUsageFlags.Dynamic),
                trapPrefab = GetEntity(authoring.trapPrefab, TransformUsageFlags.Dynamic)
            });
            var buffer = AddBuffer<OccupationCellBuffer>(entity);
            for (int i = 0; i < authoring.width * authoring.height; i++)
            {
                buffer.Add(new OccupationCellBuffer
                {
                    isOccupied = false
                });
            }
        }
    }

}