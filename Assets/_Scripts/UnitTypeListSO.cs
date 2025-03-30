using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    [CreateAssetMenu()]
    public class UnitTypeListSO : ScriptableObject
    {
        public List<UnitTypeSO> unitTypes;
        
        public UnitTypeSO GetUnitTypeSO(UnitType unitType)
        {
            foreach (UnitTypeSO unitTypeSO in unitTypes)
            {
                if (unitTypeSO.unitType == unitType) return unitTypeSO;
            }
            Debug.LogError($"UnitTypeListSO unitTypes is null for unitType - {unitType}");
            return null;
        }
    }
}