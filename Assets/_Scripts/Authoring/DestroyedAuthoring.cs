using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class DestroyedAuthoring : MonoBehaviour
    {
        private class Baker : Baker<DestroyedAuthoring>
        {
            public override void Bake(DestroyedAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Destroyed());
                
                SetComponentEnabled<Destroyed>(entity, false);
            }
        }
    }
    
    public struct Destroyed : IComponentData, IEnableableComponent
    {
    }
}