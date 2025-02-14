using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class SelectedAuthoring : MonoBehaviour
    {
        private class SelectedAuthoringBaker : Baker<SelectedAuthoring>
        {
            public override void Bake(SelectedAuthoring authoring)
            {
            }
        }
    }
}