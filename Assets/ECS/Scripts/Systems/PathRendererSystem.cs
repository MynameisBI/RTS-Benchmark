using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class PathRendererSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        foreach (var pathRendererReference in GameObject.FindObjectsOfType<PathRendererReference>())
        {
            if (entityManager.HasBuffer<UnitPathBuffer>(pathRendererReference.entity) &&
                    !entityManager.GetComponentData<UnitComponent>(pathRendererReference.entity).hasRerenderedPath)
            {
                DynamicBuffer<UnitPathBuffer> pathBuffer = entityManager.GetBuffer<UnitPathBuffer>(pathRendererReference.entity);

                if (pathRendererReference.pathRenderer != null)
                    GameObject.Destroy(pathRendererReference.pathRenderer.gameObject);

                GameObject lineObj = new GameObject("TempLine");
                pathRendererReference.pathRenderer = lineObj.AddComponent<LineRenderer>();

                pathRendererReference.pathRenderer.positionCount = pathBuffer.Length;
                pathRendererReference.pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
                pathRendererReference.pathRenderer.startColor = Color.green;
                pathRendererReference.pathRenderer.endColor = Color.green;
                pathRendererReference.pathRenderer.startWidth = 0.05f;
                pathRendererReference.pathRenderer.endWidth = 0.05f;

                for (int i = 0; i < pathBuffer.Length; i++)
                {
                    pathRendererReference.pathRenderer.SetPosition(i, new Vector2(pathBuffer[i].position.x, pathBuffer[i].position.y));
                }

                SystemAPI.GetComponentRW<UnitComponent>(pathRendererReference.entity).ValueRW.hasRerenderedPath = true;
            }
            else if (!entityManager.Exists(pathRendererReference.entity))
            {
                //GameObject.Destroy(pathRendererReference.gameObject);
            }
        }
    }
}
