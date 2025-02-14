using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class LoseTargetAuthoring : MonoBehaviour
    {
        private class LoseTargetAuthoringBaker : Baker<LoseTargetAuthoring>
        {
            public override void Bake(LoseTargetAuthoring authoring)
            {
            }
        }
    }
}