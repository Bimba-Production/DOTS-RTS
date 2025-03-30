using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct SetRallyLinePositionSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRO<LocalToWorld> buildingTransform, RefRO<LocalTransform> localTransform, RefRO<RallyPositionOffset> rallyPositionOffset)
                     in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<LocalTransform>, RefRO<RallyPositionOffset>>())
            {
                float3 buildingWorldPos = buildingTransform.ValueRO.Position;
                float3 pointerWorldPos = SystemAPI.GetComponent<LocalToWorld>(rallyPositionOffset.ValueRO.pointer).Position;

                float3 linePosition = ((pointerWorldPos + buildingWorldPos) / 2) - localTransform.ValueRO.Position;

                RefRW<LocalTransform> lineTransform = SystemAPI.GetComponentRW<LocalTransform>(rallyPositionOffset.ValueRO.line);
                lineTransform.ValueRW.Position = linePosition + new float3(0f, 0.1f, 0f);

                float3 dir = math.normalize(pointerWorldPos - buildingWorldPos + new float3(0f, 0.1f, 0f));
                lineTransform.ValueRW.Rotation = math.mul(quaternion.LookRotation(dir, math.up()), quaternion.RotateY(math.radians(90)));

                RefRW<PostTransformMatrix> linePostTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(rallyPositionOffset.ValueRO.line);
                float scale = math.distance(pointerWorldPos, buildingWorldPos);
                linePostTransformMatrix.ValueRW.Value = float4x4.Scale(scale / 10, 1f, 0.2f);
            }
        }
    }
}