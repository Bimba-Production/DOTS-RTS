using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class AnimationDataHolderAuthoring : MonoBehaviour
    {
        private class AnimationDataHolderAuthoringBaker : Baker<AnimationDataHolderAuthoring>
        {
            public override void Bake(AnimationDataHolderAuthoring authoring)
            {
            }
        }
    }
}