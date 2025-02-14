using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class EntitiesReferencesAuthoring : MonoBehaviour
    {
        public GameObject bulletPrefabGameObject;
        public GameObject zombiePrefabGameObject;
        public GameObject shootLightGameObject;
        
        public class Baker : Baker<EntitiesReferencesAuthoring>
        {
            public override void Bake(EntitiesReferencesAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EntitiesReferences
                {
                    bulletPrefab = GetEntity(authoring.bulletPrefabGameObject, TransformUsageFlags.Dynamic),
                    zombiePrefab = GetEntity(authoring.zombiePrefabGameObject, TransformUsageFlags.Dynamic),
                    shootLightPrefab = GetEntity(authoring.shootLightGameObject, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct EntitiesReferences : IComponentData
    {
        public Entity bulletPrefab;
        public Entity zombiePrefab;
        public Entity shootLightPrefab;
    }
}