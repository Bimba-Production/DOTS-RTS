using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct SetupUnitMoverDefaultPositionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach ((RefRO<LocalTransform> localTransform, RefRW<UnitMover> unitMover,
                         RefRO<SetupUnitMoverDefaultPosition> setupUnitMoverDefaultPosition, Entity entity) 
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRW<UnitMover>,
                         RefRO<SetupUnitMoverDefaultPosition>>().WithEntityAccess())
            {
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
                ecb.RemoveComponent<SetupUnitMoverDefaultPosition>(entity);
            }
        }
    }
}