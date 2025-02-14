using _Scripts.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace _Scripts.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial struct ResetTargetSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _localTransformLookup;
        private EntityStorageInfoLookup _entityStorageInfoLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _localTransformLookup = state.GetComponentLookup<LocalTransform>(true);
            _entityStorageInfoLookup = state.GetEntityStorageInfoLookup();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _localTransformLookup.Update(ref state);
            _entityStorageInfoLookup.Update(ref state);
            
            new ResetTargetJob
            {
                localTransformLookup = _localTransformLookup,
                entityStorageInfoLookup = _entityStorageInfoLookup,
            }.ScheduleParallel();
            
            new ResetTargetOverrideJob
            {
                localTransformLookup = _localTransformLookup,
                entityStorageInfoLookup = _entityStorageInfoLookup,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ResetTargetJob: IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> localTransformLookup;
        [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
        private void Execute(ref Target target)
        {
            if (target.targetEntity != Entity.Null)
            {
                if (!entityStorageInfoLookup.Exists(target.targetEntity) || !localTransformLookup.HasComponent(target.targetEntity))
                {
                    target.targetEntity = Entity.Null;
                }
            }
        }
    }
    
    [BurstCompile]
    public partial struct ResetTargetOverrideJob: IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> localTransformLookup;
        [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
        private void Execute(ref TargetOverride targetOverride)
        {
            if (targetOverride.targetEntity != Entity.Null)
            {
                if (!entityStorageInfoLookup.Exists(targetOverride.targetEntity) || !localTransformLookup.HasComponent(targetOverride.targetEntity))
                {
                    targetOverride.targetEntity = Entity.Null;
                }
            }
        }
    }
}