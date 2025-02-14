using _Scripts.Authoring;
using _Scripts.MonoBehaviours;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct RandomWalkingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<RandomWalking> randomWalking, RefRW<UnitMover> unitMover, RefRO<LocalTransform> localTransform) 
                     in SystemAPI.Query<RefRW<RandomWalking>, RefRW<UnitMover>, RefRO<LocalTransform>>())
            {
                if (math.distancesq(localTransform.ValueRO.Position, randomWalking.ValueRO.targetPosition) < GameAssets.REACHED_TARGET_POSITION_DISTANCE_SQ)
                {
                    float3 randomDirection = new float3(randomWalking.ValueRW.random.NextFloat(-1f, 1f), 0, randomWalking.ValueRW.random.NextFloat(-1f, +1f));
                    
                    randomWalking.ValueRW.targetPosition = randomWalking.ValueRO.originPosition + randomDirection 
                        * randomWalking.ValueRW.random.NextFloat(randomWalking.ValueRO.distanceMin, randomWalking.ValueRO.distanceMax);
                }
                else
                {
                    unitMover.ValueRW.targetPosition = randomWalking.ValueRO.targetPosition;
                }
            }
        }
    }
}