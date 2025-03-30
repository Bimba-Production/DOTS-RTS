using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace _Scripts.Systems
{
    public partial struct CommandDistributionSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<UnitMover> unitMover, RefRO<Target> target, RefRO<Friendly> friendly, Entity entity) 
                     in SystemAPI.Query<RefRW<UnitMover>, RefRO<Target>, RefRO<Friendly>>().WithPresent<MoveOverride>().WithEntityAccess())
            {
                DynamicBuffer<CommandBuffer> commandBuffer = SystemAPI.GetBuffer<CommandBuffer>(entity);
                
                if (commandBuffer.IsEmpty)
                {
                    return;
                }

                if (unitMover.ValueRO.isMoving)
                {
                    return;
                }

                if (target.ValueRO.targetEntity != Entity.Null)
                {
                    return;
                }

                CommandBuffer command = commandBuffer[0];

                switch (command.value)
                {
                    case CommandType.Reach:
                        // RefRW<MoveOverride> moveOverride = SystemAPI.GetComponentRW<MoveOverride>(entity);
                        // moveOverride.ValueRW.targetPosition = command.point;
                        // SystemAPI.SetComponentEnabled<MoveOverride>(entity, true);
                        
                        unitMover.ValueRW.targetPosition = command.point;
                        break;
                    case CommandType.Attack:
                        unitMover.ValueRW.targetPosition = command.point;
                        break;
                    case CommandType.Build:
                        DynamicBuffer<BuildingBuffer> buildingsBuffer = SystemAPI.GetBuffer<BuildingBuffer>(entity);
                        BuildingBuffer building = buildingsBuffer[0];
                        
                        unitMover.ValueRW.targetPosition = building.targetPosition;

                        RefRW<Worker> worker = SystemAPI.GetComponentRW<Worker>(entity);
                        
                        worker.ValueRW.buildingToBuild = building.buildingToBuild;
                        worker.ValueRW.buildingToDestroy = building.buildingToDestroy;
                        
                        buildingsBuffer.RemoveAt(0);
                        break;
                    case CommandType.Fix:
                    case CommandType.Get:
                    case CommandType.Patrol:
                    case CommandType.Treat:
                        break;
                }
                
                commandBuffer.RemoveAt(0);
            }
        }
    }
}