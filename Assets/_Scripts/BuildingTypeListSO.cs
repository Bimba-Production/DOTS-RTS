using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    [CreateAssetMenu()]
    public class BuildingTypeListSO : ScriptableObject
    {
        public List<BuildingTypeSO> buildingTypes;

        public BuildingTypeSO none;
        
        public BuildingTypeSO GetBuildingTypeSO(BuildingType buildingType)
        {
            foreach (BuildingTypeSO buildingTypeSO in buildingTypes)
            {
                if (buildingTypeSO.buildingType == buildingType) return buildingTypeSO;
            }
            Debug.LogError($"BuildingTypeListSO buildingTypes is null for buildingType - {buildingType}");
            return null;
        }
    }
}