using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ECSResourceManagerAuthoring : MonoBehaviour
{
    [HideInInspector]
    public int[] teamResources;
    public int teamNum = 2;
    public int startingResources = 20;

    public class Baker : Baker<ECSResourceManagerAuthoring>
    {
        public override void Bake(ECSResourceManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ECSResourceManager
            {
                teamNum = authoring.teamNum
            });

            var buffer = AddBuffer<TeamResourceBuffer>(entity);
            for (int i = 0; i < authoring.teamNum; i++)
            {
                buffer.Add(new TeamResourceBuffer
                {
                    amount = authoring.startingResources
                });
            }
        }
    }
}
