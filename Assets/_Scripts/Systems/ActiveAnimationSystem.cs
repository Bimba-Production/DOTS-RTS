using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace _Scripts.Systems
{
    public partial struct ActiveAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AnimationDataHolder>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            AnimationDataHolder animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();
            
            ActiveAnimationJob activeAnimationJob = new ActiveAnimationJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                animationsData = animationDataHolder.animationsData,
            };

            activeAnimationJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ActiveAnimationJob: IJobEntity
    {
        public float deltaTime;
        public BlobAssetReference<BlobArray<AnimationData>> animationsData;
        private void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
        {
            ref AnimationData animationData = ref animationsData.Value[(int)activeAnimation.activeAnimationType];
            activeAnimation.frameTimer += deltaTime;
            
            if (activeAnimation.frameTimer > animationData.frameTimerMax)
            {
                activeAnimation.frameTimer -= animationData.frameTimerMax;
                activeAnimation.frame = (activeAnimation.frame + 1) % animationData.frameMax;

                materialMeshInfo.Mesh = animationData.intMeshIdBlobArray[activeAnimation.frame];

                if (activeAnimation.frame == 0 &&
                    AnimationDataSO.IsAnimationUninterruptible(activeAnimation.activeAnimationType))
                {
                    activeAnimation.activeAnimationType = AnimationType.None;
                }
            }
        }
    }
}