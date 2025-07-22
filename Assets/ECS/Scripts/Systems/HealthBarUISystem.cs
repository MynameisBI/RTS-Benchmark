using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class HealthBarUISystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var refObj in GameObject.FindObjectsOfType<HealthBarReference>())
        {
            if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<HealthComponent>(refObj.entity))
            {
                var healthComponent = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<HealthComponent>(refObj.entity);
                refObj.slider.maxValue = healthComponent.maxHealth;
                refObj.slider.value = healthComponent.health;

                var transform = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalTransform>(refObj.entity);
                refObj.transform.position = transform.Position;
            } else
            {
                GameObject.Destroy(refObj.gameObject);
            }
        }
    }
}
