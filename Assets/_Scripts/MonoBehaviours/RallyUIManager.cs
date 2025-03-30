using _Scripts.Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace _Scripts.MonoBehaviours
{
    public class RallyUIManager : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private GameObject _lineRendererContainer;
        
        private Entity _targetEntity = Entity.Null;
        private EntityManager _entityManager;
        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            UnitSelectionManager.Instance.OnSelectionEntitiesChanged += UnitSelectionManager_OnSelectionEntitiesChanged;
            UnitSelectionManager.Instance.OnTargetPositionsBufferChanged += UnitSelectionManager_OnSelectionEntitiesChanged;
            BuildingPlacementManager.Instance.OnTargetPositionsBufferChanged += UnitSelectionManager_OnSelectionEntitiesChanged;
            UnitSelectionManager.Instance.OnTargetPositionsBufferCleared += UnitSelectionManager_OnTargetPositionsBufferCleared;

            HideLine();
        }
        
        private void Update()
        {
            if (_targetEntity == Entity.Null) return;

            UpdateVisuals();
        }

        private void UnitSelectionManager_OnSelectionEntitiesChanged(object sender, System.EventArgs e)
        {
            UpdateTarget();
        }
        
        private void UnitSelectionManager_OnTargetPositionsBufferCleared(object sender, System.EventArgs e)
        {
            _targetEntity = Entity.Null;
            BuildingPlacementManager.Instance.IsActive = false;
            HideLine();
        }
        
        private void ShowLine()
        {
            _lineRendererContainer.SetActive(true);
        }

        private void HideLine()
        {
            _lineRendererContainer.SetActive(false);
            
            _lineRenderer.positionCount = 0;
        }

        private void UpdateVisuals()
        {
            if (_targetEntity == Entity.Null || !_entityManager.HasComponent<LocalTransform>(_targetEntity)) _targetEntity = GetTarget();
            
            if (_targetEntity == Entity.Null || !_entityManager.HasComponent<LocalTransform>(_targetEntity)) return;
            
            LocalTransform localTransform = _entityManager.GetComponentData<LocalTransform>(_targetEntity);
            UnitMover unitMover = _entityManager.GetComponentData<UnitMover>(_targetEntity);

            DynamicBuffer<CommandBuffer> commandBuffer =
                _entityManager.GetBuffer<CommandBuffer>(_targetEntity, true);
            
            NativeArray<Vector3> positions = new NativeArray<Vector3>(commandBuffer.Length + 2, Allocator.Temp);
            
            positions[0] = localTransform.Position;

            positions[1] = unitMover.targetPosition;
            
            for (int i = 0; i < commandBuffer.Length; i++)
            {
                positions[i + 2] = commandBuffer[i].point;
            }

            DrawLine(positions);
        }
        
        private void DrawLine(NativeArray<Vector3> positions)
        {
            _lineRenderer.positionCount = positions.Length;
            Vector3[] positionsArray = positions.ToArray();
            _lineRenderer.SetPositions(positionsArray);
        }

        private void UpdateTarget()
        {
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, UnitMover>().Build(_entityManager);
            
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Persistent);

            foreach (Entity entity in entityArray)
            {
                DynamicBuffer<CommandBuffer> commandBuffer =
                    _entityManager.GetBuffer<CommandBuffer>(entity, true);

                if (!commandBuffer.IsEmpty)
                {
                    _targetEntity = entity;
                    ShowLine();
                    return;
                }
            }
            
            _targetEntity = Entity.Null;
            HideLine();
        }

        private Entity GetTarget()
        {
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, UnitMover>().Build(_entityManager);
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Persistent);

            foreach (Entity entity in entityArray)
            {
                DynamicBuffer<CommandBuffer> commandBuffer =
                    _entityManager.GetBuffer<CommandBuffer>(entity, true);

                if (!commandBuffer.IsEmpty)
                {
                    return entity;
                }
            }
            
            return Entity.Null;
        }
    }
}