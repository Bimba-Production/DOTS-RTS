using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class FindTargetAuthoring : MonoBehaviour
    {
        private class FindTargetAuthoringBaker : Baker<FindTargetAuthoring>
        {
            public override void Bake(FindTargetAuthoring authoring)
            {
            }
        }
    }
}