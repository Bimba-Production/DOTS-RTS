using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ShootProjectileAttackAuthoring : MonoBehaviour
    {
        private class ShootProjectileAttackAuthoringBaker : Baker<ShootProjectileAttackAuthoring>
        {
            public override void Bake(ShootProjectileAttackAuthoring authoring)
            {
            }
        }
    }
}