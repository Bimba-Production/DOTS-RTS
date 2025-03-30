using _Scripts.Authoring;
using Unity.Transforms;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

namespace _Scripts.Systems
{
    public partial struct BulletMoverSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach ((RefRW<LocalTransform> localTransform, RefRO<Bullet> bullet, RefRO<Target> target, Entity entity)
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Bullet>, RefRO<Target>>().WithEntityAccess())
            {
                if (target.ValueRO.targetEntity == Entity.Null)
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }
                
                LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
                ShootVictim targetShootVictim = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.targetEntity);
                float3 targetPosition = targetLocalTransform.TransformPoint(targetShootVictim.hitLocalPosition);
             
                float distanceBeforeSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

                float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
                moveDirection = math.normalize(moveDirection);
                
                localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

                float distanceAfterSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

                if (distanceBeforeSq <= distanceAfterSq)
                {
                    localTransform.ValueRW.Position = targetPosition;
                }
                
                float destroyDistanceSq = 0.2f;
                if (math.distancesq(localTransform.ValueRO.Position, targetPosition) < destroyDistanceSq)
                {
                    RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                    targetHealth.ValueRW.healthAmount -= bullet.ValueRO.damageAmount;
                    targetHealth.ValueRW.onHealthChanged = true;
                    
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}