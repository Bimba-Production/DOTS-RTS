using _Scripts.Authoring;
using _Scripts.MonoBehaviours;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class BuildingBarracksUI : MonoBehaviour
    {
        [SerializeField] private Button _soldierButton;
        [SerializeField] private Button _scoutButton;
        [SerializeField] private Image _progressBar;
        [SerializeField] private RectTransform _unitQueueContainer;
        [SerializeField] private RectTransform _unitQueueTemplate;
        
        private Entity _entity;
        private EntityManager _entityManager;
        private NativeArray<Entity> _barracks;
        private void Awake()
        {
            _soldierButton.onClick.AddListener(BuildingBarracksUI_OnSoldierButtonClicked);
            _scoutButton.onClick.AddListener(BuildingBarracksUI_OnScoutButtonClicked);
            
            _unitQueueTemplate.gameObject.SetActive(false);
        }

        private void BuildingBarracksUI_OnSoldierButtonClicked()
        {
            _entityManager.SetComponentData(_barracks[0], new BuildingBarracksUnitEnqueue
            {
                unitType = UnitType.Soldier,
            });
            
            _entityManager.SetComponentEnabled<BuildingBarracksUnitEnqueue>(_barracks[0], true);

            RotateLeft(_barracks);
        }
        private void BuildingBarracksUI_OnScoutButtonClicked()
        {
            _entityManager.SetComponentData(_barracks[0], new BuildingBarracksUnitEnqueue
            {
                unitType = UnitType.Scout,
            });
            
            _entityManager.SetComponentEnabled<BuildingBarracksUnitEnqueue>(_barracks[0], true);
            
            RotateLeft(_barracks);
        }
        
        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            UnitSelectionManager.Instance.OnSelectionEntitiesChanged += UnitSelectionManager_OnSelectionEntitiesChanged;
            DOTSEventsManager.Instance.OnBarracksUnitQueueChanged += DOTSEventsManager_OnBarracksUnitQueueChanged;
            
            Hide();
        }

        private void DOTSEventsManager_OnBarracksUnitQueueChanged(object sender, System.EventArgs e)
        {
            Entity entity = (Entity)sender;
            if (entity == _entity)
            {
                UpdateUnitQueueVisual();
            }
        }
        
        private void Update()
        {
            UpdateProgressBarVisual();

            if (_entity != Entity.Null)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    BuildingBarracksUI_OnSoldierButtonClicked();
                }
            
                if (Input.GetKeyDown(KeyCode.S))
                {
                    BuildingBarracksUI_OnScoutButtonClicked();
                }
            }
        }
        
        private void UnitSelectionManager_OnSelectionEntitiesChanged(object sender, System.EventArgs e)
        {
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, BuildingBarracks>().Build(_entityManager);
            
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Persistent);

            if (entityArray.Length > 0)
            {
                _entity = entityArray[0];
                _barracks = entityArray;
                
                Show();
                UpdateProgressBarVisual();
                UpdateUnitQueueVisual();
            }
            else
            {
                _entity = Entity.Null;
                ClearEntities(_barracks);
                
                Hide();
            }
        }

        private void UpdateProgressBarVisual()
        {
            if (_entity == Entity.Null)
            {
                _progressBar.fillAmount = 0f;
                return;
            }
            
            BuildingBarracks buildingBarracks = _entityManager.GetComponentData<BuildingBarracks>(_entity);
            if (buildingBarracks.activeUnitType == UnitType.None)
            {
                _progressBar.fillAmount = 0f;
            }
            else
            {
                _progressBar.fillAmount = buildingBarracks.progress / buildingBarracks.progressMax;
            }
        }

        private void UpdateUnitQueueVisual()
        {
            foreach (Transform child in _unitQueueContainer)
            {
                if (child == _unitQueueTemplate)
                {
                    continue;
                }
                
                Destroy(child.gameObject);
            }
            
            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeBuffers =
                _entityManager.GetBuffer<SpawnUnitTypeBuffer>(_entity, true);

            foreach (SpawnUnitTypeBuffer spawnUnitTypeBuffer in spawnUnitTypeBuffers)
            {
                RectTransform unitQueueRectTransform = Instantiate(_unitQueueTemplate, _unitQueueContainer);
                unitQueueRectTransform.gameObject.SetActive(true);

                UnitTypeSO unitTypeSO = GameAssets.Instance.unitTypeListSO.GetUnitTypeSO(spawnUnitTypeBuffer.unitType);
                unitQueueRectTransform.GetComponent<Image>().sprite = unitTypeSO.sprite;
            }
        }
        
        private static void RotateLeft<T>(NativeArray<T> array) where T : struct
        {
            if (array.Length < 2) return;

            T first = array[0];

            for (int i = 0; i < array.Length - 1; i++)
            {
                array[i] = array[i + 1];
            }

            array[^1] = first;
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
    }
}