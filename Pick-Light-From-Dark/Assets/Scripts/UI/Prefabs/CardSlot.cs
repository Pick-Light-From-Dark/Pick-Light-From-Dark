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
        [SerializeField] private TextMeshProUGUI panicText;
        [SerializeField] private TextMeshProUGUI exciteText;
        [SerializeField] private TextMeshProUGUI stackText;

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

        /// <summary>当前剩余堆叠层数（可堆叠类卡牌用）</summary>
        public int StackCount { get; private set; }

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
        public void SetCardData(CardData data, int stackCount = -1)
        {
            CardData = data;
            if (data == null) return;

            // 堆叠层数：-1 表示使用数据中的初始层数
            StackCount = stackCount >= 0 ? stackCount : data.initialStack;

            if (nameText != null) nameText.text = data.cardName;
            if (timeText != null) timeText.text = $"{data.CalculateTotalDuration():F1}s";
            if (panicText != null) panicText.text = $"{data.panicDelta:+0;-0}";
            if (exciteText != null) exciteText.text = $"{data.exciteDelta:+0;-0}";
            RefreshStackDisplay();
        }

        /// <summary>纯前端设置（无数据绑定时用）</summary>
        public void SetDisplay(string cardName, string time, string panic, string excite)
        {
            if (nameText != null) nameText.text = cardName;
            if (timeText != null) timeText.text = time;
            if (panicText != null) panicText.text = panic;
            if (exciteText != null) exciteText.text = excite;
            StackCount = 1;
            RefreshStackDisplay();
        }

        /// <summary>消耗一层堆叠（读条成功时调用），返回是否耗尽</summary>
        public bool ConsumeStack()
        {
            if (CardData == null || CardData.cardType != CardType.Stackable) return true;
            StackCount--;
            RefreshStackDisplay();
            return StackCount <= 0;
        }

        /// <summary>恢复一层堆叠（被打断时调用）</summary>
        public void RestoreStack()
        {
            if (CardData == null || CardData.cardType != CardType.Stackable) return;
            StackCount++;
            RefreshStackDisplay();
        }

        private void RefreshStackDisplay()
        {
            if (stackText == null) return;
            bool show = CardData != null
                     && CardData.cardType == CardType.Stackable
                     && StackCount > 1;
            stackText.gameObject.SetActive(show);
            if (show) stackText.text = $"x{StackCount}";
        }

        public void AcceptDrop(Transform newParent) => acceptedParent = newParent;

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
