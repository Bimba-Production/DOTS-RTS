using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ZombieSpawnerAuthoring : MonoBehaviour
    {
        private class ZombieSpawnerAuthoringBaker : Baker<ZombieSpawnerAuthoring>
        {
            public override void Bake(ZombieSpawnerAuthoring authoring)
            {
            }
        }
    }
}