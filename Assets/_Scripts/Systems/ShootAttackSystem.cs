using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Scripts.Systems
{
    public partial struct ShootAttackSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntitiesReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<LocalTransform> localTransform, RefRW<ShootAttack> shootAttack,
                         RefRO<Target> target, RefRW<UnitMover> unitMover, RefRO<Unit> unit, Entity entity) 
                     in SystemAPI.Query<RefRW<LocalTransform>,RefRW<ShootAttack>,
                         RefRO<Target>, RefRW<UnitMover>, RefRO<Unit>>().WithDisabled<MoveOverride>().WithEntityAccess())
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
                
                shootAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                
                if (shootAttack.ValueRO.timer > 0)
                {
                    continue;    
                }
                
                shootAttack.ValueRW.timer = shootAttack.ValueRO.timerMax;

                RefRW<TargetOverride> enemyTargetOverride = SystemAPI.GetComponentRW<TargetOverride>(target.ValueRO.targetEntity);
                Unit enemyUnit = SystemAPI.GetComponent<Unit>(target.ValueRO.targetEntity);
                if (enemyTargetOverride.ValueRO.targetEntity == Entity.Null && enemyUnit.faction != unit.ValueRO.faction)
                {
                    enemyTargetOverride.ValueRW.targetEntity = entity;
                }
                
                float3 bulletSpawnWorldPosition = localTransform.ValueRO.TransformPoint(shootAttack.ValueRO.bulletSpawnLocalPosition);
                
                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.healthAmount -= shootAttack.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChanged = true;
                shootAttack.ValueRW.onShoot.isTriggered = true;
                shootAttack.ValueRW.onShoot.shootFromPosition = bulletSpawnWorldPosition;
            }
        }
    }
}