using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class RallyPositionOffsetAuthoring : MonoBehaviour
    {
        public Transform rallyPositionOffset;

        public class Baker : Baker<RallyPositionOffsetAuthoring>
        {
            public override void Bake(RallyPositionOffsetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RallyPositionOffset
                {
                    value = authoring.rallyPositionOffset.localPosition,
                });
            }
        }
    }

    public struct RallyPositionOffset : IComponentData
    {
        public float3 value;
    }
}