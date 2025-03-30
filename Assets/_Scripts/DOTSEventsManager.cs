using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace _Scripts
{
    public class DOTSEventsManager : MonoBehaviour
    {
        public static DOTSEventsManager Instance {get; private set;}

        public event EventHandler OnBarracksUnitQueueChanged;
        
        private void Awake()
        {
            Instance = this;
        }

        public void TriggerOnBarracksUnitQueueChanged(NativeList<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                OnBarracksUnitQueueChanged?.Invoke(entity, EventArgs.Empty);
            }
        }
    }
}