using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts.Systems
{
    public partial struct RemoveUnitFromQueueSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeBuffers,
                         EnabledRefRW<RemoveUnitFromQueue> enabledRefRW,
                         RefRW<BuildingBarracks> buildingBarracks)
                     in SystemAPI.Query<DynamicBuffer<SpawnUnitTypeBuffer>,
                         EnabledRefRW<RemoveUnitFromQueue>,
                         RefRW<BuildingBarracks>>())
            {
                if (spawnUnitTypeBuffers.Length > 0)
                {
                    if (spawnUnitTypeBuffers.Length == 1) buildingBarracks.ValueRW.progress = 0f;
                    spawnUnitTypeBuffers.RemoveAt(spawnUnitTypeBuffers.Length - 1);
                    buildingBarracks.ValueRW.onUnitQueueChanged = true;
                }

                enabledRefRW.ValueRW = false;
            }
        }
    }
}