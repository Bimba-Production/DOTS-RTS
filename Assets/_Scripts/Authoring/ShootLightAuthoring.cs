using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ShootLightAuthoring : MonoBehaviour
    {
        public float timerMax;

        public class Baker : Baker<ShootLightAuthoring>
        {
            public override void Bake(ShootLightAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShootLight
                {
                    timerMax = authoring.timerMax,
                });
            }
        }
    }

    public struct ShootLight: IComponentData
    {
        public float timer;
        public float timerMax;
    }
}