using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class RandomWalkingAuthoring : MonoBehaviour
    {
        private class RandomWalkingAuthoringBaker : Baker<RandomWalkingAuthoring>
        {
            public override void Bake(RandomWalkingAuthoring authoring)
            {
            }
        }
    }
}