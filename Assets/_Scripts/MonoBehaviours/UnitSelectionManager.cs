﻿using System;
using _Scripts.Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace _Scripts.MonoBehaviours
{
    public class UnitSelectionManager : MonoBehaviour 
    {
        [SerializeField] private ParticleSystem _moveMarker;
        [SerializeField] private ParticleSystem _attackMoveMarker;
        public static UnitSelectionManager Instance { get; private set; }


        public event EventHandler OnSelectionAreaStart;
        public event EventHandler OnSelectionAreaEnd;


        private Vector2 selectionStartMousePosition;


        private void Awake() {
            Instance = this;
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                selectionStartMousePosition = Input.mousePosition;

                OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
            }
            if (Input.GetMouseButtonUp(0)) {
                Vector2 selectionEndMousePosition = Input.mousePosition;

                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);

                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
                
                for (int i = 0; i < entityArray.Length; i++) {
                    entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
                    Selected selected = selectedArray[i];
                    selected.onDeselected = true;
                    entityManager.SetComponentData(entityArray[i], selected);
                }
                
                Rect selectionAreaRect = GetSelectionAreaRect();
                float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
                float multipleSelectionSizeMin = 40f;
                bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

                if (isMultipleSelection) {
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
                            CollidesWith = 1u << GameAssets.UNITS_LAYER,
                            GroupIndex = 0,
                        }
                    };
                    if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit)) {
                        if (entityManager.HasComponent<Unit>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity)) {
                            // Hit a Unit
                            entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                            Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                            selected.onSelected = true;
                            entityManager.SetComponentData(raycastHit.Entity, selected);
                        }
                    }
                }


                OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.A)) {
                Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, UnitMover>().WithPresent<MoveOverride>().Build(entityManager);

                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<MoveOverride> moveOverrideArray = entityQuery.ToComponentDataArray<MoveOverride>(Allocator.Temp);
                NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
                NativeArray<float3> movePositionArray = GenerateMovePositionArray(mouseWorldPosition, entityArray.Length);

                if (Input.GetKeyDown(KeyCode.A))
                {
                    OnSingleUnitClicked(entityManager, out var isAttackingSingleTarget);

                    if (!isAttackingSingleTarget)
                    {
                        LoseTargetOverrides(entityManager);
                        
                        for (int i = 0; i < unitMoverArray.Length; i++) {
                            UnitMover unitMover = unitMoverArray[i];
                            unitMover.targetPosition = movePositionArray[i];
                            unitMoverArray[i] = unitMover;
                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);
                        }
             
                        entityQuery.CopyFromComponentDataArray(unitMoverArray);
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

                    if (!isAttackingSingleTarget)
                    {
                        LoseTargetOverrides(entityManager);
                        
                        for (int i = 0; i < moveOverrideArray.Length; i++) {
                            MoveOverride moveOverride = moveOverrideArray[i];
                            moveOverride.targetPosition = movePositionArray[i];
                            moveOverrideArray[i] = moveOverride;
                            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], true);
                        }
                    
                        entityQuery.CopyFromComponentDataArray(moveOverrideArray);
                    }
                    
                    if (unitMoverArray.Length > 0)
                    { 
                        _moveMarker.transform.position = mouseWorldPosition + new Vector3(0f, 0.1f, 0f);
                        _moveMarker.Play();
                    }
                }
            }
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

                entityManager.SetComponentEnabled<MoveOverride>(selectedEntityArray[i], false);
            }

            selectedEntityQuery.CopyFromComponentDataArray(selectedTargetOverrideArray);
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
                    CollidesWith = 1u << GameAssets.UNITS_LAYER,
                    GroupIndex = 0,
                }
            };

            isAttackingSingleTarget = false;

            if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
            {
                if (entityManager.HasComponent<Unit>(raycastHit.Entity))
                {
                    // Hit a Unit
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

                        entityManager.SetComponentEnabled<MoveOverride>(selectedEntityArray[i], false);
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
                int ringPositionCount = 3 + ring * 2;

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
    }
}