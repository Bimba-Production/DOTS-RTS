using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ShootProjectileAttackAuthoring : MonoBehaviour
    {
        public float timerMax;
        public int damageAmount;
        public float attackDistance;
        public Transform bulletSpawnPositionTransform;
        
        public class Baker : Baker<ShootProjectileAttackAuthoring> {


            public override void Bake(ShootProjectileAttackAuthoring authoring) {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShootProjectileAttack {
                    timerMax = authoring.timerMax,
                    damageAmount = authoring.damageAmount,
                    attackDistance = authoring.attackDistance,
                    bulletSpawnLocalPosition = authoring.bulletSpawnPositionTransform.localPosition,
                });
            }
        }
    }
    
    public struct ShootProjectileAttack : IComponentData {

        public float timer;
        public float timerMax;
        public int damageAmount;
        public float attackDistance;
        public float3 bulletSpawnLocalPosition;
        public OnShootEvent onShoot;

        public struct OnShootEvent {
            public bool isTriggered;
            public float3 shootFromPosition;
        }
    }
}