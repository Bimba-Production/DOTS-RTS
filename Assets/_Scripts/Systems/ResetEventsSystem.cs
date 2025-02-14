using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public partial struct ResetEventsSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ResetSelectedEventsJob().ScheduleParallel();
            new ResetHealthEventsJob().ScheduleParallel();
            new ResetShootAttackEventsJob().ScheduleParallel();
        }
    }
    
    [BurstCompile]
    public partial struct ResetShootAttackEventsJob: IJobEntity
    {
        private void Execute(ref ShootAttack shootAttack)
        {
            shootAttack.onShoot.isTriggered = false;
        }
    }

    [BurstCompile]
    public partial struct ResetHealthEventsJob: IJobEntity
    {
        private void Execute(ref Health health)
        {
            health.onHealthChanged = false;
        }
    }

    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct ResetSelectedEventsJob: IJobEntity
    {
        private void Execute(ref Selected selected)
        {
            selected.onSelected = false;
            selected.onDeselected = false;
        }
    }
}