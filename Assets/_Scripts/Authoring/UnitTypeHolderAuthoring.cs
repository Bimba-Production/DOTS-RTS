using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class UnitTypeHolderAuthoring : MonoBehaviour
    {
        private class UnitTypeHolderAuthoringBaker : Baker<UnitTypeHolderAuthoring>
        {
            public override void Bake(UnitTypeHolderAuthoring authoring)
            {
            }
        }
    }
}