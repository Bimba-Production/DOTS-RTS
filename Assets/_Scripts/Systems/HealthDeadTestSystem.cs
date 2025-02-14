using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct HealthDeadTestSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach ((RefRO<Health> health, Entity entity) in SystemAPI.Query<RefRO<Health>>().WithEntityAccess())
            {
                if (health.ValueRO.healthAmount <= 0)
                {
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}