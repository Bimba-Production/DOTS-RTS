using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class TargetAuthoring : MonoBehaviour
    {
        private class TargetAuthoringBaker : Baker<TargetAuthoring>
        {
            public override void Bake(TargetAuthoring authoring)
            {
            }
        }
    }
}