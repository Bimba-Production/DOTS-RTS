using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class DestroyedAuthoring : MonoBehaviour
    {
        private class DestroyedAuthoringBaker : Baker<DestroyedAuthoring>
        {
            public override void Bake(DestroyedAuthoring authoring)
            {
            }
        }
    }
}