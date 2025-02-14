using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace _Scripts.Systems
{
    public partial struct ZombieSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRO<LocalTransform> localTransform, RefRW<ZombieSpawner> zombieSpawner) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>())
            {
                
            }
        }
    }
}