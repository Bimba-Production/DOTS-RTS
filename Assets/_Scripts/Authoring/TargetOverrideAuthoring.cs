using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class TargetOverrideAuthoring : MonoBehaviour
    {
        private class TargetOverrideAuthoringBaker : Baker<TargetOverrideAuthoring>
        {
            public override void Bake(TargetOverrideAuthoring authoring)
            {
            }
        }
    }
}