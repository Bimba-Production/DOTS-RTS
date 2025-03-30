using _Scripts.Authoring;
using _Scripts.MonoBehaviours;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct BuildBuildingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach ((RefRW<UnitMover> unitMover, RefRW<Worker> worker, Entity entity) 
                     in SystemAPI.Query<RefRW<UnitMover>, RefRW<Worker>>().WithEntityAccess())
            {
                if (unitMover.ValueRO.isMoving) continue;
                
                if (worker.ValueRO.buildingToBuild == Entity.Null) continue;
                
                Entity buildingEntity = state.EntityManager.Instantiate(worker.ValueRO.buildingToBuild);
                SystemAPI.SetComponent(buildingEntity, LocalTransform.FromPosition(unitMover.ValueRO.targetPosition));

                worker.ValueRW.buildingToBuild = Entity.Null;
                
                DynamicBuffer<BuildingBuffer> buildingBuffers = SystemAPI.GetBuffer<BuildingBuffer>(entity);
                
                if (buildingBuffers.IsEmpty) UnitSelectionManager.Instance.ClearPositionsBuffer();
                
                unitMover.ValueRW.targetPosition = unitMover.ValueRO.targetPosition + new float3(10f, 0, 0);
                
                ecb.DestroyEntity(worker.ValueRO.buildingToDestroy);
                
                worker.ValueRW.buildingToDestroy = Entity.Null;
            }
        }
    }
}