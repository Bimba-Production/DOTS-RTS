using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class EntitiesReferencesAuthoring : MonoBehaviour
    {
        public GameObject bulletPrefabGameObject;
        public GameObject zombiePrefabGameObject;
        public GameObject shootLightGameObject;
        public GameObject soldierGameObject;
        public GameObject scoutGameObject;
        
        public GameObject buildingTowerPrefabGameObject;
        public GameObject buildingBarracksGameObject;
        
        public GameObject buildingTowerVisualGameObject;
        public GameObject buildingBarracksVisualGameObject;
        
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
                    soldierPrefab = GetEntity(authoring.soldierGameObject, TransformUsageFlags.Dynamic),
                    scoutPrefab = GetEntity(authoring.scoutGameObject, TransformUsageFlags.Dynamic),
                    buildingTowerPrefabEntity = GetEntity(authoring.buildingTowerPrefabGameObject, TransformUsageFlags.Dynamic),
                    buildingBarracksPrefabEntity = GetEntity(authoring.buildingBarracksGameObject, TransformUsageFlags.Dynamic),
                    buildingTowerVisualPrefabEntity = GetEntity(authoring.buildingTowerVisualGameObject, TransformUsageFlags.Dynamic),
                    buildingBarracksVisualPrefabEntity = GetEntity(authoring.buildingBarracksVisualGameObject, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    public struct EntitiesReferences : IComponentData
    {
        public Entity bulletPrefab;
        public Entity zombiePrefab;
        public Entity shootLightPrefab;
        public Entity soldierPrefab;
        public Entity scoutPrefab;
        
        public Entity buildingTowerPrefabEntity;
        public Entity buildingBarracksPrefabEntity;
        
        public Entity buildingTowerVisualPrefabEntity;
        public Entity buildingBarracksVisualPrefabEntity;
    }
}