using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct ShootProjectileAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) 
        {
            state.RequireForUpdate<EntitiesReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

            foreach (RefRW<ShootProjectileAttack> shootProjectileAttack 
                     in SystemAPI.Query<RefRW<ShootProjectileAttack>>())
            {
                shootProjectileAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (shootProjectileAttack.ValueRO.timer < 0) shootProjectileAttack.ValueRW.timer = 0f;
            }
            
            foreach ((
                         RefRW<LocalTransform> localTransform, RefRW<ShootProjectileAttack> shootProjectileAttack, 
                         RefRO<Target> target, RefRW<UnitMover> unitMover, Entity entity)
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ShootProjectileAttack>,
                         RefRO<Target>, RefRW<UnitMover>>().WithDisabled<MoveOverride>().WithEntityAccess())
            {

                if (target.ValueRO.targetEntity == Entity.Null)
                {
                    continue;
                }

                LocalTransform targetLocalTransform =
                    SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

                if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) >
                    shootProjectileAttack.ValueRO.attackDistance)
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
                localTransform.ValueRW.Rotation =
                    math.slerp(localTransform.ValueRO.Rotation, targetRotation,
                        SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);
            }
            
            foreach ((RefRW<LocalTransform> localTransform, RefRW<ShootProjectileAttack> shootProjectileAttack, 
                         RefRO<Target> target, RefRO<Faction> faction, Entity entity)
                     in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ShootProjectileAttack>,
                         RefRO<Target>, RefRO<Faction>>().WithEntityAccess())
            {

                if (target.ValueRO.targetEntity == Entity.Null)
                {
                    continue;
                }

                LocalTransform targetLocalTransform =
                    SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
                
                if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) >
                    shootProjectileAttack.ValueRO.attackDistance)
                {
                    continue;
                }

                if (SystemAPI.HasComponent<MoveOverride>(entity) && SystemAPI.IsComponentEnabled<MoveOverride>(entity))
                {
                    continue;
                }
                
                if (shootProjectileAttack.ValueRO.timer > 0f)
                {
                    continue;
                }

                shootProjectileAttack.ValueRW.timer = shootProjectileAttack.ValueRO.timerMax;

                float3 bulletSpawnWorldPosition =
                    localTransform.ValueRO.TransformPoint(shootProjectileAttack.ValueRO.bulletSpawnLocalPosition);
                Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.bulletPrefab);
                SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPosition));

                RefRW<Bullet> bulletBullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
                bulletBullet.ValueRW.damageAmount = shootProjectileAttack.ValueRO.damageAmount;

                RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
                bulletTarget.ValueRW.targetEntity = target.ValueRO.targetEntity;

                shootProjectileAttack.ValueRW.onShoot.isTriggered = true;
                shootProjectileAttack.ValueRW.onShoot.shootFromPosition = bulletSpawnWorldPosition;
                
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