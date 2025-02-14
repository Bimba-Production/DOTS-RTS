using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class MoveOverrideAuthoring : MonoBehaviour
    {
        private class MoveOverrideAuthoringBaker : Baker<MoveOverrideAuthoring>
        {
            public override void Bake(MoveOverrideAuthoring authoring)
            {
            }
        }
    }
}