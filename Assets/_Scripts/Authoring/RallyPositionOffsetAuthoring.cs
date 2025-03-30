using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class RallyPositionOffsetAuthoring : MonoBehaviour
    {
        public Transform rallyPositionOffset;
        public GameObject pointerGameObject;
        public GameObject lineGameObject;

        public class Baker : Baker<RallyPositionOffsetAuthoring>
        {
            public override void Bake(RallyPositionOffsetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RallyPositionOffset
                {
                    value = authoring.rallyPositionOffset.localPosition,
                    pointer = GetEntity(authoring.pointerGameObject, TransformUsageFlags.Dynamic),
                    line = GetEntity(authoring.lineGameObject, TransformUsageFlags.NonUniformScale),
                });
            }
        }
    }

    public struct RallyPositionOffset : IComponentData
    {
        public float3 value;
        public Entity pointer;
        public Entity line;
    }
}