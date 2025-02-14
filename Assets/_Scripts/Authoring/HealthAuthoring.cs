using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class HealthAuthoring : MonoBehaviour
    {
        private class HealthAuthoringBaker : Baker<HealthAuthoring>
        {
            public override void Bake(HealthAuthoring authoring)
            {
            }
        }
    }
}