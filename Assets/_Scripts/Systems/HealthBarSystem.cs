using _Scripts.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Scripts.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct HealthBarSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _localTransformLookup;
        private ComponentLookup<Health> _healthLookup;
        private ComponentLookup<PostTransformMatrix> _postTransformMatrix;
        
        public void OnCreate(ref SystemState state)
        {
            _localTransformLookup = state.GetComponentLookup<LocalTransform>();
            _healthLookup = state.GetComponentLookup<Health>(true);
            _postTransformMatrix = state.GetComponentLookup<PostTransformMatrix>(false);
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _postTransformMatrix.Update(ref state);
            _healthLookup.Update(ref state);
            _localTransformLookup.Update(ref state);
            
            Vector3 cameraForward = Vector3.zero;
            if (Camera.main != null)
            {
                cameraForward = Camera.main.transform.forward;
            }
            
            HealthBarJob healthBarJob = new HealthBarJob
            {
                localTransformLookup = _localTransformLookup,
                healthLookup = _healthLookup,
                postTransformMatrix = _postTransformMatrix,
                cameraForward = cameraForward,
            };
            healthBarJob.ScheduleParallel();
            
            // foreach ((RefRW<LocalTransform> localTransform, RefRO<HealthBar> healthBar) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthBar>>())
            // {
            //     LocalTransform parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.healthEntity);
            //     if (localTransform.ValueRO.Scale == 1f)
            //     {
            //         localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(Quaternion.LookRotation(cameraForward, Vector3.up));
            //     }
            //
            //     Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.healthEntity);
            //
            //     if (!health.onHealthChanged)
            //     {
            //         continue;
            //     }
            //     
            //     float healthNormalized = (float)health.healthAmount / health.healthAmountMax;
            //
            //     localTransform.ValueRW.Scale = healthNormalized == 1f ? 0f : 1f;
            //     
            //     RefRW<PostTransformMatrix> barPostTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.barVisualEntity);
            //     barPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1f, 1f);
            // }
        }
    }

    [BurstCompile]
    public partial struct HealthBarJob: IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformLookup;
        [ReadOnly] public ComponentLookup<Health> healthLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> postTransformMatrix;
        public float3 cameraForward;
        
        private void Execute(in HealthBar healthBar, Entity entity)
        {
            RefRW<LocalTransform> localTransform = localTransformLookup.GetRefRW(entity);
            
            LocalTransform parentLocalTransform = localTransformLookup[healthBar.healthEntity];
            if (localTransform.ValueRO.Scale == 1f)
            {
                localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(Quaternion.LookRotation(cameraForward, Vector3.up));
            }
    
            Health health = healthLookup[healthBar.healthEntity];
    
            if (!health.onHealthChanged)
            {
                return;
            }
                
            float healthNormalized = (float)health.healthAmount / health.healthAmountMax;
    
            localTransform.ValueRW.Scale = healthNormalized == 1f ? 0f : 1f;
                
            RefRW<PostTransformMatrix> barPostTransformMatrix = postTransformMatrix.GetRefRW(healthBar.barVisualEntity);
            barPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1f, 1f);
        }
    }
}