using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    [CreateAssetMenu()]
    public class AnimationDataListSO : ScriptableObject
    {
        public List<AnimationDataSO> animationDataList;

        public AnimationDataSO GetAnimationDataSO(AnimationType animationType)
        {
            foreach (AnimationDataSO animationDataSo in animationDataList)
            {
                if (animationDataSo.animationType == animationType) return animationDataSo;
            }
            Debug.LogError($"AnimationDataListSO animationDataList is null for animationType - {animationType}");
            return null;
        }
    }
}