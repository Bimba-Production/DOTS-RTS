using _Scripts.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace _Scripts.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public partial struct ResetEventsSystem : ISystem
    {
        private NativeArray<JobHandle> _jobHandles;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _jobHandles = new NativeArray<JobHandle>(5, Allocator.Persistent);
        }
        
        public void OnUpdate(ref SystemState state)
        {
            _jobHandles[0] = new ResetSelectedEventsJob().ScheduleParallel(state.Dependency);
            _jobHandles[1] = new ResetHealthEventsJob().ScheduleParallel(state.Dependency);
            _jobHandles[2] = new ResetShootAttackEventsJob().ScheduleParallel(state.Dependency);
            _jobHandles[3] = new ResetMeleeAttackEventsJob().ScheduleParallel(state.Dependency);
            _jobHandles[4] = new ResetShootProjectileAttackEventsJob().ScheduleParallel(state.Dependency);
            
            NativeList<Entity> onBarracksUnitQueueChangedEntityList = new NativeList<Entity>(Allocator.TempJob);
            new ResetBuildingBarracksEventsJob
            {
                onUnitQueueChangedEntityList = onBarracksUnitQueueChangedEntityList.AsParallelWriter(),
            }.ScheduleParallel(state.Dependency).Complete();
            
            DOTSEventsManager.Instance.TriggerOnBarracksUnitQueueChanged(onBarracksUnitQueueChangedEntityList);
            
            state.Dependency = JobHandle.CombineDependencies(_jobHandles);
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
    public partial struct ResetShootProjectileAttackEventsJob: IJobEntity
    {
        private void Execute(ref ShootProjectileAttack shootProjectileAttack)
        {
            shootProjectileAttack.onShoot.isTriggered = false;
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
    
    [BurstCompile]
    public partial struct ResetMeleeAttackEventsJob: IJobEntity
    {
        private void Execute(ref MeleeAttack meleeAttack)
        {
            meleeAttack.onAttacked = false;
        }
    }    
    
    [BurstCompile]
    public partial struct ResetBuildingBarracksEventsJob: IJobEntity
    {
        public NativeList<Entity>.ParallelWriter onUnitQueueChangedEntityList;
        
        private void Execute(ref BuildingBarracks buildingBarracks, Entity entity)
        {
            if (buildingBarracks.onUnitQueueChanged)
            {
                onUnitQueueChangedEntityList.AddNoResize(entity);
            }
            
            buildingBarracks.onUnitQueueChanged = false;
        }
    }
}