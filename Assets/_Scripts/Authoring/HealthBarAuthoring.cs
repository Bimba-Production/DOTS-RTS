using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class HealthBarAuthoring : MonoBehaviour
    {
        private class HealthBarAuthoringBaker : Baker<HealthBarAuthoring>
        {
            public override void Bake(HealthBarAuthoring authoring)
            {
            }
        }
    }
}