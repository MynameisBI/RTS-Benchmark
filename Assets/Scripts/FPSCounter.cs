using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText;
    public TMP_Text averageFPSText;

    private float inverseAverageFPS = 0F;

    private void Update()
    {
        float fps = 1f / Time.unscaledDeltaTime;
        fpsText.text = $"FPS: {fps:0}";

        inverseAverageFPS += (Time.unscaledDeltaTime - inverseAverageFPS) * 0.03f;
        averageFPSText.text = $"Average FPS: {1f / inverseAverageFPS:0}";
    }
}
