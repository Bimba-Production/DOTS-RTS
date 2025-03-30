using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class UnitAuthoring : MonoBehaviour
    {
        private class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Unit());
            }
        }
    }

    public struct Unit: IComponentData
    {
    }
    
    public struct CommandBuffer: IBufferElementData
    {
        public CommandType value;
        public float3 point;
    }
    
    public enum CommandType
    {
        Reach = 0,
        Build = 1,
        Attack = 2,
        Get = 3,
        Fix = 4,
        Patrol = 5,
        Treat = 6,
    }
}