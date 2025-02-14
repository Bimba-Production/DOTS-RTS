using _Scripts.Authoring;
using _Scripts.MonoBehaviours;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Physics;
using Unity.Burst;
using Unity.Mathematics;

namespace _Scripts.Systems
{
    public partial struct FindTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);
            
            foreach ((RefRO<LocalTransform> localTransform, RefRW<FindTarget> findTarget,
                         RefRW<Target> target, RefRO<TargetOverride> targetOverride) 
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRW<FindTarget>,
                         RefRW<Target>, RefRO<TargetOverride>>())
            {

                findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (findTarget.ValueRO.timer > 0f)
                {
                    continue;
                }
                findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;

                if (targetOverride.ValueRO.targetEntity != Entity.Null)
                {
                    target.ValueRW.targetEntity = targetOverride.ValueRO.targetEntity;
                    continue;
                }
                
                distanceHits.Clear();
                
                CollisionFilter collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.UNITS_LAYER,
                    GroupIndex = 0
                };

                Entity closestTargetEntity = Entity.Null;
                float closestTargetDistance = float.MaxValue;
                float currentTargetDistanceOffset = 0f;
                if (target.ValueRO.targetEntity != Entity.Null)
                {
                    closestTargetEntity = target.ValueRO.targetEntity;
                    LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(closestTargetEntity);
                    closestTargetDistance = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);
                    currentTargetDistanceOffset = 2f;
                }
                
                if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.range, ref distanceHits, collisionFilter))
                {
                    foreach (DistanceHit distanceHit in distanceHits)
                    {
                        if (!SystemAPI.Exists(distanceHit.Entity) || !SystemAPI.HasComponent<Unit>(distanceHit.Entity))
                        {
                            continue;
                        }
                        
                        Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                        if (targetUnit.faction == findTarget.ValueRO.targetFaction)
                        {
                            if (closestTargetEntity == Entity.Null)
                            {
                                closestTargetEntity = distanceHit.Entity;
                                closestTargetDistance = distanceHit.Distance;
                            }
                            else
                            {
                                if (distanceHit.Distance + currentTargetDistanceOffset < closestTargetDistance)
                                {
                                    closestTargetEntity = distanceHit.Entity;
                                    closestTargetDistance = distanceHit.Distance;
                                }
                            }
                        }
                    }
                    
                    if (closestTargetEntity != Entity.Null) target.ValueRW.targetEntity = closestTargetEntity;
                }
            }
        } 
    }
}