using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class PathRendererReference : MonoBehaviour
{
    public LineRenderer pathRenderer;
    public Entity entity;

    public static PathRendererReference CreatePathRenderer(Entity entity)
    {
        GameObject go = new GameObject("HealthBarReference");

        PathRendererReference pathRendererReference = go.AddComponent<PathRendererReference>();
        pathRendererReference.entity = entity;

        return pathRendererReference;
    }
}
