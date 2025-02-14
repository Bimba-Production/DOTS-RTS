using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class MeleeAttackAuthoring : MonoBehaviour
    {
        private class MeleeAttackAuthoringBaker : Baker<MeleeAttackAuthoring>
        {
            public override void Bake(MeleeAttackAuthoring authoring)
            {
            }
        }
    }
}