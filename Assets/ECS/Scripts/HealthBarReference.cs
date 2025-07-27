using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarReference : MonoBehaviour
{
    public Slider slider;
    public Entity entity;

    public static HealthBarReference CreateHealthBar(Entity entity, int maxHealth, int? team)
    {
        GameObject go = new GameObject("HealthBarReference");

        HealthBarReference healthBarReference = go.AddComponent<HealthBarReference>();
        healthBarReference.slider = healthBarReference.SetupHealthBar(maxHealth, team);
        healthBarReference.entity = entity;

        return healthBarReference;
    }

    private Slider SetupHealthBar(int health, int? team)
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
}
