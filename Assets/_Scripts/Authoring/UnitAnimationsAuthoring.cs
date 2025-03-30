using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class UnitAnimationsAuthoring : MonoBehaviour
    {
        public AnimationType idleAnimationType;
        public AnimationType walkAnimationType;
        public AnimationType shootAnimationType;
        public AnimationType aimAnimationType;
        public AnimationType meleeAttackAnimationType;
        
        public class Baker : Baker<UnitAnimationsAuthoring>
        {
            public override void Bake(UnitAnimationsAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new UnitAnimations
                {
                    idleAnimationType = authoring.idleAnimationType,
                    walkAnimationType = authoring.walkAnimationType,
                    shootAnimationType = authoring.shootAnimationType,
                    aimAnimationType = authoring.aimAnimationType,
                    meleeAttackAnimationType = authoring.meleeAttackAnimationType,
                });
            }
        }
    }

    public struct UnitAnimations : IComponentData
    {
        public AnimationType idleAnimationType;
        public AnimationType walkAnimationType;
        public AnimationType shootAnimationType;
        public AnimationType aimAnimationType;
        public AnimationType meleeAttackAnimationType;
    }
}