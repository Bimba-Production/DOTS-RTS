using _Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace _Scripts.Systems
{
    [UpdateBefore(typeof(ActiveAnimationSystem))]
    public partial struct ChangeAnimationSystem : ISystem
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
            
            ChangeAnimationJob changeAnimationJob = new ChangeAnimationJob
            {
                animationsData = animationDataHolder.animationsData,
            };

            changeAnimationJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct  ChangeAnimationJob: IJobEntity
    {
        public BlobAssetReference<BlobArray<AnimationData>> animationsData;
        
        private void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
        {
            if (AnimationDataSO.IsAnimationUninterruptible(activeAnimation.activeAnimationType)) return;

            if (activeAnimation.activeAnimationType != activeAnimation.nextAnimationType)
            {
                activeAnimation.frame = 0;
                activeAnimation.frameTimer = 0f;
                activeAnimation.activeAnimationType = activeAnimation.nextAnimationType;
                    
                ref AnimationData animationData = ref animationsData.Value[(int)activeAnimation.activeAnimationType];
                materialMeshInfo.Mesh = animationData.intMeshIdBlobArray[0];
            }
        }
    }
}