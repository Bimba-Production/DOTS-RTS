using _Scripts.Authoring;
using Unity.Entities;
using UnityEngine;

namespace _Scripts
{
    [CreateAssetMenu()]
    public class BuildingTypeSO : ScriptableObject
    {
        public BuildingType buildingType;
        public Transform prefab;
        public float buildingDistanceMin;
        public bool showInBuildingPlacementManagerUI;
        public Sprite icon;
        public Transform visualPrefab;
        public KeyCode keyCode;
        public bool IsNone => buildingType == BuildingType.None;
        
        public Entity GetPrefabEntity(EntitiesReferences entitiesReferences)
        {
            switch (buildingType)
            {
                default: 
                case BuildingType.None:
                case BuildingType.Tower: return entitiesReferences.buildingTowerPrefabEntity;
                case BuildingType.Barracks: return entitiesReferences.buildingBarracksPrefabEntity;
            }
        }
        
        public Entity GetVisualPrefabEntity(EntitiesReferences entitiesReferences)
        {
            switch (buildingType)
            {
                default: 
                case BuildingType.None:
                case BuildingType.Tower: return entitiesReferences.buildingTowerVisualPrefabEntity;
                case BuildingType.Barracks: return entitiesReferences.buildingBarracksVisualPrefabEntity;
            }
        }
    }

    public enum BuildingType
    {
        None = 0,
        ZombieSpawner = 1,
        Tower = 2,
        Barracks = 3,
    }
}