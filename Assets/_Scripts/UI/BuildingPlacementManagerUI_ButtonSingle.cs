using System;
using _Scripts.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class BuildingPlacementManagerUI_ButtonSingle : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _selected;
        
        private BuildingTypeSO _buildingTypeSO;
        private KeyCode _keyCode = KeyCode.None;

        private void Update()
        {
            if (_keyCode != KeyCode.None)
            {
                if (Input.GetKeyDown(_keyCode))
                {
                    BuildingPlacementManager.Instance.SetActiveBuildingTypeSO(_buildingTypeSO);
                }
            }
        }

        public void Setup(BuildingTypeSO buildingTypeSO, KeyCode keyCode)
        {
            _buildingTypeSO = buildingTypeSO;
            _keyCode = keyCode;
            
            GetComponent<Button>().onClick.AddListener(() =>
            {
                BuildingPlacementManager.Instance.SetActiveBuildingTypeSO(_buildingTypeSO);
            });
            
            _icon.sprite = _buildingTypeSO.icon;
        }

        public void ShowSelected()
        {
            _selected.enabled = true;
        }
        
        public void HideSelected()
        {
            _selected.enabled = false;   
        }
    }
}