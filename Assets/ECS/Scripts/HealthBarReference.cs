using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarReference : MonoBehaviour
{
    public Slider slider;
    public Entity entity;

    public static HealthBarReference CreateHealthBar(Entity entity, int maxHealth)
    {
        GameObject go = new GameObject("HealthBarReference");

        HealthBarReference healthBarReference = go.AddComponent<HealthBarReference>();
        healthBarReference.slider = healthBarReference.SetupHealthBar(maxHealth);
        healthBarReference.entity = entity;

        return healthBarReference;
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
        sliderRect.localPosition = new Vector2(0, 0.9f);

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
}
