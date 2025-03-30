using _Scripts.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;

namespace _Scripts.Systems
{
    public partial struct MeleeAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            NativeList<RaycastHit> hits = new NativeList<RaycastHit>(Allocator.Temp);
            
            foreach ((RefRO<LocalTransform> localTransform, RefRW<MeleeAttack> meleeAttack, RefRO<Target> target, RefRW<UnitMover> unitMover) 
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRW<MeleeAttack>, RefRO<Target>, RefRW<UnitMover>>().WithDisabled<MoveOverride>())
            {
                if (target.ValueRO.targetEntity == Entity.Null)
                {
                    continue;
                }
                
                LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

                bool isCloseEnoughToAttack =
                    math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.Position) < meleeAttack.ValueRO.attackDistanceSq;
                bool isTouchingTarget = false;

                if (!isCloseEnoughToAttack)
                {
                    float3 directionToTarget = targetLocalTransform.Position - localTransform.ValueRO.Position;
                    directionToTarget = math.normalize(directionToTarget);

                    float distanceExtraToTestRaycast = 0.4f;
                    RaycastInput raycastInput = new RaycastInput
                    {
                        Start = localTransform.ValueRO.Position,
                        End = localTransform.ValueRO.Position + directionToTarget * (meleeAttack.ValueRO.colliderSize + distanceExtraToTestRaycast),
                        Filter = CollisionFilter.Default,
                    };
                    
                    hits.Clear();
                    
                    if (collisionWorld.CastRay(raycastInput, ref hits))
                    {
                        foreach (RaycastHit hit in hits)
                        {
                            if (hit.Entity == target.ValueRO.targetEntity)
                            {
                                isTouchingTarget = true;
                                break;
                            }
                        }    
                    }
                }
                
                if (!isCloseEnoughToAttack && !isTouchingTarget)
                {
                    unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
                }
                else
                {
                    unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
                    
                    meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;

                    if (meleeAttack.ValueRO.timer > 0)
                    {
                        continue;
                    }

                    meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;
                    
                    RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                    targetHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                    targetHealth.ValueRW.onHealthChanged = true;
                    
                    meleeAttack.ValueRW.onAttacked = true;
                }
            }
        }
    }
}