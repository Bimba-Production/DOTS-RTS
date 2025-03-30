using _Scripts.Authoring;
using _Scripts.MonoBehaviours;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct BuildingBarracksSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntitiesReferences>();
        }

        public void OnUpdate(ref SystemState state)
        {
            EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

            foreach ((RefRW<BuildingBarracks> buildingBarracks,
                         DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeBuffers,
                         RefRO<BuildingBarracksUnitEnqueue> buildingBarracksUnitEnqueue,
                         EnabledRefRW<BuildingBarracksUnitEnqueue> buildingBarracksUnitEnqueueEnabled)
                     in SystemAPI.Query<RefRW<BuildingBarracks>,
                         DynamicBuffer<SpawnUnitTypeBuffer>,
                         RefRO<BuildingBarracksUnitEnqueue>,
                         EnabledRefRW<BuildingBarracksUnitEnqueue>>())
            {
                spawnUnitTypeBuffers.Add(new SpawnUnitTypeBuffer
                {
                    unitType = buildingBarracksUnitEnqueue.ValueRO.unitType
                });

                buildingBarracksUnitEnqueueEnabled.ValueRW = false;
                
                buildingBarracks.ValueRW.onUnitQueueChanged = true;
            }
            
            foreach ((RefRO<LocalTransform> localTransform, RefRW<BuildingBarracks> buildingBarracks,
                         RefRO<RallyPositionOffset> rallyPositionOffset, DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeBuffers) 
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRW<BuildingBarracks>,
                         RefRO<RallyPositionOffset>, DynamicBuffer<SpawnUnitTypeBuffer>>())
            {
                if (spawnUnitTypeBuffers.IsEmpty)
                {
                    continue;
                }

                if (buildingBarracks.ValueRO.activeUnitType != spawnUnitTypeBuffers[0].unitType)
                {
                    buildingBarracks.ValueRW.activeUnitType = spawnUnitTypeBuffers[0].unitType;
                    UnitTypeSO activeUnitTypeSO = GameAssets.Instance.unitTypeListSO.GetUnitTypeSO(buildingBarracks.ValueRO.activeUnitType);

                    buildingBarracks.ValueRW.progressMax = activeUnitTypeSO.progressMax;
                }
                
                buildingBarracks.ValueRW.progress += SystemAPI.Time.DeltaTime;

                if (buildingBarracks.ValueRO.progress < buildingBarracks.ValueRO.progressMax)
                {
                    continue;
                }
                
                buildingBarracks.ValueRW.progress = 0f;

                UnitType unitType = spawnUnitTypeBuffers[0].unitType;
                UnitTypeSO unitTypeSO = GameAssets.Instance.unitTypeListSO.GetUnitTypeSO(unitType);
                spawnUnitTypeBuffers.RemoveAt(0);
                buildingBarracks.ValueRW.onUnitQueueChanged = true;

                Entity spawnedUnitEntity = state.EntityManager.Instantiate(unitTypeSO.GetPrefabEntity(entitiesReferences));
                SystemAPI.SetComponent(spawnedUnitEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));
                RefRW<UnitMover> unitMover = SystemAPI.GetComponentRW<UnitMover>(spawnedUnitEntity);
                unitMover.ValueRW.targetPosition = rallyPositionOffset.ValueRO.value;
            }
        }
    }
}