using _Scripts.Authoring;
using Unity.Entities;
using UnityEngine;

namespace _Scripts
{
    [CreateAssetMenu()]
    public class UnitTypeSO : ScriptableObject
    {
        public UnitType unitType;

        public float progressMax;
        
        public Sprite sprite;
        
        public Entity GetPrefabEntity(EntitiesReferences entitiesReferences)
        {
            switch (unitType)
            {
                default: 
                case UnitType.None:
                case UnitType.Soldier: return entitiesReferences.soldierPrefab;
                case UnitType.Scout: return entitiesReferences.scoutPrefab;
                case UnitType.Zombie: return entitiesReferences.zombiePrefab;
            }
        }
    }

    public enum UnitType
    {
        None = 0,
        Soldier = 1,
        Scout = 2,
        Zombie = 3,
        Tower = 4,
        Worker = 5,
    }
}