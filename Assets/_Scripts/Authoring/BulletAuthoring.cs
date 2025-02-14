using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class BulletAuthoring : MonoBehaviour
    {
        private class BulletAuthoringBaker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
            }
        }
    }
}