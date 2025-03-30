using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Authoring
{
    public class ActiveAnimationAuthoring : MonoBehaviour
    {
        public AnimationType nextAnimationType;
        
        public class Baker : Baker<ActiveAnimationAuthoring>
        {
            public override void Bake(ActiveAnimationAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ActiveAnimation
                {
                    nextAnimationType = authoring.nextAnimationType,
                });
            }
        }
    }

    public struct ActiveAnimation : IComponentData
    {
        public int frame;
        public float frameTimer;
        public AnimationType activeAnimationType;
        public AnimationType nextAnimationType;
    }
}