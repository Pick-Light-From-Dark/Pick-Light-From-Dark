using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Game.Data;

namespace Game.UI
{
    public class CardSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("UI组件")]
        [SerializeField] private Image cardImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI emoText;
        [SerializeField] private TextMeshProUGUI descText;

        [Header("拖拽")]
        [SerializeField] private float dragAlpha = 0.7f;

        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private int originalSiblingIndex;
        private bool wasDragged;
        private Vector3 beginMousePos;
        private RectTransform rectTransform;
        private Transform acceptedParent;

        /// <summary>绑定的卡牌数据</summary>
        public CardData CardData { get; private set; }

        public static event System.Action<CardSlot> OnCardClicked;
        public static CardSlot CurrentDragging { get; private set; }

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>从 ScriptableObject 设置卡牌显示</summary>
        public void SetCardData(CardData data)
        {
            CardData = data;
            if (data == null) return;

            if (nameText != null) nameText.text = data.cardName;
            if (timeText != null) timeText.text = $"{data.CalculateTotalDuration():F1}s";
            if (emoText != null)
                emoText.text = FormatEmo(data.panicDelta, data.exciteDelta);
            if (descText != null) descText.text = data.description;
        }

        /// <summary>纯前端设置（无数据绑定时用）</summary>
        public void SetDisplay(string cardName, string time, string emo)
        {
            if (nameText != null) nameText.text = cardName;
            if (timeText != null) timeText.text = time;
            if (emoText != null) emoText.text = emo;
        }

        public void AcceptDrop(Transform newParent) => acceptedParent = newParent;

        private static string FormatEmo(int panic, int excite)
        {
            string s = "";
            if (panic != 0) s += $"慌{panic:+0;-0}";
            if (excite != 0) s += $" 兴{excite:+0;-0}";
            return s.Trim();
        }

        // ==================== 点击 ====================

        public void OnPointerClick(PointerEventData eventData)
        {
            OnCardClicked?.Invoke(this);
        }

        // ==================== 拖拽 ====================

        public void OnBeginDrag(PointerEventData eventData)
        {
            wasDragged = false;
            beginMousePos = eventData.position;
            CurrentDragging = this;
            acceptedParent = null;

            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

            transform.SetParent(transform.root, true);
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Vector3.Distance(eventData.position, beginMousePos) > 5f)
                wasDragged = true;

            Vector2 localPos;
            RectTransform parentRt = transform.parent as RectTransform;
            if (parentRt != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRt, eventData.position, eventData.pressEventCamera, out localPos))
            {
                rectTransform.localPosition = localPos;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (acceptedParent != null)
            {
                transform.SetParent(acceptedParent, false);
                rectTransform.localPosition = Vector3.zero;
            }
            else
            {
                if (originalParent != null)
                {
                    transform.SetParent(originalParent, false);
                    transform.SetSiblingIndex(originalSiblingIndex);
                }
                rectTransform.localPosition = Vector3.zero;
            }

            CurrentDragging = null;
            wasDragged = false;
            acceptedParent = null;
        }
    }
}
