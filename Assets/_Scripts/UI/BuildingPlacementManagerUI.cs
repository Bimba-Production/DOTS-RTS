using System.Collections.Generic;
using _Scripts.MonoBehaviours;
using UnityEngine;

namespace _Scripts.UI
{
    public class BuildingPlacementManagerUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _buildingContainer;
        [SerializeField] private RectTransform _buildingTemplate;
        [SerializeField] private BuildingTypeListSO _buildingTypeListSO;

        private Dictionary<BuildingTypeSO, BuildingPlacementManagerUI_ButtonSingle> _buildingButtonDictionary = 
                                            new Dictionary<BuildingTypeSO, BuildingPlacementManagerUI_ButtonSingle>();
        
        public static BuildingPlacementManagerUI Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
            _buildingTemplate.gameObject.SetActive(false);

            SetupBuildingPlacementUI();
            HideALLBuildingsPlacementUI();
        }
        
        private void Start()
        {
            BuildingPlacementManager.Instance.OnActiveBuildingTypeSOChanged += BuildingPlacementManager_OnActiveBuildingTypeSOChanged;
            UpdateSelectedVisual();
            
            UnitSelectionManager.Instance.OnWorkerSelected += UnitSelectionManager_OnWorkerSelected;
            UnitSelectionManager.Instance.OnWorkerDeselected += UnitSelectionManager_OnWorkerDeselected;
        }

        private void UnitSelectionManager_OnWorkerSelected(object sender, System.EventArgs e)
        {
            SetupBuildingPlacementUI();
        }
        
        private void UnitSelectionManager_OnWorkerDeselected(object sender, System.EventArgs e)
        {
            HideALLBuildingsPlacementUI();
        }

        private void SetupBuildingPlacementUI()
        {
            _buildingContainer.gameObject.SetActive(true);
            
            foreach (BuildingTypeSO buildingTypeSO in _buildingTypeListSO.buildingTypes)
            {
                if (!buildingTypeSO.showInBuildingPlacementManagerUI) continue;

                if (_buildingButtonDictionary.ContainsKey(buildingTypeSO))
                {
                    _buildingButtonDictionary[buildingTypeSO].gameObject.SetActive(true);
                }
                else
                {
                    RectTransform buildingRectTransform = Instantiate(_buildingTemplate, _buildingContainer);
                    buildingRectTransform.gameObject.SetActive(true);

                    BuildingPlacementManagerUI_ButtonSingle buttonSingle =
                        buildingRectTransform.GetComponent<BuildingPlacementManagerUI_ButtonSingle>();
                
                    _buildingButtonDictionary[buildingTypeSO] = buttonSingle;
                    buttonSingle.Setup(buildingTypeSO, buildingTypeSO.keyCode);
                }
            }
        }

        private void HideALLBuildingsPlacementUI()
        {
            foreach (BuildingTypeSO buildingTypeSO in _buildingButtonDictionary.Keys)
            {
                _buildingButtonDictionary[buildingTypeSO].gameObject.SetActive(false);
            }
            
            _buildingContainer.gameObject.SetActive(false);
        }

        private void BuildingPlacementManager_OnActiveBuildingTypeSOChanged(object sender, System.EventArgs e)
        {
            UpdateSelectedVisual();
        }
        
        private void UpdateSelectedVisual()
        {
            foreach (BuildingTypeSO buildingTypeSO in _buildingButtonDictionary.Keys)
            {
                _buildingButtonDictionary[buildingTypeSO].HideSelected();
            }
            _buildingButtonDictionary[BuildingPlacementManager.Instance.GetActiveBuildingTypeSO()].ShowSelected();
        }
    }
}