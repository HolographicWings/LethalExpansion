using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LethalExpansion.Utils.HUD
{
    internal class SettingMenu_DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public UnityEvent onBeginDragEvent;
        public UnityEvent onDragEvent;
        public UnityEvent onEndDragEvent;

        public RectTransform rectTransform;
        private Canvas canvas;

        private void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            canvas = FindFirstObjectByType<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDragEvent != null)
            {
                onBeginDragEvent.Invoke();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(eventData != null && rectTransform != null)
            {
                rectTransform.anchoredPosition = ClampToWindow(rectTransform.anchoredPosition + eventData.delta / canvas.scaleFactor);
                if(onDragEvent != null)
                {
                    onDragEvent.Invoke();
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDragEvent != null)
            {
                onEndDragEvent.Invoke();
            }
        }
        private Vector2 ClampToWindow(Vector2 position)
        {
            Vector3[] corners = new Vector3[4];

            float minX = -260;
            float maxX = 260;
            float minY = -60f;
            float maxY = 50f;

            float clampedX = Mathf.Clamp(position.x, minX, maxX);
            float clampedY = Mathf.Clamp(position.y, minY, maxY);

            return new Vector2(clampedX, clampedY);
        }
    }
}
