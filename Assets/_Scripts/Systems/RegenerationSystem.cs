using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts.Systems
{
    public partial struct RegenerationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRO<Target> target, RefRW<Regeneration> regeneration, RefRW<Health> health)
                     in SystemAPI.Query<RefRO<Target>, RefRW<Regeneration>, RefRW<Health>>())
            {
                if (target.ValueRO.targetEntity != Entity.Null) continue;
                
                regeneration.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                
                if (regeneration.ValueRO.timer > 0) continue;
                
                regeneration.ValueRW.timer = regeneration.ValueRO.timerMax;

                if (health.ValueRO.healthAmount < health.ValueRO.healthAmountMax)
                {
                    health.ValueRW.onHealthChanged = true;
                    health.ValueRW.healthAmount += regeneration.ValueRO.regenerationAmount;
                    if (health.ValueRO.healthAmount > health.ValueRO.healthAmountMax) health.ValueRW.healthAmount = health.ValueRO.healthAmountMax;
                }
            }
        }
    }
}