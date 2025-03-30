using _Scripts.Authoring;
using _Scripts.MonoBehaviours;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace _Scripts.Systems
{
    public partial struct UnitMoverSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            UnitMoverJob unitMoverJob = new UnitMoverJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
            };

            unitMoverJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct UnitMoverJob : IJobEntity
    {
        public float deltaTime;
        
        private void Execute(ref LocalTransform localTransform, ref UnitMover unitMover,
            ref PhysicsVelocity physicsVelocity)
        {
            float3 moveDir = unitMover.targetPosition - localTransform.Position;
            
            if (math.lengthsq(moveDir) <= GameAssets.REACHED_TARGET_POSITION_DISTANCE_SQ)
            {
                physicsVelocity.Linear = float3.zero;
                physicsVelocity.Angular = float3.zero;
                unitMover.isMoving = false;
                return;
            }
            
            unitMover.isMoving = true;
            
            moveDir = math.normalize(moveDir);

            localTransform.Rotation = 
                math.slerp(localTransform.Rotation,
                    quaternion.LookRotation(moveDir, math.up()),
                    deltaTime * unitMover.rotationSpeed);

            physicsVelocity.Linear = moveDir * unitMover.moveSpeed;
            physicsVelocity.Angular = float3.zero;
        }
    }
}