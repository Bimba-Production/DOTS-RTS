using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct ShootAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntitiesReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (RefRW<ShootAttack> shootAttack 
                     in SystemAPI.Query<RefRW<ShootAttack>>())
            {
                shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (shootAttack.ValueRO.timer < 0) shootAttack.ValueRW.timer = 0f;
            }
            
            foreach ((RefRW<LocalTransform> localTransform, RefRW<ShootAttack> shootAttack,
                         RefRO<Target> target, RefRW<UnitMover> unitMover, RefRO<Faction> faction, Entity entity) 
                     in SystemAPI.Query<RefRW<LocalTransform>,RefRW<ShootAttack>,
                         RefRO<Target>, RefRW<UnitMover>, RefRO<Faction>>().WithDisabled<MoveOverride>().WithEntityAccess())
            {
                if (target.ValueRO.targetEntity == Entity.Null)
                {
                    continue;
                }
                
                LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
                
                if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) > shootAttack.ValueRO.attackDistance)
                {
                    unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
                    continue;
                }
                else
                {
                    unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
                }
                
                float3 aimDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
                aimDirection = math.normalize(aimDirection);
                
                quaternion targetRotation = quaternion.LookRotation(aimDirection, math.up());
                localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);
            }
            
            foreach ((RefRW<LocalTransform> localTransform, RefRW<ShootAttack> shootAttack,
                         RefRO<Target> target, RefRO<Faction> faction, Entity entity) 
                     in SystemAPI.Query<RefRW<LocalTransform>,RefRW<ShootAttack>,
                         RefRO<Target>, RefRO<Faction>>().WithEntityAccess())
            {
                if (target.ValueRO.targetEntity == Entity.Null)
                {
                    continue;
                }
                
                LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

                if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) >
                    shootAttack.ValueRO.attackDistance)
                {
                    continue;
                }

                if (SystemAPI.HasComponent<MoveOverride>(entity) && SystemAPI.IsComponentEnabled<MoveOverride>(entity))
                {
                    continue;
                }
                
                if (shootAttack.ValueRO.timer > 0)
                {
                    continue;    
                }
                
                shootAttack.ValueRW.timer = shootAttack.ValueRO.timerMax;
                
                float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);
                
                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= shootAttack.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChanged = true;
                shootAttack.ValueRW.onShoot.isTriggered = true;
                shootAttack.ValueRW.onShoot.shootFromPosition = bulletSpawnWorldPosition;

                if (SystemAPI.HasComponent<TargetOverride>(target.ValueRO.targetEntity))
                {
                    RefRW<TargetOverride> enemyTargetOverride = SystemAPI.GetComponentRW<TargetOverride>(target.ValueRO.targetEntity);
                    Faction enemyFaction = SystemAPI.GetComponent<Faction>(target.ValueRO.targetEntity);
                
                    if (enemyTargetOverride.ValueRO.targetEntity == Entity.Null && enemyFaction.factionType != faction.ValueRO.factionType)
                    {
                        enemyTargetOverride.ValueRW.targetEntity = entity;
                    }
                }
            }
        }
    }
}