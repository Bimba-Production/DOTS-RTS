using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ZonbieAuthoring : MonoBehaviour
    {
        private class ZonbieAuthoringBaker : Baker<ZonbieAuthoring>
        {
            public override void Bake(ZonbieAuthoring authoring)
            {
            }
        }
    }
}