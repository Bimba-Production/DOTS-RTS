using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class AnimatedMeshAuthoring : MonoBehaviour
    {
        private class AnimatedMeshAuthoringBaker : Baker<AnimatedMeshAuthoring>
        {
            public override void Bake(AnimatedMeshAuthoring authoring)
            {
            }
        }
    }
}