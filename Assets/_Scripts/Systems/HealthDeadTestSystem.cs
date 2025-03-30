using _Scripts.Authoring;
using _Scripts.UI;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct HealthDeadTestSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        // [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach ((RefRO<Health> health, RefRO<Selected> selected, RefRO<Unit> unit) 
                     in SystemAPI.Query<RefRO<Health>, RefRO<Selected>, RefRO<Unit>>())
            {
                if (health.ValueRO.healthAmount <= 0)
                {
                    SelectionUI.Instance.onUnitDead = true;
                }
            }
            
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