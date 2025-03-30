﻿using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts.Systems
{
    public partial struct ShootLightDestroySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach ((RefRW<ShootLight> shootLight, Entity entity) in SystemAPI.Query<RefRW<ShootLight>>().WithEntityAccess())
            {
                shootLight.ValueRW.timer += SystemAPI.Time.DeltaTime;

                if (shootLight.ValueRO.timer > shootLight.ValueRO.timerMax)
                {
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}