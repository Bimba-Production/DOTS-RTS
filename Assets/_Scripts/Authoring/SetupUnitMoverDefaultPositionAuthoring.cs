using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class SetupUnitMoverDefaultPositionAuthoring : MonoBehaviour
    {
        private class
            SetupUnitMoverDefaultPositionAuthoringBaker : Baker<SetupUnitMoverDefaultPositionAuthoring>
        {
            public override void Bake(SetupUnitMoverDefaultPositionAuthoring authoring)
            {
            }
        }
    }
}