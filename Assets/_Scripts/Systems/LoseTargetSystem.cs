using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct LoseTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRO<LocalTransform> localTransform, RefRW<Target> target,
                         RefRO<LoseTarget> loseTarget, RefRW<TargetOverride> targetOverride) 
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Target>,
                         RefRO<LoseTarget>, RefRW<TargetOverride>>())
            {
                if (target.ValueRO.targetEntity == Entity.Null)
                {
                    continue;
                }
                
                LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);

                if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) > loseTarget.ValueRO.loseTargetDistance)
                {
                    target.ValueRW.targetEntity = Entity.Null;
                    targetOverride.ValueRW.targetEntity = Entity.Null;
                }
            }
        }
    }
}