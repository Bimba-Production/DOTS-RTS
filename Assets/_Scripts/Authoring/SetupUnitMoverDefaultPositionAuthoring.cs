﻿using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class SetupUnitMoverDefaultPositionAuthoring : MonoBehaviour
    {
        public class Baker : Baker<SetupUnitMoverDefaultPositionAuthoring>
        {
            public override void Bake(SetupUnitMoverDefaultPositionAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SetupUnitMoverDefaultPosition());
            }
        }
    }

    public struct SetupUnitMoverDefaultPosition : IComponentData
    {
        
    }
}