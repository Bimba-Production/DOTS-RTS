using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Authoring
{
    public class FindTargetAuthoring : MonoBehaviour
    {
        public float range;
        public FactionType targetFactionType;
        public float timerMax;
        
        public class Baker : Baker<FindTargetAuthoring>
        {
            public override void Bake(FindTargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FindTarget
                {
                    range = authoring.range,
                    targetFactionType = authoring.targetFactionType,
                    timerMax = authoring.timerMax,
                });
            }
        }
    }

    public struct FindTarget : IComponentData
    {
        public float range;
        public FactionType targetFactionType;
        public float timer;
        public float timerMax;
    }
}