using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace _Scripts.Authoring
{
    public class ZombieSpawnerAuthoring : MonoBehaviour
    {
        public float timerMax;
        public uint randomSeed;
        public float spawnRange;
        public float randomWalkingDistanceMin;
        public float randomWalkingDistanceMax;
        
        public class Baker : Baker<ZombieSpawnerAuthoring>
        {
            public override void Bake(ZombieSpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ZombieSpawner
                {
                    timerMax = authoring.timerMax,
                    spawnRange = authoring.spawnRange,
                    randomWalkingDistanceMin = authoring.randomWalkingDistanceMin,
                    randomWalkingDistanceMax = authoring.randomWalkingDistanceMax,
                });
                AddComponent(entity, new ZombieSpawnerRandom
                {
                    random = Random.CreateFromIndex(authoring.randomSeed),
                });
            }
        }
    }

    public struct ZombieSpawner : IComponentData
    {
        public float timer;
        public float timerMax;
        public float spawnRange;
        public float randomWalkingDistanceMin;
        public float randomWalkingDistanceMax;
    }

    public struct ZombieSpawnerRandom : IComponentData
    {
        public Random random;
    }
}