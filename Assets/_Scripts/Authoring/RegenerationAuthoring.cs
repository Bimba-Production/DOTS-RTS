using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class RegenerationAuthoring : MonoBehaviour
    {
        public float timerMax;
        public int regenerationAmount;
        
        public class RegenerationAuthoringBaker : Baker<RegenerationAuthoring>
        {
            public override void Bake(RegenerationAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Regeneration
                {
                    timer = authoring.timerMax,
                    timerMax = authoring.timerMax,
                    regenerationAmount = authoring.regenerationAmount,
                });
            }
        }
    }

    public struct Regeneration : IComponentData
    {
        public float timer;
        public float timerMax;
        public int regenerationAmount;
    }
}