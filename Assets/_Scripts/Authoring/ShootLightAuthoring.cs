using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ShootLightAuthoring : MonoBehaviour
    {
        private class ShootLightAuthoringBaker : Baker<ShootLightAuthoring>
        {
            public override void Bake(ShootLightAuthoring authoring)
            {
            }
        }
    }
}