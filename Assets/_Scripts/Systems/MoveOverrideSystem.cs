using _Scripts.Authoring;
using _Scripts.MonoBehaviours;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct MoveOverrideSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRO<LocalTransform> localTransform, RefRO<MoveOverride> moveOverride,
                         EnabledRefRW<MoveOverride> enabledMoveOverride, RefRW<UnitMover> unitMover) 
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveOverride>,
                         EnabledRefRW<MoveOverride>, RefRW<UnitMover>>())
            {

                if (math.distancesq(localTransform.ValueRO.Position, moveOverride.ValueRO.targetPosition) > GameAssets.REACHED_TARGET_POSITION_DISTANCE_SQ)
                {
                    unitMover.ValueRW.targetPosition = moveOverride.ValueRO.targetPosition;
                }
                else
                {
                    enabledMoveOverride.ValueRW = false;
                }
            }
        }
    }
}