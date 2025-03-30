using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts.Systems
{
    [UpdateAfter(typeof(BuildBuildingSystem))]
    public partial struct DestroySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach ((RefRO<Destroyed> destroyed, Entity entity)
                     in SystemAPI.Query<RefRO<Destroyed>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
        }
    }
}