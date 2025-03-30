using _Scripts.Authoring;
using _Scripts.UI;
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

            foreach ((RefRO<Health> health, RefRO<Selected> selected, RefRO<Unit> unit) 
                     in SystemAPI.Query<RefRO<Health>, RefRO<Selected>, RefRO<Unit>>())
            {
                if (!health.ValueRO.onHealthChanged)
                {
                    continue;
                }

                if (SelectionUI.Instance.onHealthChanged)
                {
                    continue;
                }
                
                SelectionUI.Instance.onHealthChanged = true;
            }
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
    
            float healthNormalized = (float)health.healthAmount / health.healthAmountMax;
            
            if (healthNormalized == 1f && localTransform.ValueRO.Scale != 0f) localTransform.ValueRW.Scale = 0f;
            
            if (!health.onHealthChanged)
            {
                return;
            }
            
            localTransform.ValueRW.Scale = healthNormalized == 1f ? 0f : 1f;

            RefRW<LocalTransform> localTransformBarGreen = localTransformLookup.GetRefRW(healthBar.barGreen);
            RefRW<LocalTransform> localTransformBarYellow = localTransformLookup.GetRefRW(healthBar.barYellow);
            RefRW<LocalTransform> localTransformBarBrown = localTransformLookup.GetRefRW(healthBar.barBrown);
            RefRW<LocalTransform> localTransformBarRed = localTransformLookup.GetRefRW(healthBar.barRed);
            
            if (healthNormalized < 0.3f)
            {
                localTransformBarGreen.ValueRW.Scale = 0f;
                localTransformBarYellow.ValueRW.Scale = 0f;
                localTransformBarBrown.ValueRW.Scale = 0f;
                localTransformBarRed.ValueRW.Scale = 1f;
            }
            else if (healthNormalized < 0.5f)
            {
                localTransformBarGreen.ValueRW.Scale = 0f;
                localTransformBarYellow.ValueRW.Scale = 0f;
                localTransformBarBrown.ValueRW.Scale = 1f;
                localTransformBarRed.ValueRW.Scale = 0f;
            }
            else if (healthNormalized < 0.7f)
            {
                localTransformBarGreen.ValueRW.Scale = 0f;
                localTransformBarYellow.ValueRW.Scale = 1f;
                localTransformBarBrown.ValueRW.Scale = 0f;
                localTransformBarRed.ValueRW.Scale = 0f;
            }
            else
            {
                localTransformBarGreen.ValueRW.Scale = 1f;
                localTransformBarYellow.ValueRW.Scale = 0f;
                localTransformBarBrown.ValueRW.Scale = 0f;
                localTransformBarRed.ValueRW.Scale = 0f;
            }
                
            RefRW<PostTransformMatrix> barPostTransformMatrix = postTransformMatrix.GetRefRW(healthBar.barVisualEntity);
            barPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1f, 1f);
        }
    }
}