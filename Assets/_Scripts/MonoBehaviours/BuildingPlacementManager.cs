using System;
using _Scripts.Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;
using Material = UnityEngine.Material;

namespace _Scripts.MonoBehaviours
{
    public class BuildingPlacementManager : MonoBehaviour
    {
        [SerializeField] private BuildingTypeSO _buildingTypeSO;
        [SerializeField] private Material _material;

        public static BuildingPlacementManager Instance { get; private set; }
        public event EventHandler OnActiveBuildingTypeSOChanged;
        public event EventHandler OnTargetPositionsBufferChanged;
        
        private Transform _ghostTransform;
        private EntityManager _entityManager;

        public bool IsActive = false;
        public NativeList<Entity> workers = new NativeList<Entity>(0, Allocator.Persistent);

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
        
        private void Update()
        {
            if (_ghostTransform != null)
            {
                _ghostTransform.position = MouseWorldPosition.Instance.GetPosition();
            }
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (_buildingTypeSO.IsNone) return;
            if (Input.GetMouseButtonDown(1))
            {
                IsActive = false;
                SetActiveBuildingTypeSO(GameAssets.Instance.buildingTypeListSO.none);
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (CanPlaceBuilding())
                {
                    Vector3 mousePosition = MouseWorldPosition.Instance.GetPosition();
                    EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                    EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(EntitiesReferences));
                    EntitiesReferences entitiesReferences = entityQuery.GetSingleton<EntitiesReferences>();

                    Entity workerEntity = GetWorker();
                    if (workerEntity != Entity.Null)    
                    {
                        Entity entity = entityManager.Instantiate(_buildingTypeSO.GetVisualPrefabEntity(entitiesReferences));
                        entityManager.SetComponentData(entity, LocalTransform.FromPosition(mousePosition));
                        
                        DynamicBuffer<BuildingBuffer> buildingBuffer = entityManager.GetBuffer<BuildingBuffer>(workerEntity);
                        DynamicBuffer<CommandBuffer> commandBuffer = entityManager.GetBuffer<CommandBuffer>(workerEntity);
                        Worker worker = entityManager.GetComponentData<Worker>(workerEntity);

                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            if (buildingBuffer.Length > 0)
                            {
                                commandBuffer.Clear();
                                for (int i = 0; i < buildingBuffer.Length; i++)
                                {
                                    BuildingBuffer building = buildingBuffer[i];
                    
                                    _entityManager.SetComponentEnabled<Destroyed>(building.buildingToDestroy, true);
                                }
                
                                buildingBuffer.Clear();
                            }
                            else if (worker.buildingToBuild != Entity.Null)
                            {
                                _entityManager.SetComponentEnabled<Destroyed>(worker.buildingToDestroy, true);
                
                                worker.buildingToBuild = Entity.Null;
                                worker.buildingToDestroy = Entity.Null;
                                _entityManager.SetComponentData(workerEntity, worker);
                            
                                StopWorkerUnit(entityManager, workerEntity);
                            }
                            else
                            {
                                StopWorkerUnit(entityManager, workerEntity);
                            }
                        }
                        
                        buildingBuffer.Add( new BuildingBuffer
                        {
                            buildingToBuild = _buildingTypeSO.GetPrefabEntity(entitiesReferences),
                            buildingToDestroy = entity,
                            targetPosition = LocalTransform.FromPosition(mousePosition).Position,
                        });
                        
                        commandBuffer.Add( new CommandBuffer
                        {
                            point = LocalTransform.FromPosition(mousePosition).Position,
                            value = CommandType.Build,
                        });  
                        
                        OnTargetPositionsBufferChanged?.Invoke(this, EventArgs.Empty);
                        
                        SetActiveBuildingTypeSO(GameAssets.Instance.buildingTypeListSO.none);
                    }
                }
            }
        }

        private Entity GetWorker()
        {
            int count = Int32.MaxValue; 
            Entity targetWorker = Entity.Null;
            
            foreach (Entity entity in workers)
            {
                DynamicBuffer<BuildingBuffer> buildingBuffer = _entityManager.GetBuffer<BuildingBuffer>(entity);

                if (buildingBuffer.Length < count)
                {
                    count = buildingBuffer.Length;
                    targetWorker = entity;
                }
            }
            
            return targetWorker;
        }

        private bool CanPlaceBuilding()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            EntityQuery physicsEntityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
            PhysicsWorldSingleton physicsWorldSingleton = physicsEntityQuery.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.BUILDINGS_LAYER,
                GroupIndex = 0,
            };

            UnityEngine.BoxCollider boxCollider = _buildingTypeSO.prefab.GetComponent<UnityEngine.BoxCollider>();
            Vector3 mousePosition = MouseWorldPosition.Instance.GetPosition();
            float bonusExtents = 1.1f;
            NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);

            if (collisionWorld.OverlapBox(mousePosition, Quaternion.identity,
                    boxCollider.size * .5f * bonusExtents, ref distanceHits, collisionFilter))
            {
                return false;
            }

            distanceHits.Clear();
            if (collisionWorld.OverlapSphere(mousePosition, _buildingTypeSO.buildingDistanceMin, ref distanceHits,
                    collisionFilter))
            {
                foreach (DistanceHit distanceHit in distanceHits)
                {
                    if (entityManager.HasComponent<BuildingTypeSOHolder>(distanceHit.Entity))
                    {
                        BuildingTypeSOHolder buildingTypeSoHolder = entityManager.GetComponentData<BuildingTypeSOHolder>(distanceHit.Entity);
                        if (buildingTypeSoHolder.buildingType == _buildingTypeSO.buildingType)
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }

        public BuildingTypeSO GetActiveBuildingTypeSO()
        {
            return _buildingTypeSO;
        }

        public void SetActiveBuildingTypeSO(BuildingTypeSO buildingTypeSO)
        {
            IsActive = true;

            _buildingTypeSO = buildingTypeSO;

            if (_ghostTransform != null)
            {
                Destroy(_ghostTransform.gameObject);
            }

            if (!_buildingTypeSO.IsNone)
            {
                _ghostTransform = Instantiate(_buildingTypeSO.visualPrefab);
                foreach (MeshRenderer meshRenderer in _ghostTransform.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRenderer.material = _material;
                }
            }
            
            OnActiveBuildingTypeSOChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ClearBuildCommand()
        {
            IsActive = false;

            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, Worker>().Build(_entityManager);
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Persistent);
            
            foreach (Entity entity in entityArray)
            {
                Worker worker = _entityManager.GetComponentData<Worker>(entity);
                DynamicBuffer<BuildingBuffer> buildingBuffer = _entityManager.GetBuffer<BuildingBuffer>(entity);

                for (int i = 0; i < buildingBuffer.Length; i++)
                {
                    BuildingBuffer building = buildingBuffer[i];
                    
                    _entityManager.SetComponentEnabled<Destroyed>(building.buildingToDestroy, true);
                }
                
                buildingBuffer.Clear();
                
                if (worker.buildingToBuild != Entity.Null)
                {
                    _entityManager.SetComponentEnabled<Destroyed>(worker.buildingToDestroy, true);
                    
                    worker.buildingToBuild = Entity.Null;
                    worker.buildingToDestroy = Entity.Null;
                    
                    _entityManager.SetComponentData(entity, worker);
                }
            }
            
            SetActiveBuildingTypeSO(GameAssets.Instance.buildingTypeListSO.none);
        }

        private void StopWorkerUnit(EntityManager entityManager, Entity entity)
        {
            UnitMover unitMover = entityManager.GetComponentData<UnitMover>(entity);
            LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(entity);
            unitMover.targetPosition = localTransform.Position;
                            
            entityManager.SetComponentData(entity, unitMover);
            entityManager.SetComponentEnabled<MoveOverride>(entity, false);
        }
    }
}