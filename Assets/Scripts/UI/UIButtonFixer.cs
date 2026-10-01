using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivalShooter.UI
{
    /// <summary>
    /// Attaches to UI buttons to guarantee responsive mouse and touch clicks.
    /// Ensures raycast targets are properly configured on parent images and disabled on child labels.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonFixer : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Button button;
        private Image image;
        private Vector3 originalScale;

        private void Awake()
        {
            button = GetComponent<Button>();
            image = GetComponent<Image>();
            originalScale = transform.localScale;
            EnforceRaycastTargets();
        }

        private void OnEnable()
        {
            EnforceRaycastTargets();
            transform.localScale = originalScale;
        }

        private void OnDisable()
        {
            transform.localScale = originalScale;
        }

        private void EnforceRaycastTargets()
        {
            if (image != null)
            {
                image.raycastTarget = true;
                if (button != null && button.targetGraphic == null)
                {
                    button.targetGraphic = image;
                }
            }

            // Ensure child labels never block pointer events for the button
            foreach (var txt in GetComponentsInChildren<Text>(true))
            {
                txt.raycastTarget = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            transform.localScale = originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.localScale = originalScale * 0.95f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = originalScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.localScale = originalScale * 1.05f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = originalScale;
        }
    }
}
