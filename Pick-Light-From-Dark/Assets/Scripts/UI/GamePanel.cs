using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Game.Data;
using Game.Config;
using Game.Card;
using Game.UI;

public class GamePanel : BasePanel
{
    [Header("备选区容器")]
    [SerializeField] private Transform cardSpawnPos1;
    [SerializeField] private Transform cardSpawnPos2;
    [SerializeField] private Transform cardSpawnPos3;

    [Header("思考框（卡牌拖放目标）")]
    [SerializeField] private GameObject cardDropZoneObj;

    [Header("读条UI — 红色底条 + 绿色填充 + 时间文本")]
    [SerializeField] private Image cardLoadingBarBg;
    [SerializeField] private Image cardLoadingBarFill;
    [SerializeField] private TextMeshProUGUI LoadingCount;

    [Header("卡牌信息弹窗")]
    [SerializeField] private GameObject cardInfoPopup;
    [SerializeField] private GameObject cardSelectionArea;

    private CardGrid cardGrid;
    private CardDropZone cardDropZone;
    private CardManager cardManager;

    /// <summary>读条前的备选区卡牌快照（打断时恢复用）</summary>
    private List<CardData> preReadSnapshot;

    /// <summary>当前正在读条的卡牌（放在思考框中显示）</summary>
    private CardSlot readingCardSlot;

    /// <summary>弹窗打开前的 timescale 缓存</summary>
    private float prePopupTimeScale = 1f;

    // ==================== 自管读条状态 ====================

    private bool isReading;
    private float readTime;
    private float totalReadTime;
    private CardData readingCardData;

    protected override void Awake()
    {
        base.Awake();

        EnsureCanvasRaycaster();
        EnsureEventSystem();
        EnsureCardGrid();
        EnsureDropZone();

        cardManager = CardManager.Instance;

        CardDropZone.OnCardDropped += OnCardDropped;
        CardSlot.OnCardClicked += OnCardClicked;
        CardManager.OnSelectionChanged += RefreshSelectionArea;

        if (cardLoadingBarBg != null) cardLoadingBarBg.gameObject.SetActive(false);
        if (LoadingCount != null) LoadingCount.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isReading)
        {
            readTime += Time.unscaledDeltaTime;
            float progress = totalReadTime > 0f ? Mathf.Clamp01(readTime / totalReadTime) : 0f;

            if (cardLoadingBarFill != null)
                cardLoadingBarFill.fillAmount = progress;

            if (LoadingCount != null)
                LoadingCount.text = $"{readTime:F1}s / {totalReadTime:F1}s";

            if (readTime >= totalReadTime)
                OnReadingComplete();
        }
    }

    void OnDestroy()
    {
        CardDropZone.OnCardDropped -= OnCardDropped;
        CardSlot.OnCardClicked -= OnCardClicked;
        CardManager.OnSelectionChanged -= RefreshSelectionArea;
    }

    public override void ShowMe()
    {
        RefreshSelectionArea();
    }

    public override void HideMe() { }

    // ==================== 读条生命周期 ====================

    /// <summary>拖入卡牌 → 开始读条</summary>
    private void StartReading(CardSlot card)
    {
        if (card.CardData == null) return;
        if (isReading) return;

        readingCardSlot = card;
        readingCardData = card.CardData;
        totalReadTime = card.CardData.CalculateTotalDuration();
        readTime = 0f;
        isReading = true;

        // 将卡牌移入思考框
        var dropRt = cardDropZoneObj != null ? cardDropZoneObj.GetComponent<RectTransform>() : null;
        if (dropRt != null)
        {
            card.transform.SetParent(dropRt, false);
            card.transform.localPosition = Vector3.zero;
        }

        // 快照备选区 → 隐藏
        preReadSnapshot = new List<CardData>();
        foreach (var data in cardManager.GetAvailableCards())
            preReadSnapshot.Add(data);

        cardGrid.Clear();
        if (cardSelectionArea != null) cardSelectionArea.SetActive(false);

        // 显示读条UI
        if (cardLoadingBarBg != null) cardLoadingBarBg.gameObject.SetActive(true);
        if (cardLoadingBarFill != null) cardLoadingBarFill.fillAmount = 0f;
        if (LoadingCount != null)
        {
            LoadingCount.gameObject.SetActive(true);
            LoadingCount.text = $"0.0s / {totalReadTime:F1}s";
        }

        Debug.Log($"[GamePanel] 开始读条: {card.CardData.cardName} 时长={totalReadTime:F1}s");
    }

    /// <summary>读条完成</summary>
    private void OnReadingComplete()
    {
        isReading = false;

        // 进度条顶满
        if (cardLoadingBarFill != null) cardLoadingBarFill.fillAmount = 1f;

        // 销毁思考框中的卡牌
        DestroyReadingCard();

        // 隐藏读条UI
        if (cardLoadingBarBg != null) cardLoadingBarBg.gameObject.SetActive(false);
        if (LoadingCount != null) LoadingCount.gameObject.SetActive(false);

        // 通知 CardManager
        cardManager.OnCardUsed(readingCardData);

        preReadSnapshot = null;
        RefreshSelectionArea();
        Debug.Log($"[GamePanel] 读条完成: {readingCardData.cardName}");
    }

    /// <summary>打断读条（空格键）</summary>
    public void InterruptReading()
    {
        if (!isReading) return;

        isReading = false;
        readTime = 0f;

        DestroyReadingCard();
        if (cardLoadingBarBg != null) cardLoadingBarBg.gameObject.SetActive(false);
        if (LoadingCount != null) LoadingCount.gameObject.SetActive(false);

        cardManager.OnCardInterrupted(readingCardData);

        // 恢复备选区快照
        cardGrid.Clear();
        if (preReadSnapshot != null)
        {
            var spawns = new[] { cardSpawnPos1, cardSpawnPos2, cardSpawnPos3 };
            for (int i = 0; i < preReadSnapshot.Count && i < spawns.Length; i++)
            {
                int stacks = cardManager.GetRemainingStacks(preReadSnapshot[i].id);
                cardGrid.AddCard(preReadSnapshot[i], spawns[i], stacks);
            }
            preReadSnapshot = null;
        }

        if (cardSelectionArea != null) cardSelectionArea.SetActive(true);
        Debug.Log("[GamePanel] 读条被打断");
    }

    private void DestroyReadingCard()
    {
        if (readingCardSlot != null)
        {
            Destroy(readingCardSlot.gameObject);
            readingCardSlot = null;
        }
    }

    // ==================== 备选区刷新 ====================

    private void RefreshSelectionArea()
    {
        cardGrid.Clear();

        var spawns = new[] { cardSpawnPos1, cardSpawnPos2, cardSpawnPos3 };
        List<CardData> available;

        if (cardManager != null)
            available = cardManager.GetAvailableCards();
        else
            available = new List<CardData>();

        if (available.Count == 0)
        {
            var containers = Resources.LoadAll<CardDataContainer>("Card");
            if (containers != null)
            {
                var sorted = new List<CardDataContainer>(containers);
                sorted.Sort((a, b) =>
                {
                    int idA = a.cardData != null ? a.cardData.id : 0;
                    int idB = b.cardData != null ? b.cardData.id : 0;
                    return idA.CompareTo(idB);
                });
                foreach (var c in sorted)
                {
                    if (c.cardData != null) available.Add(c.cardData);
                }
            }
        }

        for (int i = 0; i < available.Count && i < spawns.Length; i++)
        {
            int stacks = cardManager != null ? cardManager.GetRemainingStacks(available[i].id) : 1;
            cardGrid.AddCard(available[i], spawns[i], stacks);
        }

        if (cardSelectionArea != null)
            cardSelectionArea.SetActive(available.Count > 0);
    }

    // ==================== 卡牌交互 ====================

    private void OnCardClicked(CardSlot card)
    {
        if (cardInfoPopup == null || card.CardData == null) return;

        bool isActive = !cardInfoPopup.activeSelf;
        cardInfoPopup.SetActive(isActive);

        if (isActive)
        {
            prePopupTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            FillPopupInfo(card.CardData);
        }
        else
        {
            Time.timeScale = prePopupTimeScale;
        }
    }

    private void FillPopupInfo(CardData data)
    {
        FindAndSetText(cardInfoPopup, "CardNameText", data.cardName);
        FindAndSetText(cardInfoPopup, "DescText", data.description);
        FindAndSetText(cardInfoPopup, "PanicText", $"慌乱变化: {data.panicDelta:+0;-0}");
        FindAndSetText(cardInfoPopup, "ExciteText", $"兴奋变化: {data.exciteDelta:+0;-0}");
        FindAndSetText(cardInfoPopup, "InterruptText", $"打断慌乱: +{data.interruptPanicAdd}");
        FindAndSetText(cardInfoPopup, "TimeText", $"读条时长: {data.CalculateTotalDuration():F1}s");
        FindAndSetText(cardInfoPopup, "TypeText", data.cardType == CardType.Stackable ? "可堆叠" : "不可堆叠");
    }

    private void FindAndSetText(GameObject root, string childName, string value)
    {
        var child = root.transform.Find(childName);
        if (child == null) return;
        var tmp = child.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = value;
    }

    // ==================== 卡牌拖放 ====================

    private void OnCardDropped(CardSlot card)
    {
        if (card.CardData == null) return;
        if (isReading) return;

        Debug.Log($"[GamePanel] 卡牌拖入思考框: {card.CardData.cardName}");
        StartReading(card);
    }

    // ==================== 初始化 ====================

    private void EnsureCardGrid()
    {
        cardGrid = GetComponentInChildren<CardGrid>();
        if (cardGrid == null)
        {
            var go = new GameObject("CardGrid", typeof(RectTransform), typeof(CardGrid));
            go.transform.SetParent(transform, false);
            cardGrid = go.GetComponent<CardGrid>();
        }

        var baseCard = Resources.Load<GameObject>("Card/BaseCard");
        if (baseCard != null)
        {
            var field = typeof(CardGrid).GetField("cardSlotPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(cardGrid, baseCard);
        }
    }

    private void EnsureDropZone()
    {
        if (cardDropZoneObj != null)
        {
            cardDropZone = cardDropZoneObj.GetComponent<CardDropZone>();
            if (cardDropZone == null)
                cardDropZone = cardDropZoneObj.AddComponent<CardDropZone>();

            var img = cardDropZoneObj.GetComponent<Image>();
            if (img == null)
                img = cardDropZoneObj.AddComponent<Image>();
            img.raycastTarget = true;

            var rt = cardDropZoneObj.GetComponent<RectTransform>();
            if (rt.sizeDelta.x < 10 || rt.sizeDelta.y < 10)
                rt.sizeDelta = new Vector2(200, 200);
        }

        if (cardDropZone == null)
        {
            cardDropZone = GetComponentInChildren<CardDropZone>();
            if (cardDropZone == null)
            {
                var go = new GameObject("CardDropZone", typeof(RectTransform), typeof(Image), typeof(CardDropZone));
                go.transform.SetParent(transform, false);
                go.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(200, 200);
                cardDropZone = go.GetComponent<CardDropZone>();
            }
        }
    }

    private void EnsureCanvasRaycaster()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();
    }

    private void EnsureEventSystem()
    {
        var es = EventSystem.current;
        if (es != null && es.GetComponent<StandaloneInputModule>() == null)
            es.gameObject.AddComponent<StandaloneInputModule>();
        else if (es == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
