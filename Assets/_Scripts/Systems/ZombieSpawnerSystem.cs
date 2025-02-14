using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct ZombieSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EntitiesReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach ((RefRO<LocalTransform> localTransform, RefRW<ZombieSpawner> zombieSpawner, RefRW<ZombieSpawnerRandom> zombieSpawnerRandom) 
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>, RefRW<ZombieSpawnerRandom>>())
            {
                zombieSpawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                
                if (zombieSpawner.ValueRW.timer > 0)
                {
                    continue;
                }
                
                zombieSpawner.ValueRW.timer = zombieSpawner.ValueRO.timerMax;

                Entity zombieEntity = state.EntityManager.Instantiate(entitiesReferences.zombiePrefab);

                float3 randomPosition = localTransform.ValueRO.Position;
                
                if (zombieSpawner.ValueRO.spawnRange > 0)
                {
                    float3 minPosition = localTransform.ValueRO.Position - new float3(zombieSpawner.ValueRO.spawnRange, 0, zombieSpawner.ValueRO.spawnRange);
                    float3 maxPosition = localTransform.ValueRO.Position + new float3(zombieSpawner.ValueRO.spawnRange, 0, zombieSpawner.ValueRO.spawnRange);
                    randomPosition = zombieSpawnerRandom.ValueRW.random.NextFloat3(minPosition, maxPosition) + localTransform.ValueRO.Position;
                }
                
                SystemAPI.SetComponent(zombieEntity, LocalTransform.FromPosition(randomPosition));
                ecb.AddComponent(zombieEntity, new RandomWalking
                {
                    originPosition = localTransform.ValueRO.Position,
                    targetPosition = localTransform.ValueRO.Position,
                    distanceMin = zombieSpawner.ValueRO.randomWalkingDistanceMin,
                    distanceMax = zombieSpawner.ValueRO.randomWalkingDistanceMax,
                    random = new Random((uint)zombieSpawnerRandom.ValueRW.random.NextInt(1, 10000)),
                });
            }
        }
    }
}