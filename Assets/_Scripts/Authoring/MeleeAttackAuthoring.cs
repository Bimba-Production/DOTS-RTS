using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class MeleeAttackAuthoring : MonoBehaviour
    {
        public float timerMax;
        public float attackDistanceSq;
        public int damageAmount;
        public float colliderSize;
        
        public class Baker : Baker<MeleeAttackAuthoring>
        {
            public override void Bake(MeleeAttackAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MeleeAttack
                {
                    timerMax = authoring.timerMax,
                    attackDistanceSq = authoring.attackDistanceSq,
                    damageAmount = authoring.damageAmount,
                    colliderSize = authoring.colliderSize,
                });
            }
        }
    }

    public struct MeleeAttack : IComponentData
    {
        public float timer;
        public float timerMax;
        public float attackDistanceSq;
        public int damageAmount;
        public float colliderSize;
    }
}