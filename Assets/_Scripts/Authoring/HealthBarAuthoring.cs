using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class HealthBarAuthoring : MonoBehaviour
    {
        public GameObject barVisualGameObject;
        public GameObject healthGameObject;
        public GameObject barGreenGameObject;
        public GameObject barYellowGameObject;
        public GameObject barBrownGameObject;
        public GameObject barRedGameObject;
        
        public class Baker : Baker<HealthBarAuthoring>
        {
            public override void Bake(HealthBarAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HealthBar
                {
                    barVisualEntity = GetEntity(authoring.barVisualGameObject, TransformUsageFlags.NonUniformScale),
                    healthEntity = GetEntity(authoring.healthGameObject, TransformUsageFlags.Dynamic),
                    barGreen = GetEntity(authoring.barGreenGameObject, TransformUsageFlags.Dynamic),
                    barYellow = GetEntity(authoring.barYellowGameObject, TransformUsageFlags.Dynamic),
                    barBrown = GetEntity(authoring.barBrownGameObject, TransformUsageFlags.Dynamic),
                    barRed = GetEntity(authoring.barRedGameObject, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct HealthBar : IComponentData
    {
        public Entity barVisualEntity;
        public Entity healthEntity;
        public Entity barGreen;
        public Entity barYellow;
        public Entity barBrown;
        public Entity barRed;
    }
}