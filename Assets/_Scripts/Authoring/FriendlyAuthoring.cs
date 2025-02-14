using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class FriendlyAuthoring : MonoBehaviour
    {
        private class FriendlyAuthoringBaker : Baker<FriendlyAuthoring>
        {
            public override void Bake(FriendlyAuthoring authoring)
            {
            }
        }
    }
}