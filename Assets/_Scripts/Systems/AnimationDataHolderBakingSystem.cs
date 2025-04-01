using System.Collections.Generic;
using _Scripts.Authoring;
using Unity.Collections;
using Unity.Rendering;
using Unity.Entities;

namespace _Scripts.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    public partial struct AnimationDataHolderBakingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            AnimationDataListSO animationDataListSO = null;
            foreach (RefRO<AnimationDataHolderObjectData> animationDataHolderObjectData
                     in SystemAPI.Query<RefRO<AnimationDataHolderObjectData>>())
            {
                animationDataListSO = animationDataHolderObjectData.ValueRO.animationDataListSO.Value;
            }
            
            Dictionary<AnimationType, int[]> blobAssetDataDictionary = new Dictionary<AnimationType, int[]>();

            foreach (AnimationType animationType in System.Enum.GetValues(typeof(AnimationType)))
            {
                AnimationDataSO animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);
                blobAssetDataDictionary[animationType] = new int[animationDataSO.meshes.Length];
            }

            foreach ((RefRO<AnimationDataHolderSubEntity> animationDataHolderSubEntity, RefRO<MaterialMeshInfo> materialMeshInfo)
                     in SystemAPI.Query<RefRO<AnimationDataHolderSubEntity>, RefRO<MaterialMeshInfo>>())
            {
                blobAssetDataDictionary[animationDataHolderSubEntity.ValueRO.animationType]
                    [animationDataHolderSubEntity.ValueRO.meshIndex] = materialMeshInfo.ValueRO.Mesh;
            }
            
            foreach (RefRW<AnimationDataHolder> animationDataHolder
                     in SystemAPI.Query<RefRW<AnimationDataHolder>>())
            {
                BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
                ref BlobArray<AnimationData> animationsData = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();
                
                BlobBuilderArray<AnimationData> animationDataBlobArray = 
                                    blobBuilder.Allocate<AnimationData>(ref animationsData, System.Enum.GetValues(typeof(AnimationType)).Length);
                
                int index = 0;
                foreach (AnimationType animationType in System.Enum.GetValues(typeof(AnimationType)))
                {
                    AnimationDataSO animationDataSO = animationDataListSO.GetAnimationDataSO(animationType);
                    
                    BlobBuilderArray<int> blobBuilderArray = 
                        blobBuilder.Allocate<int>(ref animationDataBlobArray[index].intMeshIdBlobArray, animationDataSO.meshes.Length);
                    
                    animationDataBlobArray[index].frameTimerMax = animationDataSO.frameTimerMax;
                    animationDataBlobArray[index].frameMax = animationDataSO.meshes.Length;
                    
                    for (int i = 0; i < animationDataSO.meshes.Length; i++)
                    {
                        blobBuilderArray[i] = blobAssetDataDictionary[animationType][i];
                    }

                    index++;
                }
                
                animationDataHolder.ValueRW.animationsData =
                    blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);
                
                blobBuilder.Dispose();
            }
        }
    }
}