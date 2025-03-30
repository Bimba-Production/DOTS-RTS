using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class BuildingBarracksAuthoring : MonoBehaviour
    {
        private class BuildingBarracksAuthoringBaker : Baker<BuildingBarracksAuthoring>
        {
            public override void Bake(BuildingBarracksAuthoring authoring)
            {
            }
        }
    }
}