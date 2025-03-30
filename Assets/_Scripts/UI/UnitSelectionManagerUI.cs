using _Scripts.MonoBehaviours;
using UnityEngine;

namespace _Scripts.UI
{
    public class UnitSelectionManagerUI : MonoBehaviour 
    {
        [SerializeField] private RectTransform _selectionAreaRectTransform;
        [SerializeField] private Canvas _canvas;
        
        private void Start() {
            UnitSelectionManager.Instance.OnSelectionAreaStart += UnitSelectionManager_OnSelectionAreaStart;
            UnitSelectionManager.Instance.OnSelectionAreaEnd += UnitSelectionManager_OnSelectionAreaEnd;

            _selectionAreaRectTransform.gameObject.SetActive(false);
        }

        private void Update() {
            if (_selectionAreaRectTransform.gameObject.activeSelf) {
                UpdateVisual();
            }
        }

        private void UnitSelectionManager_OnSelectionAreaStart(object sender, System.EventArgs e) {
            _selectionAreaRectTransform.gameObject.SetActive(true);

            UpdateVisual();
        }

        private void UnitSelectionManager_OnSelectionAreaEnd(object sender, System.EventArgs e) {
            _selectionAreaRectTransform.gameObject.SetActive(false);
        }

        private void UpdateVisual() {
            Rect selectionAreaRect = UnitSelectionManager.Instance.GetSelectionAreaRect();

            float canvasScale = _canvas.transform.localScale.x;
            _selectionAreaRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x, selectionAreaRect.y) / canvasScale;
            _selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRect.width, selectionAreaRect.height) / canvasScale;
        }
    }
}
