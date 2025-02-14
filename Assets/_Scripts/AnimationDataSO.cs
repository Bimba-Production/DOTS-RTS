using UnityEngine;

namespace _Scripts
{
    [CreateAssetMenu()]
    public class AnimationDataSO : ScriptableObject
    {
        public Mesh[] meshes;
        public float frameTimerMax;
    }
}