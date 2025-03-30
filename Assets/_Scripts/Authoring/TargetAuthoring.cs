using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class TargetAuthoring : MonoBehaviour
    {
        public GameObject targetGameObject;
        
        public class Baker : Baker<TargetAuthoring>
        {
            public override void Bake(TargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Target
                {
                    targetEntity = GetEntity(authoring.targetGameObject, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct Target: IComponentData
    {
        public Entity targetEntity;
    }
}