using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class RegenerationAuthoring : MonoBehaviour
    {
        private class RegenerationAuthoringBaker : Baker<RegenerationAuthoring>
        {
            public override void Bake(RegenerationAuthoring authoring)
            {
            }
        }
    }
}