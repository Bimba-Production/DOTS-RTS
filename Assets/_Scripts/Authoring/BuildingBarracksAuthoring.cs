using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class BuildingBarracksAuthoring : MonoBehaviour
    {
        public float progressMax;
        
        public class Baker : Baker<BuildingBarracksAuthoring>
        {
            public override void Bake(BuildingBarracksAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingBarracks
                {
                    progressMax = authoring.progressMax,
                });
                
                AddBuffer<SpawnUnitTypeBuffer>(entity);
                
                AddComponent(entity, new BuildingBarracksUnitEnqueue());
                SetComponentEnabled<BuildingBarracksUnitEnqueue>(entity, false);
                
                AddComponent(entity, new RemoveUnitFromQueue());
                SetComponentEnabled<RemoveUnitFromQueue>(entity, false);
            }
        }
    }

    public struct RemoveUnitFromQueue : IComponentData, IEnableableComponent
    {
        
    }
    
    public struct BuildingBarracksUnitEnqueue : IComponentData, IEnableableComponent
    {
        public UnitType unitType;
    }
    
    public struct BuildingBarracks : IComponentData
    {
        public float progress;
        public float progressMax;
        public UnitType activeUnitType;
        public bool onUnitQueueChanged;
    }

    [InternalBufferCapacity(10)]
    public struct SpawnUnitTypeBuffer : IBufferElementData
    {
        public UnitType unitType;
    }
}