using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class LoseTargetAuthoring : MonoBehaviour
    {
        public float loseTargetDistance;

        public class Baker : Baker<LoseTargetAuthoring>
        {
            public override void Bake(LoseTargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LoseTarget
                {
                    loseTargetDistance = authoring.loseTargetDistance,
                });
            }
        }
    }
    
    public struct LoseTarget: IComponentData
    {
        public float loseTargetDistance;
    }
}