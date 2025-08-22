using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamObject : GridObject
{
    [Header("Team Object Properties")]
    public int team;
    public int health = 5;
    protected int maxHealth;

    protected GameManager gameManager;

    private Slider healthSlider;

    protected void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    protected void Start()
    {
        maxHealth = health;
        healthSlider = SetupHealthBar(health);
    }

    protected Slider SetupHealthBar(int health)
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

        return healthSlider;
    }

    protected void Attack(TeamObject target, int damage)
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
        health = Mathf.Min(health + heal, maxHealth);
        if (healthSlider != null)
            healthSlider.value = health;
    }

    public bool IsFullHealth()
    {
        return health >= maxHealth;
    }

    protected void OnDestroy()
    {
        if (healthSlider != null)
        {
            Destroy(healthSlider.gameObject);
        }
    }
}
