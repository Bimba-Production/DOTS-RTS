using System;
using UnityEngine;

namespace _Scripts.MonoBehaviours
{
    public class GameAssets : MonoBehaviour
    {
        public const int UNITS_LAYER = 6;
        public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;
        
        public static GameAssets Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}