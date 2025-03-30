using _Scripts.Authoring;
using UnityEngine;

namespace _Scripts
{
    [CreateAssetMenu()]
    public class AnimationDataSO : ScriptableObject
    {
        public AnimationType animationType;
        public Mesh[] meshes;
        public float frameTimerMax;

        public static bool IsAnimationUninterruptible(AnimationType animationType)
        {
            switch (animationType)
            {
                case AnimationType.ScoutShoot:
                case AnimationType.SoldierShoot:
                case AnimationType.ZombieAttack:
                        return true;
                default:
                    return false;
            }
        }
    }
    
    public enum AnimationType
    {
        None = 0,
        SoldierIdle = 1,
        SoldierWalk = 2,
        SoldierAim = 5,
        SoldierShoot = 6,
        ZombieIdle = 3,
        ZombieWalk = 4,
        ZombieAttack = 7,
        ScoutIdle = 8,
        ScoutWalk = 9,
        ScoutAim = 10,
        ScoutShoot = 11,
        WorkerIdle = 12,
    }
}