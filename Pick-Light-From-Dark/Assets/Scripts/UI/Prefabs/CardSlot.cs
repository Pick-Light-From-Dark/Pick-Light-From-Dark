using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Game.Data;

namespace Game.UI
{
    public class CardSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI组件")]
        [SerializeField] private Image cardImage;
        [SerializeField] private Image cardBackgroundImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI panicText;
        [SerializeField] private TextMeshProUGUI exciteText;
        [SerializeField] private TextMeshProUGUI stackText;

        [Header("拖拽")]
        [SerializeField] private float dragAlpha = 0.7f;
        [SerializeField] private float dragSmoothSpeed = 25f;

        [Header("悬停")]
        [SerializeField] private float hoverScale = 1.12f;
        [SerializeField] private float hoverSpeed = 8f;

        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private int originalSiblingIndex;
        private RectTransform rectTransform;
        private Transform acceptedParent;

        // 拖拽平滑
        private Vector2 dragTargetPos;
        private bool isDragging;
        private bool dragTargetSet;
        private bool dragFirstFrameDone;

        // 悬停缩放
        private bool isHovered;
        private float targetScale = 1f;

        /// <summary>绑定的卡牌数据</summary>
        public CardData CardData { get; private set; }

        /// <summary>当前剩余堆叠层数（可堆叠类卡牌用）</summary>
        public int StackCount { get; private set; }

        public static event System.Action<CardSlot> OnCardClicked;
        public static event System.Action<CardSlot> OnCardDragStarted;
        public static event System.Action<CardSlot> OnCardDragEnded;
        public static CardSlot CurrentDragging { get; private set; }
        public static bool IsPopupActive { get; set; }

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        void Update()
        {
            // 悬停缩放平滑过渡
            float currentScale = rectTransform.localScale.x;
            float desired = isDragging ? 1f : targetScale;
            float newScale = Mathf.Lerp(currentScale, desired, Time.unscaledDeltaTime * hoverSpeed);
            if (Mathf.Abs(newScale - desired) < 0.001f) newScale = desired;
            rectTransform.localScale = Vector3.one * newScale;

            // 拖拽位置：首次帧直接定位，后续平滑跟随
            if (isDragging && dragTargetSet)
            {
                if (!dragFirstFrameDone)
                {
                    rectTransform.localPosition = dragTargetPos;
                    dragFirstFrameDone = true;
                }
                else
                {
                    Vector2 current = rectTransform.localPosition;
                    float t = Time.unscaledDeltaTime * dragSmoothSpeed;
                    rectTransform.localPosition = t < 1f
                        ? Vector2.Lerp(current, dragTargetPos, t)
                        : dragTargetPos;
                }
            }
        }

        /// <summary>从 ScriptableObject 设置卡牌显示</summary>
        public void SetCardData(CardData data, int stackCount = -1)
        {
            CardData = data;
            if (data == null) return;

            // 堆叠层数：-1 表示使用数据中的初始层数
            StackCount = stackCount >= 0 ? stackCount : data.initialStack;

            if (nameText != null) nameText.text = data.cardName;
            if (timeText != null) timeText.text = $"{data.CalculateTotalDuration():F0}";
            if (panicText != null) panicText.text = $"{data.panicDelta:+0;-0}";
            if (exciteText != null) exciteText.text = $"{data.exciteDelta:+0;-0}";

            // 卡面图：优先用 cardSprite，其次用 iconPath 从 Resources 加载
            if (cardImage != null)
            {
                if (data.cardSprite != null)
                    cardImage.sprite = data.cardSprite;
                else if (!string.IsNullOrEmpty(data.iconPath))
                    cardImage.sprite = Resources.Load<Sprite>(data.iconPath);
            }

            // 卡牌背景图：根据 cardBackgroundType 从 Resources 加载
            if (cardBackgroundImage != null)
            {
                string bgPath = $"Card/CardImgBk/CardImgBk_{data.cardBackgroundType}";
                var bgSprite = Resources.Load<Sprite>(bgPath);
                if (bgSprite != null)
                    cardBackgroundImage.sprite = bgSprite;
            }

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

        private Image stackBgImage;

        private void RefreshStackDisplay()
        {
            if (stackText == null) return;
            bool show = CardData != null && CardData.cardType == CardType.Stackable;

            // 找到数量背景图（和 stackText 同节点或父节点上）
            if (stackBgImage == null)
                stackBgImage = stackText.GetComponent<Image>() ?? stackText.GetComponentInParent<Image>();

            stackText.gameObject.SetActive(show);
            if (stackBgImage != null) stackBgImage.enabled = show;
            if (show) stackText.text = $"{StackCount}";
        }

        public void AcceptDrop(Transform newParent) => acceptedParent = newParent;

        // ==================== 悬停 ====================

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (GamePanel.IsInteractionLocked) return;
            isHovered = true;
            targetScale = hoverScale;
            MusicMgr.Instance.PlaySound("DXH_SOUND/11.悬停卡片UI");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            targetScale = 1f;
        }

        // ==================== 点击 ====================

        public void OnPointerClick(PointerEventData eventData)
        {
            if (GamePanel.IsInteractionLocked) return;
            MusicMgr.Instance.PlaySound("DXH_SOUND/12.点击卡片UI");
            OnCardClicked?.Invoke(this);
        }

        // ==================== 拖拽 ====================

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsPopupActive) return;
            if (GamePanel.IsInteractionLocked) return;
            CurrentDragging = this;
            acceptedParent = null;
            isDragging = true;

            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

            transform.SetParent(transform.root, true);
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;

            // 拖拽时恢复原大小
            targetScale = 1f;
            dragTargetPos = rectTransform.localPosition;
            dragTargetSet = false;
            dragFirstFrameDone = false;

            OnCardDragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform parentRt = transform.parent as RectTransform;
            if (parentRt != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRt, eventData.position, eventData.pressEventCamera, out Vector2 localPos))
            {
                dragTargetPos = localPos;
                dragTargetSet = true;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (acceptedParent != null)
            {
                transform.SetParent(acceptedParent, false);
                rectTransform.localPosition = Vector3.zero;
                dragTargetPos = Vector2.zero;
            }
            else
            {
                if (originalParent != null)
                {
                    transform.SetParent(originalParent, false);
                    transform.SetSiblingIndex(originalSiblingIndex);
                }
                rectTransform.localPosition = Vector3.zero;
                dragTargetPos = Vector2.zero;
            }

            OnCardDragEnded?.Invoke(this);
            CurrentDragging = null;
            acceptedParent = null;
        }
    }
}
