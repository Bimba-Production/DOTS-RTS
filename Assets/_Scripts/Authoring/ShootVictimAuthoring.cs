using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ShootVictimAuthoring : MonoBehaviour
    {
        private class ShootVictimAuthoringBaker : Baker<ShootVictimAuthoring>
        {
            public override void Bake(ShootVictimAuthoring authoring)
            {
            }
        }
    }
}