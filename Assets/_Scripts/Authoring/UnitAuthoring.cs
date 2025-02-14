using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class UnitAuthoring : MonoBehaviour
    {
        private class UnitAuthoringBaker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
            }
        }
    }
}