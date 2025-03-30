using _Scripts.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace _Scripts.Systems
{
    // [UpdateAfter(typeof(ShootAttackSystem))]
    [UpdateBefore(typeof(ResetEventsSystem))]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct AnimationStateSystem : ISystem
    {
        private ComponentLookup<ActiveAnimation> _activeAnimationLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _activeAnimationLookup = state.GetComponentLookup<ActiveAnimation>(false);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _activeAnimationLookup.Update(ref state);
            
            IdleWalkingAnimationStateJob idleWalkingAnimationStateJob = new IdleWalkingAnimationStateJob
            {
                activeAnimationLookup = _activeAnimationLookup,
            };
            
            idleWalkingAnimationStateJob.ScheduleParallel();
            
            
            _activeAnimationLookup.Update(ref state);
            
            AimShootAnimationStateJob aimShootAnimationStateJob = new AimShootAnimationStateJob
            {
                activeAnimationLookup = _activeAnimationLookup,
            };
            
            aimShootAnimationStateJob.ScheduleParallel();
            
            
            _activeAnimationLookup.Update(ref state);
            
            MeleeAttackAnimationStateJob meleeAttackAnimationStateJob = new MeleeAttackAnimationStateJob
            {
                activeAnimationLookup = _activeAnimationLookup,
            };
            
            meleeAttackAnimationStateJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct IdleWalkingAnimationStateJob: IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationLookup;
        private void Execute(in AnimatedMesh animatedMesh, in UnitMover unitMover, in UnitAnimations unitAnimations)
        {
            RefRW<ActiveAnimation> activeAnimation = activeAnimationLookup.GetRefRW(animatedMesh.meshEntity);
                
            if (unitMover.isMoving)
            {
                activeAnimation.ValueRW.nextAnimationType = unitAnimations.walkAnimationType;
            }
            else
            {
                activeAnimation.ValueRW.nextAnimationType = unitAnimations.idleAnimationType;
            }
        }
    }
    
    [BurstCompile]
    public partial struct AimShootAnimationStateJob: IJobEntity
    { 
        [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationLookup;

        private void Execute(in AnimatedMesh animatedMesh,
            in ShootAttack shootAttack,
            in UnitAnimations unitAnimations,
            in UnitMover unitMover,
            in Target target)
        {
            if (!unitMover.isMoving && target.targetEntity != Entity.Null)
            {
                RefRW<ActiveAnimation> activeAnimation = activeAnimationLookup.GetRefRW(animatedMesh.meshEntity);
                activeAnimation.ValueRW.nextAnimationType = unitAnimations.aimAnimationType;
            }
            
            if (shootAttack.onShoot.isTriggered)
            {
                RefRW<ActiveAnimation> activeAnimation = activeAnimationLookup.GetRefRW(animatedMesh.meshEntity);
                activeAnimation.ValueRW.nextAnimationType = unitAnimations.shootAnimationType;
            }
        }
    }
    
    [BurstCompile]
    public partial struct MeleeAttackAnimationStateJob: IJobEntity
    { 
        [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationLookup;

        private void Execute(in AnimatedMesh animatedMesh, in MeleeAttack meleeAttack, in UnitAnimations unitAnimations)
        {
            if (meleeAttack.onAttacked)
            {
                RefRW<ActiveAnimation> activeAnimation = activeAnimationLookup.GetRefRW(animatedMesh.meshEntity);
                activeAnimation.ValueRW.nextAnimationType = unitAnimations.meleeAttackAnimationType;
            }
        }
    }
}