using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Unit : GridObject
{
    protected GameManager gameManager;

    public float speed = 5f;
    protected List<Vector2Int> currentPath;

    public int health = 5;
    private int maxHealth;
    private Slider healthSlider;

    public int range = 2;
    public int damage = 1;
    public float attackSpeed = 1f;
    protected float secondsPerAttack;
    protected float secondsToAttack;
    protected Unit currentTarget;

    private LineRenderer pathRenderer;

    protected void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        maxHealth = health;
    }

    protected void Start()
    {
        secondsPerAttack = 1 / attackSpeed;

        if (currentPath == null)
            currentPath = new List<Vector2Int>();

        healthSlider = SetupHealthBar(health);
    }

    private Slider SetupHealthBar(int health)
    {
        // Create UI Canvas
        GameObject canvasGO = new GameObject("HealthCanvas");
        canvasGO.transform.SetParent(this.transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        //canvasGO.AddComponent<CanvasScaler>();
        //canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1, 0.2f);
        canvasRect.localPosition = new Vector3(0, 0, 0);

        // Create Slider
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(canvasGO.transform);
        Slider healthSlider = sliderGO.AddComponent<Slider>();

        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(1.2f, 0.3f);
        sliderRect.localPosition = new Vector2(0, 0.75f);

        // Background and fill (basic visuals)
        Image bg = sliderGO.AddComponent<Image>();
        bg.color = Color.gray;
        healthSlider.targetGraphic = bg;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(sliderGO.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.red;
        healthSlider.fillRect = fill.GetComponent<RectTransform>();

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // Setup health values
        healthSlider.minValue = 0;
        healthSlider.maxValue = health;
        healthSlider.value = health;


        // Create Text object
        GameObject textGO = new GameObject("HealthText");
        textGO.transform.SetParent(canvasGO.transform);
        TextMeshProUGUI teamText = textGO.AddComponent<TextMeshProUGUI>();

        // Configure the RectTransform
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.localPosition = new Vector2(-0.6f, 0.75f);

        // Configure the text
        teamText.text = $"{team}";
        teamText.alignment = TextAlignmentOptions.Center;
        teamText.color = Color.white;
        teamText.fontSize = 0.8f;

        return healthSlider;
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

    protected void Attack(Unit target)
    {
        target.TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Destroy(gameObject);
        }
        if (healthSlider != null)
            healthSlider.value = health;
    }

    public void ReceiveHeal(int heal)
    {
        health = Mathf.Max(health + heal, maxHealth);
        if (healthSlider != null)
            healthSlider.value = health;
    }

    public bool IsFullHealth()
    {
        return health >= maxHealth;
    }

    private void OnDestroy()
    {
        if (pathRenderer != null)
        {
            Destroy(pathRenderer.gameObject);
        }
        if (healthSlider != null)
        {
            Destroy(healthSlider.gameObject);
        }
    }
}
