using System;
using _Scripts.Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.MonoBehaviours
{
    public class UnitSelectionManager : MonoBehaviour 
    {
        [SerializeField] private ParticleSystem _moveMarker;
        [SerializeField] private ParticleSystem _attackMoveMarker;
        public static UnitSelectionManager Instance { get; private set; }
        
        public event EventHandler OnSelectionAreaStart;
        public event EventHandler OnSelectionAreaEnd;
        public event EventHandler OnSelectionEntitiesChanged;
        public event EventHandler OnWorkerSelected;
        public event EventHandler OnWorkerDeselected;
        public event EventHandler OnTargetPositionsBufferChanged;
        public event EventHandler OnTargetPositionsBufferCleared;

        private NativeArray<Entity> selectedBarracksArray;

        private Vector2 selectionStartMousePosition;


        private void Awake() {
            Instance = this;
        }

        private void Update() {
            if (!BuildingPlacementManager.Instance.GetActiveBuildingTypeSO().IsNone) return;
            
            if (EventSystem.current.IsPointerOverGameObject())
            {
                OnPointerOverGameObject();
                return;
            }
            
            if (!Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0)) {
                selectionStartMousePosition = Input.mousePosition;

                OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
            }
            
            if (!Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonUp(0)) OnMouseButtonUp();
            
            if (Input.GetKeyDown(KeyCode.Escape)) OnESC();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
            {
                DeselectAllUnits();
                SelectUnitsUsingControl();
            }
            
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.A)) OnAttack();
        }

        private void LoseTargetOverrides(EntityManager entityManager)
        {
            EntityQuery selectedEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>()
                .WithPresent<TargetOverride>().Build(entityManager);

            NativeArray<Entity> selectedEntityArray = selectedEntityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<TargetOverride> selectedTargetOverrideArray = selectedEntityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
            
            for (int i = 0; i < selectedEntityArray.Length; i++)
            {
                TargetOverride targetOverride = selectedTargetOverrideArray[i];
                targetOverride.targetEntity = Entity.Null;
                selectedTargetOverrideArray[i] = targetOverride;

                if (entityManager.HasComponent<MoveOverride>(selectedEntityArray[i]))
                {
                    entityManager.SetComponentEnabled<MoveOverride>(selectedEntityArray[i], false);
                }
            }

            selectedEntityQuery.CopyFromComponentDataArray(selectedTargetOverrideArray);
        }

        private void SelectUnitsUsingControl()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery physicsEntityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
            PhysicsWorldSingleton physicsWorldSingleton = physicsEntityQuery.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                GroupIndex = 0,
            };
            
            RaycastInput raycastInput = new RaycastInput
            {
                Start = cameraRay.GetPoint(0f),
                End = cameraRay.GetPoint(9999f),
                Filter = collisionFilter,
            };
            
            if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
            {
                if (entityManager.HasComponent<Faction>(raycastHit.Entity)
                    && entityManager.HasComponent<Selected>(raycastHit.Entity))
                {
                    LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(raycastHit.Entity);
                    NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);
                    
                    if (entityManager.HasComponent<UnitTypeHolder>(raycastHit.Entity))
                    {
                        if (collisionWorld.OverlapSphere(localTransform.Position, GameAssets.CONTROL_DISTANCE, ref distanceHits, collisionFilter))
                        {
                            UnitTypeHolder raycastHitUnitTypeHolder = entityManager.GetComponentData<UnitTypeHolder>(raycastHit.Entity);
                            Faction raycastHitFaction = entityManager.GetComponentData<Faction>(raycastHit.Entity);
                        
                            foreach (DistanceHit distanceHit in distanceHits)
                            {
                                if (entityManager.HasComponent<Faction>(distanceHit.Entity)
                                    && entityManager.HasComponent<UnitTypeHolder>(distanceHit.Entity)
                                    && entityManager.HasComponent<Selected>(distanceHit.Entity))
                                {
                                    UnitTypeHolder distanceHitUnitTypeHolder = entityManager.GetComponentData<UnitTypeHolder>(distanceHit.Entity);
                                    Faction distanceHitFaction = entityManager.GetComponentData<Faction>(distanceHit.Entity);

                                    if (raycastHitUnitTypeHolder.unitType == distanceHitUnitTypeHolder.unitType
                                        && raycastHitFaction.factionType == distanceHitFaction.factionType)
                                    {
                                        Selected selected = entityManager.GetComponentData<Selected>(distanceHit.Entity);
                                        selected.onSelected = true;
                                        entityManager.SetComponentData(distanceHit.Entity, selected);
                                        entityManager.SetComponentEnabled<Selected>(distanceHit.Entity, true);
                                    }
                                }
                            }
                        }
                    }
                    else if (entityManager.HasComponent<BuildingTypeSOHolder>(raycastHit.Entity))
                    {
                        if (collisionWorld.OverlapSphere(localTransform.Position, GameAssets.CONTROL_DISTANCE, ref distanceHits, collisionFilter))
                        {
                            BuildingTypeSOHolder raycastHitBuildingTypeHolder = entityManager.GetComponentData<BuildingTypeSOHolder>(raycastHit.Entity);
                            Faction raycastHitFaction = entityManager.GetComponentData<Faction>(raycastHit.Entity);
                        
                            foreach (DistanceHit distanceHit in distanceHits)
                            {
                                if (entityManager.HasComponent<Faction>(distanceHit.Entity)
                                    && entityManager.HasComponent<BuildingTypeSOHolder>(distanceHit.Entity)
                                    && entityManager.HasComponent<Selected>(distanceHit.Entity))
                                {
                                    BuildingTypeSOHolder distanceHitBuildingTypeHolder = entityManager.GetComponentData<BuildingTypeSOHolder>(distanceHit.Entity);
                                    Faction distanceHitFaction = entityManager.GetComponentData<Faction>(distanceHit.Entity);

                                    if (raycastHitBuildingTypeHolder.buildingType == distanceHitBuildingTypeHolder.buildingType
                                        && raycastHitFaction.factionType == distanceHitFaction.factionType)
                                    {
                                        Selected selected = entityManager.GetComponentData<Selected>(distanceHit.Entity);
                                        selected.onSelected = true;
                                        entityManager.SetComponentData(distanceHit.Entity, selected);
                                        entityManager.SetComponentEnabled<Selected>(distanceHit.Entity, true);
                                    }
                                }
                            }
                        }
                    }
                    
                    OnSelectionEntitiesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
        private void OnSingleUnitClicked(EntityManager entityManager, out bool isAttackingSingleTarget)
        {
            EntityQuery physicsEntityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
            PhysicsWorldSingleton physicsWorldSingleton = physicsEntityQuery.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastInput raycastInput = new RaycastInput
            {
                Start = cameraRay.GetPoint(0f),
                End = cameraRay.GetPoint(9999f),
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                    GroupIndex = 0,
                }
            };

            isAttackingSingleTarget = false;

            if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
            {
                if (entityManager.HasComponent<Faction>(raycastHit.Entity))
                {
                    // Hit a Unit or Object
                    EntityQuery selectedEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>()
                        .WithPresent<TargetOverride>().Build(entityManager);

                    NativeArray<Entity> selectedEntityArray = selectedEntityQuery.ToEntityArray(Allocator.Temp);
                    NativeArray<TargetOverride> selectedTargetOverrideArray = selectedEntityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);

                    isAttackingSingleTarget = true;

                    for (int i = 0; i < selectedEntityArray.Length; i++)
                    {
                        if (raycastHit.Entity == selectedEntityArray[i]) continue;

                        TargetOverride targetOverride = selectedTargetOverrideArray[i];
                        targetOverride.targetEntity = raycastHit.Entity;
                        selectedTargetOverrideArray[i] = targetOverride;

                        if (entityManager.HasComponent<MoveOverride>(selectedEntityArray[i]))
                        {
                            entityManager.SetComponentEnabled<MoveOverride>(selectedEntityArray[i], false);
                        }
                        
                        ClearWorkerBuildingTarget(entityManager, selectedEntityArray[i]);
                    }

                    selectedEntityQuery.CopyFromComponentDataArray(selectedTargetOverrideArray);
                }
            }
        }

        public Rect GetSelectionAreaRect() {
            Vector2 selectionEndMousePosition = Input.mousePosition;

            Vector2 lowerLeftCorner = new Vector2(
                Mathf.Min(selectionStartMousePosition.x, selectionEndMousePosition.x),
                Mathf.Min(selectionStartMousePosition.y, selectionEndMousePosition.y));

            Vector2 upperRightCorner = new Vector2(
                Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x),
                Mathf.Max(selectionStartMousePosition.y, selectionEndMousePosition.y));

            return new Rect(
                lowerLeftCorner.x,
                lowerLeftCorner.y,
                upperRightCorner.x - lowerLeftCorner.x,
                upperRightCorner.y - lowerLeftCorner.y
            );
        }

        private void DeselectAllUnits()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
            
            for (int i = 0; i < entityArray.Length; i++) {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
                Selected selected = selectedArray[i];
                selected.onDeselected = true;
                entityManager.SetComponentData(entityArray[i], selected);

                if (BuildingPlacementManager.Instance.workers.Length > 0)
                {
                    BuildingPlacementManager.Instance.workers.Clear();
                    OnWorkerDeselected?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void OnESC()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            Entity entity = GetMostLoadedBarrackEntity();
            if (entity == Entity.Null) return;
            entityManager.SetComponentEnabled<RemoveUnitFromQueue>(entity, true);
        }

        private Entity GetMostLoadedBarrackEntity()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, BuildingBarracks>()
                .WithPresent<RemoveUnitFromQueue>().Build(entityManager);

            NativeArray<Entity> selectedEntityArray = entityQuery.ToEntityArray(Allocator.Temp);
            
            int maxLength = 0;
            Entity result = Entity.Null;
            foreach (Entity entity in selectedEntityArray)
            {
                DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeBuffers = entityManager.GetBuffer<SpawnUnitTypeBuffer>(entity);

                if (maxLength < spawnUnitTypeBuffers.Length)
                {
                    maxLength = spawnUnitTypeBuffers.Length;
                    result = entity;
                }
            }
            
            return result;
        }

        private void OnPointerOverGameObject()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            
                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                
                Rect selectionAreaRect = GetSelectionAreaRect();
                float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
                float multipleSelectionSizeMin = 40f;
                bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

                if (selectionStartMousePosition == new Vector2(-1000, -1000)) isMultipleSelection = false;
                
                if (isMultipleSelection) {
                    DeselectAllUnits();

                    entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

                    entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                    NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                    for (int i = 0; i < localTransformArray.Length; i++) {
                        LocalTransform unitLocalTransform = localTransformArray[i];
                        Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);
                        if (selectionAreaRect.Contains(unitScreenPosition)) {
                            // Unit is inside the selection area
                            entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                            Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                            selected.onSelected = true;
                            entityManager.SetComponentData(entityArray[i], selected);
                        }
                    }
                    
                    OnSelectionEntitiesChanged?.Invoke(this, EventArgs.Empty);
                }
                
                OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }
        
        private void OnMouseButtonUp()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);

            if (!BuildingPlacementManager.Instance.IsActive) DeselectAllUnits();
            
            if (selectionStartMousePosition == new Vector2(-1000, -1000)) return;
            
            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            if (isMultipleSelection) {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

                DeselectAllUnits();

                for (int i = 0; i < localTransformArray.Length; i++) {
                    LocalTransform unitLocalTransform = localTransformArray[i];
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);
                    if (selectionAreaRect.Contains(unitScreenPosition)) {
                        // Unit is inside the selection area
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                        selected.onSelected = true;
                        entityManager.SetComponentData(entityArray[i], selected);

                        if (entityManager.HasComponent<Worker>(entityArray[i]))
                        {
                            BuildingPlacementManager.Instance.workers.Add(entityArray[i]);
                        }
                    }
                    
                    if (BuildingPlacementManager.Instance.workers.Length > 0) OnWorkerSelected?.Invoke(this, EventArgs.Empty);
                }
            } else {
                // Single select
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastInput raycastInput = new RaycastInput {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                        GroupIndex = 0,
                    }
                };
                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit)) {
                    if (entityManager.HasComponent<Selected>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity)) {
                        DeselectAllUnits();
                        
                        // Hit a Selectable entity
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.onSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity, selected);

                        if (entityManager.HasComponent<Worker>(raycastHit.Entity))
                        {
                            BuildingPlacementManager.Instance.workers.Add(raycastHit.Entity);
                            OnWorkerSelected?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
            }
            
            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
            OnSelectionEntitiesChanged?.Invoke(this, EventArgs.Empty);
            selectionStartMousePosition = new Vector2(-1000, -1000);
        }

        private void OnAttack()
        {
            if (BuildingPlacementManager.Instance.IsActive) BuildingPlacementManager.Instance.ClearBuildCommand();
            
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (Input.GetKeyDown(KeyCode.A))
            {
                OnSingleUnitClicked(entityManager, out var isAttackingSingleTarget);

                if (isAttackingSingleTarget) return;
                
                EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, UnitMover>().WithPresent<MoveOverride>().Build(entityManager);

                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
                NativeArray<float3> movePositionArray = GenerateMovePositionArray(mouseWorldPosition, entityArray.Length);
                
                if (!isAttackingSingleTarget)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        for (int i = 0; i < unitMoverArray.Length; i++) {
                            AddToMovePointsBuffer(entityManager, entityArray[i], movePositionArray[i]);
                            
                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);
                            
                            ClearWorkerBuildingTarget(entityManager, entityArray[i]);
                        }
                    }
                    else
                    {
                        LoseTargetOverrides(entityManager);
                    
                        for (int i = 0; i < unitMoverArray.Length; i++) {
                            UnitMover unitMover = unitMoverArray[i];
                            ClearMovePointsBuffer(entityManager, entityArray[i]);
                            
                            unitMover.targetPosition = movePositionArray[i];
                            unitMoverArray[i] = unitMover;
                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);
                            
                            ClearWorkerBuildingTarget(entityManager, entityArray[i]);
                        }
         
                        entityQuery.CopyFromComponentDataArray(unitMoverArray);
                    }
                }
                
                if (unitMoverArray.Length > 0)
                { 
                    _attackMoveMarker.transform.position = mouseWorldPosition + new Vector3(0f, 0.1f, 0f);
                    _attackMoveMarker.Play();
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                OnSingleUnitClicked(entityManager, out var isAttackingSingleTarget);

                if (isAttackingSingleTarget) return;
                
                EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, UnitMover>().WithPresent<MoveOverride>().Build(entityManager);

                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<MoveOverride> moveOverrideArray = entityQuery.ToComponentDataArray<MoveOverride>(Allocator.Temp);
                NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
                NativeArray<float3> movePositionArray = GenerateMovePositionArray(mouseWorldPosition, entityArray.Length);
                
                if (!isAttackingSingleTarget)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        for (int i = 0; i < unitMoverArray.Length; i++) {
                            AddToMovePointsBuffer(entityManager, entityArray[i], movePositionArray[i]);

                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);
                            
                            ClearWorkerBuildingTarget(entityManager, entityArray[i]);
                        }
         
                        entityQuery.CopyFromComponentDataArray(unitMoverArray);
                    }
                    else
                    {
                        LoseTargetOverrides(entityManager);
                    
                        for (int i = 0; i < moveOverrideArray.Length; i++) {
                            ClearMovePointsBuffer(entityManager, entityArray[i]);
                            
                            MoveOverride moveOverride = moveOverrideArray[i];
                            moveOverride.targetPosition = movePositionArray[i];
                            moveOverrideArray[i] = moveOverride;
                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], true);
                            ClearWorkerBuildingTarget(entityManager, entityArray[i]);
                        }
                
                        entityQuery.CopyFromComponentDataArray(moveOverrideArray);
                    }
                }
                
                // Handle Barracks Rally Position
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, RallyPositionOffset, LocalTransform>().Build(entityManager);

                NativeArray<Entity> rallyEntityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<RallyPositionOffset> rallyPositionOffsets = entityQuery.ToComponentDataArray<RallyPositionOffset>(Allocator.Temp);
                NativeArray<LocalTransform> localTransforms = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                
                for (int i = 0; i < rallyPositionOffsets.Length; i++) {
                    RallyPositionOffset rallyPositionOffset = rallyPositionOffsets[i];
                    LocalTransform localTransform =
                        entityManager.GetComponentData<LocalTransform>(rallyPositionOffset.pointer);

                    localTransform.Position = (float3)mouseWorldPosition - localTransforms[i].Position;
                    rallyPositionOffset.value = localTransform.Position + localTransforms[i].Position;
                    
                    entityManager.SetComponentData(rallyPositionOffset.pointer, localTransform);
                    entityManager.SetComponentData(rallyEntityArray[i], rallyPositionOffset);
                }
                
                // entityQuery.CopyFromComponentDataArray(rallyPositionOffsets);
                
                if (unitMoverArray.Length > 0)
                { 
                    _moveMarker.transform.position = mouseWorldPosition + new Vector3(0f, 0.1f, 0f);
                    _moveMarker.Play();
                }
            }
        }
        
        private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount) {
            NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);
            if (positionCount == 0) {
                return positionArray;
            }
            positionArray[0] = targetPosition;
            if (positionCount == 1) {
                return positionArray;
            }

            float ringSize = 2.2f;
            int ring = 0;
            int positionIndex = 1;

            while (positionIndex < positionCount) {
                int ringPositionCount = 5 + ring * 2;

                for (int i = 0; i < ringPositionCount; i++) {
                    float angle = i * (math.PI2 / ringPositionCount);
                    float3 ringVector = math.rotate(quaternion.RotateY(angle), new float3(ringSize * (ring + 1), 0, 0));
                    float3 ringPosition = targetPosition + ringVector;

                    positionArray[positionIndex] = ringPosition;
                    positionIndex++;

                    if (positionIndex >= positionCount) {
                        break;
                    }
                }
                ring++;
            }

            return positionArray;
        }

        private void ClearWorkerBuildingTarget(EntityManager entityManager, Entity entity)
        {
            if (entityManager.HasComponent<Worker>(entity))
            {
                Worker worker = entityManager.GetComponentData<Worker>(entity);
                if (worker.buildingToBuild != Entity.Null)
                {
                    worker.buildingToBuild = Entity.Null;
                    entityManager.SetComponentData(entity, worker);
                }
            }
        }

        private void ClearMovePointsBuffer(EntityManager entityManager, Entity entity)
        {
            DynamicBuffer<CommandBuffer> commandBuffer = entityManager.GetBuffer<CommandBuffer>(entity);
            
            commandBuffer.Clear();
            
            OnTargetPositionsBufferCleared?.Invoke(this, EventArgs.Empty);
        }

        private void AddToMovePointsBuffer(EntityManager entityManager, Entity entity, float3 point)
        {
            DynamicBuffer<CommandBuffer> commandBuffer = entityManager.GetBuffer<CommandBuffer>(entity);
            
            commandBuffer.Add( new CommandBuffer
            {
                point = point,
                value = CommandType.Reach,
            });
                            
            OnTargetPositionsBufferChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public void ClearPositionsBuffer()
        {
            OnTargetPositionsBufferCleared?.Invoke(this, EventArgs.Empty);
        }
    }
}