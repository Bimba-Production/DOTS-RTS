using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class FriendlyAuthoring : MonoBehaviour
    {
        public class Baker : Baker<FriendlyAuthoring>
        {
            public override void Bake(FriendlyAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Friendly());
            }
        }
        
        public struct Friendly: IComponentData
        {
            
        }
    }
}