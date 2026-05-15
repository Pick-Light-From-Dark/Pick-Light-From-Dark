using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class ChoiceButtonEffect : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float hoverDarken = 0.65f;
        [SerializeField] private Vector2 hoverOffset = new Vector2(3f, -3f);

        private Image image;
        private RectTransform rectTransform;
        private Color defaultColor;
        private Vector2 defaultPosition;
        private bool isHovered;
        private bool isPressed;

        void Awake()
        {
            image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            defaultColor = image.color;
            defaultPosition = rectTransform.anchoredPosition;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            if (!isPressed) ApplyHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            if (!isPressed) ApplyDefault();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            ApplyPressed();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            if (isHovered) ApplyHover();
            else ApplyDefault();
        }

        void ApplyHover()
        {
            image.color = defaultColor * hoverDarken;
            rectTransform.anchoredPosition = defaultPosition + hoverOffset;
        }

        void ApplyPressed()
        {
            image.color = defaultColor * hoverDarken;
            rectTransform.anchoredPosition = defaultPosition;
        }

        void ApplyDefault()
        {
            image.color = defaultColor;
            rectTransform.anchoredPosition = defaultPosition;
        }
    }
}
