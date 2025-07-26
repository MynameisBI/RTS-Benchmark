using UnityEngine;
using Unity.Entities;
using System.IO;

public class ECSGameManagerAuthoring : MonoBehaviour
{
    public int width;
    public int height;

    public int unitCount;

    public string selectedMapName;
    [HideInInspector]
    public string selectedMapFile;

    public GameObject unitPrefab;
    public GameObject wandererPrefab;
    public GameObject lightPrefab;
    public GameObject heavyPrefab;
    public GameObject rangePrefab;
    public GameObject healerPrefab;
    public GameObject trapperPrefab;
    public GameObject workerPrefab;
    public GameObject wallPrefab;
    public GameObject resourcePrefab;
    public GameObject buildingPrefab;
    public GameObject trapPrefab;

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
                wallPrefab = GetEntity(authoring.wallPrefab, TransformUsageFlags.Dynamic),
                resourcePrefab = GetEntity(authoring.resourcePrefab, TransformUsageFlags.Dynamic),
                buildingPrefab = GetEntity(authoring.buildingPrefab, TransformUsageFlags.Dynamic),
                trapPrefab = GetEntity(authoring.trapPrefab, TransformUsageFlags.Dynamic)
            });

            var occupationCellBuffer = AddBuffer<OccupationCellBuffer>(entity);
            for (int i = 0; i < authoring.width * authoring.height; i++)
            {
                occupationCellBuffer.Add(new OccupationCellBuffer
                {
                    isOccupied = false
                });
            }

            var unitDataBuffer = AddBuffer<UnitDataBuffer>(entity);
            MapData loadedMapData = null;
            // Load MapData from selectedMapName
            if (File.Exists(authoring.selectedMapFile))
            {
                string json = File.ReadAllText(authoring.selectedMapFile);
                loadedMapData = JsonUtility.FromJson<MapData>(json);
            }
            foreach (UnitData unitData in loadedMapData.units)
            {
                unitDataBuffer.Add(new UnitDataBuffer
                {
                    id = unitData.id,
                    x = unitData.x,
                    y = unitData.y,
                    team = unitData.team,
                }); 
            }
        }
    }

}