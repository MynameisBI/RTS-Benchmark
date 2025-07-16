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
                obstaclePrefab = GetEntity(authoring.obstaclePrefab, TransformUsageFlags.Dynamic),
                trapPrefab = GetEntity(authoring.trapPrefab, TransformUsageFlags.Dynamic)
            });
            var buffer = AddBuffer<OccupationCell>(entity);
            for (int i = 0; i < authoring.width * authoring.height; i++)
            {
                buffer.Add(new OccupationCell
                {
                    isOccupied = 0
                });
            }
        }
    }

}