using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ShootAttackAuthoring : MonoBehaviour
    {
        private class ShootAttackAuthoringBaker : Baker<ShootAttackAuthoring>
        {
            public override void Bake(ShootAttackAuthoring authoring)
            {
            }
        }
    }
}