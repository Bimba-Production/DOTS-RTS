using System;
using UnityEngine;

namespace _Scripts.MonoBehaviours
{
    public class GameAssets : MonoBehaviour
    {
        public const int UNITS_LAYER = 6;
        public const int BUILDINGS_LAYER = 7;
        public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;
        public const float CONTROL_DISTANCE = 40f;
        
        public static GameAssets Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        
        public UnitTypeListSO unitTypeListSO;
        public BuildingTypeListSO buildingTypeListSO;
    }
}