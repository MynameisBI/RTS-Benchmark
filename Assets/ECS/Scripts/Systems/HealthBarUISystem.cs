using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class HealthBarUISystem : SystemBase
{
    private List<HealthBarReference> healthBarReferences;

    protected override void OnCreate()
    {
        base.OnCreate();
        healthBarReferences = new List<HealthBarReference>();
    }

    protected override void OnUpdate()
    {
        List<Entity> noHealthBarEntities = new List<Entity> ();

        foreach (var (transform, healthComponent, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<HealthComponent>>().WithEntityAccess())
        {
            HealthBarReference healthBarReference = GetHealthBarReference(entity);
            if (healthBarReference == null)
            {
                noHealthBarEntities.Add(entity);
            } else
            {
                healthBarReference.slider.maxValue = healthComponent.ValueRO.maxHealth;
                healthBarReference.slider.value = healthComponent.ValueRO.health;
                healthBarReference.transform.position = transform.ValueRO.Position;
            }
        }

        foreach (Entity entity in noHealthBarEntities)
        {
            HealthBarReference healthBarReference =
                    HealthBarReference.CreateHealthBar(entity, SystemAPI.GetComponent<HealthComponent>(entity).maxHealth,
                            SystemAPI.GetComponent<TeamComponent>(entity).teamId);
            healthBarReferences.Add(healthBarReference);
                    
        }

        for (int i = healthBarReferences.Count - 1; i >= 0; i--)
        {
            if (!World.DefaultGameObjectInjectionWorld.EntityManager.Exists(healthBarReferences[i].entity))
            {
                GameObject.Destroy(healthBarReferences[i].gameObject);
                healthBarReferences.RemoveAt(i);
            }
        }
    }

    private HealthBarReference GetHealthBarReference(Entity entity)
    {
        foreach (HealthBarReference refObj in healthBarReferences)
        {
            if (refObj.entity == entity)
                return refObj;
        }
        return null;
    }
}
