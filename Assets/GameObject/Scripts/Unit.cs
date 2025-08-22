using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Unit : TeamObject
{
    public float speed = 5f;
    protected List<Vector2Int> currentPath;
    private LineRenderer pathRenderer;

    public int range = 2;
    public int damage = 1;
    public float attackSpeed = 1f;
    protected float secondsPerAttack;
    protected float secondsToAttack;
    protected TeamObject currentTarget;

    protected new void Awake()
    {
        base.Awake();
        maxHealth = health;
    }

    protected new void Start()
    {
        base.Start();
        secondsPerAttack = 1 / attackSpeed;

        if (currentPath == null)
            currentPath = new List<Vector2Int>();
    }

    protected void Update()
    {
        secondsToAttack -= Time.deltaTime;
    }

    public void Move(int x, int y)
    {
        if (x >= 0 && x < gameManager.width && y >= 0 && y < gameManager.height)
        {
            bool[,] walkableMap = gameManager.GetObstaclesInBoolean(false);

            AStarPathfinder pathfinder = new AStarPathfinder(walkableMap);
            currentPath = pathfinder.FindPath(new Vector2Int(this.gridX, this.gridY), new Vector2Int(x, y));
            if (currentPath == null)
                currentPath = new List<Vector2Int>();

            // Draw path for debugging
            if (pathRenderer != null)
            {
                Destroy(pathRenderer.gameObject);
            }

            GameObject lineObj = new GameObject("TempLine");
            pathRenderer = lineObj.AddComponent<LineRenderer>();
            pathRenderer.positionCount = currentPath.Count;
            pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
            pathRenderer.startColor = Color.green;
            pathRenderer.endColor = Color.green;
            pathRenderer.startWidth = 0.05f;
            pathRenderer.endWidth = 0.05f;

            for (int i = 0; i < currentPath.Count; i++)
            {   
                pathRenderer.SetPosition(i, new Vector2(currentPath[i].x, currentPath[i].y));
            }
        }
        else
        {
            
        }
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();

        if (pathRenderer != null)
        {
            Destroy(pathRenderer.gameObject);
        }
    }
}
