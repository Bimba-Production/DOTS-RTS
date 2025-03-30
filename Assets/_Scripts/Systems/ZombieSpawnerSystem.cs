using _Scripts.Authoring;
using _Scripts.MonoBehaviours;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace _Scripts.Systems
{
    public partial struct ZombieSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EntitiesReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
            
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            CollisionFilter filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNITS_LAYER,
                GroupIndex = 0,
            };
            
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
            
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
                
                hits.Clear();
                
                int nearbyZombieAmount = 0;
                if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position,
                        zombieSpawner.ValueRO.randomWalkingDistanceMax, ref hits, filter))
                {
                    foreach (DistanceHit hit in hits)
                    {
                        if (!SystemAPI.Exists(hit.Entity))
                        {
                            continue;
                        }

                        if (SystemAPI.HasComponent<Unit>(hit.Entity) && SystemAPI.HasComponent<Zombie>(hit.Entity))
                        {
                            nearbyZombieAmount++;
                        }
                    }
                }

                if (nearbyZombieAmount >= zombieSpawner.ValueRO.nearbyZombiesAmountMax)
                {
                    continue;
                }

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