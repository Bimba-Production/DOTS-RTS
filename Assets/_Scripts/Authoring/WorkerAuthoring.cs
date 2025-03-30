using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class WorkerAuthoring : MonoBehaviour
    {
        public class Baker : Baker<WorkerAuthoring>
        {
            public override void Bake(WorkerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Worker
                {
                    buildingToBuild = Entity.Null,
                    buildingToDestroy = Entity.Null,
                });
                
                AddBuffer<BuildingBuffer>(entity);
            }
        }
    }
    
    public struct Worker: IComponentData
    {
        public Entity buildingToBuild;
        public Entity buildingToDestroy;
    }
    
    public struct BuildingBuffer: IBufferElementData
    {
        public Entity buildingToBuild;
        public Entity buildingToDestroy;
        public float3 targetPosition;
    }
}