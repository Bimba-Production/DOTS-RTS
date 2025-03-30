using Unity.Entities;
using UnityEngine;

namespace _Scripts.Authoring
{
    public class WorkerAuthoring : MonoBehaviour
    {
        private class WorkerAuthoringBaker : Baker<WorkerAuthoring>
        {
            public override void Bake(WorkerAuthoring authoring)
            {
            }
        }
    }
}