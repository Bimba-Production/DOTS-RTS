using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class FactionAuthoring : MonoBehaviour
    {
        private class FactionAuthoringBaker : Baker<FactionAuthoring>
        {
            public override void Bake(FactionAuthoring authoring)
            {
            }
        }
    }
}