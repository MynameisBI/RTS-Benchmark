using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class PathRendererSystem : SystemBase
{
    private List<PathRendererReference> pathRendererReferences;

    protected override void OnCreate()
    {
        base.OnCreate();
        pathRendererReferences = new List<PathRendererReference> ();
    }

    protected override void OnUpdate()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        List<Entity> toCreatePathEntities= new List<Entity>();
        foreach (var (unitComponent, entity) in SystemAPI.Query<RefRW<UnitComponent>>().WithEntityAccess())
        {
            PathRendererReference pathRendererReference = GetPathRendererReference(entity);
            if (pathRendererReference == null)
            {
                toCreatePathEntities.Add(entity);
            }
        }
        foreach (Entity entity in toCreatePathEntities)
            pathRendererReferences.Add(PathRendererReference.CreatePathRenderer(entity));


        List<(PathRendererReference, DynamicBuffer<UnitPathBuffer>)> toBePathRenderReferences =
                new List<(PathRendererReference, DynamicBuffer<UnitPathBuffer>)>();
        foreach (var pathRendererReference in GameObject.FindObjectsOfType<PathRendererReference>())
        {
            if (entityManager.HasBuffer<UnitPathBuffer>(pathRendererReference.entity) &&
                    !entityManager.GetComponentData<UnitComponent>(pathRendererReference.entity).hasRerenderedPath)
            {
                toBePathRenderReferences.Add(
                        new (pathRendererReference, entityManager.GetBuffer<UnitPathBuffer>(pathRendererReference.entity)));
            }
            else if (!entityManager.Exists(pathRendererReference.entity))
            {
                GameObject.Destroy(pathRendererReference.gameObject);
            }
        }
        foreach (var (pathRendererReference, pathBuffer) in toBePathRenderReferences)
            RerenderPath(pathRendererReference, pathBuffer);

        for (int i = pathRendererReferences.Count - 1; i >= 0; i--)
        {
            if (!entityManager.Exists(pathRendererReferences[i].entity))
            {
                GameObject.Destroy(pathRendererReferences[i].pathRenderer.gameObject);
                pathRendererReferences.RemoveAt(i);
            }
        }
    }

    private PathRendererReference GetPathRendererReference(Entity entity)
    {
        foreach (PathRendererReference refObj in pathRendererReferences)
        {
            if (refObj.entity == entity)
                return refObj;
        }
        return null;
    }

    private void RerenderPath(PathRendererReference pathRendererReference, DynamicBuffer<UnitPathBuffer> pathBuffer)
    {
        if (pathRendererReference.pathRenderer != null)
            GameObject.Destroy(pathRendererReference.pathRenderer.gameObject);

        GameObject lineObj = new GameObject("TempLine");
        pathRendererReference.pathRenderer = lineObj.AddComponent<LineRenderer>();

        pathRendererReference.pathRenderer.positionCount = pathBuffer.Length;
        pathRendererReference.pathRenderer.material = new Material(Shader.Find("Sprites/Default")); // Optimizable
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
}
