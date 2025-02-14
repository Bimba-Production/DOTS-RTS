using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class ZombieAuthoring : MonoBehaviour
    {
        public class Baker : Baker<ZombieAuthoring>
        {
            public override void Bake(ZombieAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Zombie());
            }
        }
        
        public struct Zombie: IComponentData
        {
            
        }
    }
}