using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace _Scripts.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct ShootLightSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntitiesReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
            
            foreach ((RefRO<ShootAttack> shootAttack, RefRO<Unit> unit) in SystemAPI.Query<RefRO<ShootAttack>, RefRO<Unit>>())
            {
                if (shootAttack.ValueRO.onShoot.isTriggered)
                {
                    Entity shootLightEntity = state.EntityManager.Instantiate(entitiesReferences.shootLightPrefab);
                    SystemAPI.SetComponent(shootLightEntity, LocalTransform.FromPosition(shootAttack.ValueRO.onShoot.shootFromPosition));
                }
            }
        }
    }
}