using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class AnimationDataHolderAuthoring : MonoBehaviour
    {
        public AnimationDataListSO animationDataListSO;
        public Material defaultMaterial;
        
        public class Baker : Baker<AnimationDataHolderAuthoring>
        {
            public override void Bake(AnimationDataHolderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AnimationDataHolder animationDataHolder = new AnimationDataHolder();

                int index = 0;
                foreach (AnimationType animationType in System.Enum.GetValues(typeof(AnimationType)))
                {
                    AnimationDataSO animationDataSO = authoring.animationDataListSO.GetAnimationDataSO(animationType);
                    
                    for (int i = 0; i < animationDataSO.meshes.Length; i++)
                    {
                        Mesh mesh = animationDataSO.meshes[i];

                        Entity additionalEntity = CreateAdditionalEntity(TransformUsageFlags.None, true);
                        
                        AddComponent(additionalEntity, new MaterialMeshInfo());
                        AddComponent(additionalEntity, new RenderMeshUnmanaged
                        {
                            materialForSubMesh = authoring.defaultMaterial,
                            mesh = mesh,
                        });
                        AddComponent(additionalEntity, new AnimationDataHolderSubEntity
                        {
                            animationType = animationType,
                            meshIndex = i,
                        });
                    }

                    index++;
                }
                
                AddComponent(entity, new AnimationDataHolderObjectData
                {
                    animationDataListSO = authoring.animationDataListSO,
                });
                AddComponent(entity, animationDataHolder);
            }
        }
    }

    public struct AnimationDataHolderObjectData : IComponentData
    {
        public UnityObjectRef<AnimationDataListSO> animationDataListSO;
    }
    
    public struct AnimationDataHolderSubEntity: IComponentData
    {
        public AnimationType animationType;
        public int meshIndex;
    }
    
    public struct AnimationDataHolder : IComponentData
    {
        public BlobAssetReference<BlobArray<AnimationData>> animationsData;
    }

    public struct AnimationData
    {
        public float frameTimerMax;
        public int frameMax;
        public BlobArray<int> intMeshIdBlobArray;
    }
}