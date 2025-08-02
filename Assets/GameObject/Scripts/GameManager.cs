using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public int width;
    public int height;

    public string selectedMapName;
    [HideInInspector]
    public string selectedMapFile;

    [Header("Unit Prefabs")]
    public GameObject lightUnitPrefab;
    public GameObject heavyUnitPrefab;
    public GameObject rangeUnitPrefab;
    public GameObject trapperUnitPrefab;
    public GameObject healerUnitPrefab;
    public GameObject workerUnitPrefab;
    public GameObject wandererUnitPrefab;

    [Header("Obstacle Prefabs")]
    public GameObject wallPrefab;
    public GameObject resourcePrefab;
    public GameObject buildingPrefab;

    public GameObject trapPrefab;

    private GridObject[,] obstacles;

    private bool initialized = false;

    private ResourceManager resourceManager;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
    }

    private void Start()
    {
        obstacles = new GridObject[width, height];

        resourceManager = FindObjectOfType<ResourceManager>();
    }

    private void Update()
    {
        if (!initialized)
        {
            MapData loadedMapData = null;
            // Load MapData from selectedMapName
            if (File.Exists(selectedMapFile))
            {
                string json = File.ReadAllText(selectedMapFile);
                loadedMapData = JsonUtility.FromJson<MapData>(json);
            }

            foreach (UnitData unitData in loadedMapData.units)
            {
                if (unitData.id < 0 || 9 < unitData.id)
                {
                    Debug.LogError($"Invalid unit id: {unitData.id}. Must be 0 to 9.");
                    continue; // Skip invalid unit
                }

                if (0 <= unitData.id && unitData.id <= 6)
                {
                    AddUnit(unitData.id, unitData.x, unitData.y, unitData.team);
                }

                switch (unitData.id)
                {
                    case 7: // Wall
                        AddWall(unitData.x, unitData.y);
                        break;
                    case 8: // Resource
                        AddResource(unitData.x, unitData.y);
                        break;
                    case 9: // Building
                        AddBuilding(unitData.x, unitData.y, unitData.team, true); // isGod = true for loading
                        break;
                }
            }

            //for (int team = 1; team <= 2; team ++)
            //    for (int i = 0; i < 1; i++)
            //        AddUnit(1, Random.Range(0, width), Random.Range(0, height), team);

            //for (int i = 0; i < 20; i++)
            //    AddUnit(-1, Random.Range(0, width), Random.Range(0, height), 1);
            //for (int i = 0; i < 20; i++)
            //    AddUnit(-1, Random.Range(0, width), Random.Range(0, height), 2);


            //for (int i = 1; i < width - 1; i += 2)
            //    AddWall(i, i);
            //for (int i = 1; i < width - 1; i += 2)
            //    AddWall(i, height - i);

            //for (int i = 1; i < 4; i++)
            //    AddResource(Random.Range(0, width), Random.Range(0, height));

            initialized = true;
        }
    }

    public Unit AddUnit(int id, int x, int y, int team)
    {
        Unit unit;

        switch (id)
        {
            case 0:
                unit = Instantiate(wandererUnitPrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<Unit>();
                break;
            case 1:
                unit = Instantiate(lightUnitPrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<Unit>();
                break;
            case 2:
                unit = Instantiate(heavyUnitPrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<Unit>();
                break;
            case 3:
                unit = Instantiate(rangeUnitPrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<Unit>();
                break;
            case 4:
                unit = Instantiate(trapperUnitPrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<Unit>();
                break;
            case 5:
                unit = Instantiate(healerUnitPrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<Unit>();
                break;
            case 6:
                unit = Instantiate(workerUnitPrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<Unit>();
                break;
            default:
                unit = Instantiate(wandererUnitPrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<Unit>();
                break;
        }

        unit.gridX = x;
        unit.gridY = y;
        unit.team = team;

        return unit;
    }

    public bool AddWall(int x, int y)
    {
        if (obstacles[x, y] == null)
        {
            GameObject wall = Instantiate(wallPrefab, new Vector3(x, y, 0), Quaternion.identity);
            obstacles[x, y] = wall.GetComponent<GridObject>();

            obstacles[x, y].gridX = x;
            obstacles[x, y].gridY = y;

            return true;
        }

        return false;
    }

    public bool AddResource(int x, int y)
    {
        if (obstacles[x, y] == null)
        {
            GameObject resource = Instantiate(resourcePrefab, new Vector3(x, y, 0), Quaternion.identity);
            obstacles[x, y] = resource.GetComponent<GridObject>();

            obstacles[x, y].gridX = x;
            obstacles[x, y].gridY = y;

            return true;
        }

        return false;
    }

    public bool AddBuilding(int x, int y, int team, bool isGod = false)
    {
        if (obstacles[x, y] == null)
        {
            if (!isGod && resourceManager.GetResourceAmount(team) < 10)
                return false;

            GameObject building = Instantiate(buildingPrefab, new Vector3(x, y, 0), Quaternion.identity);
            building.GetComponent<TeamObject>().team = team;

            obstacles[x, y] = building.GetComponent<GridObject>();
            obstacles[x, y].gridX = x;
            obstacles[x, y].gridY = y;

            return true;
        }

        return false;
    }

    public void RemoveBuilding(Building building)
    {
        if (building == null || obstacles[building.gridX, building.gridY] == null)
            return;
        obstacles[building.gridX, building.gridY] = null;
    }

    // if element is true, it is walkable, if false, it is an obstacle
    public bool[,] GetObstaclesInBoolean(bool countUnit = false)
    {
        bool[,] walkableMap = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                walkableMap[x, y] = obstacles[x, y] == null;
            }
        }

        if (countUnit)
        {
            Unit[] units = FindObjectsOfType<Unit>();
            for (int i = 0; i < units.Length; i++)
            {
                walkableMap[units[i].gridX, units[i].gridY] = false;
            }
        }

        return walkableMap;
    }

    public List<Vector2Int> GetAdjacentWalkableTiles(int centerX, int centerY)
    {
        List<Vector2Int> adjacentWalkableTiles = new List<Vector2Int>();

        bool[,] walkableMap = GetObstaclesInBoolean();
        if (centerX >= 1 && walkableMap[centerX - 1, centerY]) adjacentWalkableTiles.Add(new Vector2Int(centerX - 1, centerY));
        if (centerX <= width - 2 && walkableMap[centerX + 1, centerY]) adjacentWalkableTiles.Add(new Vector2Int(centerX + 1, centerY));
        if (centerY >= 1 && walkableMap[centerX, centerY - 1]) adjacentWalkableTiles.Add(new Vector2Int(centerX, centerY - 1));
        if (centerY <= height - 2 && walkableMap[centerX, centerY + 1]) adjacentWalkableTiles.Add(new Vector2Int(centerX, centerY + 1));

        return adjacentWalkableTiles;
    }
}
