using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class EntitiesReferencesAuthoring : MonoBehaviour
    {
        private class EntitiesReferencesAuthoringBaker : Baker<EntitiesReferencesAuthoring>
        {
            public override void Bake(EntitiesReferencesAuthoring authoring)
            {
            }
        }
    }
}