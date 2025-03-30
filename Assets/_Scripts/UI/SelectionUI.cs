using System;
using System.Collections.Generic;
using _Scripts.Authoring;
using _Scripts.MonoBehaviours;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class SelectionUI : MonoBehaviour
    {
        private Color HEALTH_BROWN = ColorUtility.TryParseHtmlString("#FF5A00", out var color) ? color : Color.gray;
        private Color HEALTH_GREEN = ColorUtility.TryParseHtmlString("#25FF00", out var color) ? color : Color.green;
        private Color HEALTH_RED = ColorUtility.TryParseHtmlString("#FF0014", out var color) ? color : Color.red;
        private Color HEALTH_YELLOW = ColorUtility.TryParseHtmlString("#FFF200", out var color) ? color : Color.yellow;
        
        [SerializeField] private RectTransform _selectedUnitsContainer;
        [SerializeField] private RectTransform _unitQueueTemplate;
        
        private EntityManager _entityManager;
        private NativeArray<Entity> _selectedUnits;
        private List<Image> _selectedUnitsImages = new List<Image>();

        public bool onHealthChanged = false;
        public bool onUnitDead = false;
        
        public static SelectionUI Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _unitQueueTemplate.gameObject.SetActive(false);
        }
        
        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            UnitSelectionManager.Instance.OnSelectionEntitiesChanged += UnitSelectionManager_OnSelectionEntitiesChanged;
            
            Hide();
        }

        private void Update()
        {    
            if (onUnitDead)
            {
                EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, Unit, UnitTypeHolder, Health>().Build(_entityManager);
            
                _selectedUnits = entityQuery.ToEntityArray(Allocator.Persistent);
                
                UpdateUnitQueueVisual();
                onHealthChanged = false;
                onUnitDead = false;
            }
            else if (onHealthChanged)
            {
                UpdateUnitsColor();
                onHealthChanged = false;
            }
        }

        private void UnitSelectionManager_OnSelectionEntitiesChanged(object sender, System.EventArgs e)
        {
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, Unit, UnitTypeHolder, Health>().Build(_entityManager);
            
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Persistent);

            if (entityArray.Length > 0)
            {
                _selectedUnits = entityArray;
                
                Show();
                UpdateUnitQueueVisual();
            }
            else
            {
                ClearEntities(_selectedUnits);
                _selectedUnitsImages.Clear();
                Hide();
            }
        }

        private void UpdateUnitQueueVisual()
        {
            _selectedUnitsImages.Clear();
            
            foreach (Transform child in _selectedUnitsContainer)
            {
                if (child == _unitQueueTemplate)
                {
                    continue;
                }
                
                Destroy(child.gameObject);
            }
            
            foreach (Entity entity in _selectedUnits)
            {
                UnitTypeHolder unitType = _entityManager.GetComponentData<UnitTypeHolder>(entity);
                RectTransform unitQueueRectTransform = Instantiate(_unitQueueTemplate, _selectedUnitsContainer);
                unitQueueRectTransform.gameObject.SetActive(true);

                UnitTypeSO unitTypeSO = GameAssets.Instance.unitTypeListSO.GetUnitTypeSO(unitType.unitType);
                Image image = unitQueueRectTransform.GetComponent<Image>();
                _selectedUnitsImages.Add(image);
                image.sprite = unitTypeSO.sprite;
                image.color = GetColor(entity);
            }
        }
        
        private static void ClearEntities(NativeArray<Entity> array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Entity.Null;
            }
        }
        
        private void Show()
        {
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        private Color GetColor(Entity entity)
        {
            Health health = _entityManager.GetComponentData<Health>(entity);
            
            if (health.healthAmount < 30f)
            {
                return HEALTH_RED;
            }
            else if (health.healthAmount < 50f)
            {
                return HEALTH_BROWN;
            }
            else if (health.healthAmount < 70f)
            {
               return HEALTH_YELLOW;
            }
            else
            {
                return HEALTH_GREEN;
            }
        }

        private void UpdateUnitsColor()
        {
            if (_selectedUnits.Length > 0)
            {
                int index = 0;
                foreach (Entity entity in _selectedUnits)
                {
                    _selectedUnitsImages[index].color = GetColor(entity);
                    index++;
                }
            }
        }
    }
}