using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ActiveAnimationAuthoring : MonoBehaviour
    {
        private class ActiveAnimationAuthoringBaker : Baker<ActiveAnimationAuthoring>
        {
            public override void Bake(ActiveAnimationAuthoring authoring)
            {
            }
        }
    }
}