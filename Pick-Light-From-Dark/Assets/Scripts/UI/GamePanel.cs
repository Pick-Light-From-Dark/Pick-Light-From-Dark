using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Data;
using Game.UI;

public class GamePanel : BasePanel
{
    [Header("位置（拖入你创建的 Transform）")]
    [SerializeField] private Transform cardSpawnPos1;
    [SerializeField] private Transform cardSpawnPos2;
    [SerializeField] private Transform cardSpawnPos3;
    [SerializeField] private GameObject cardDropZoneObj;

    [Header("卡牌信息弹窗")]
    [SerializeField] private GameObject cardInfoPopup;

    private CardGrid cardGrid;
    private CardDropZone cardDropZone;

    protected override void Awake()
    {
        base.Awake();

        EnsureCanvasRaycaster();
        EnsureEventSystem();
        EnsureCardGrid();
        EnsureDropZone();

        CardDropZone.OnCardDropped += OnCardDropped;
        CardSlot.OnCardClicked += OnCardClicked;
    }

    void OnDestroy()
    {
        CardDropZone.OnCardDropped -= OnCardDropped;
        CardSlot.OnCardClicked -= OnCardClicked;
    }

    public override void ShowMe()
    {
        GenerateCards();
    }

    public override void HideMe() { }

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

    private void EnsureCardGrid()
    {
        cardGrid = GetComponentInChildren<CardGrid>();
        if (cardGrid == null)
        {
            var go = new GameObject("CardGrid", typeof(RectTransform), typeof(CardGrid));
            go.transform.SetParent(transform, false);
            cardGrid = go.GetComponent<CardGrid>();
        }

        // 自动加载 BaseCard 预制体
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

            // 确保有 Image 且可被射线检测到
            var img = cardDropZoneObj.GetComponent<Image>();
            if (img == null)
                img = cardDropZoneObj.AddComponent<Image>();
            img.raycastTarget = true;

            // 确保有尺寸（0x0 无法接收拖放）
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

    private void GenerateCards()
    {
        cardGrid.Clear();

        // 从 Resources/Card/ 加载 ScriptableObject 卡牌数据
        var containers = Resources.LoadAll<CardDataContainer>("Card");
        var spawns = new[] { cardSpawnPos1, cardSpawnPos2, cardSpawnPos3 };

        if (containers != null && containers.Length > 0)
        {
            for (int i = 0; i < containers.Length && i < spawns.Length; i++)
            {
                if (containers[i].cardData != null)
                    cardGrid.AddCard(containers[i].cardData, spawns[i]);
            }
            Debug.Log($"[GamePanel] 从 Resources 加载了 {Mathf.Min(containers.Length, 3)} 张卡牌");
        }
        else
        {
            // 回退：没有 ScriptableObject 时用测试数据
            cardGrid.AddCard("看书",   "5s", "慌-5 兴+3", cardSpawnPos1);
            cardGrid.AddCard("吃薯片", "4s", "慌-8 兴+5", cardSpawnPos2);
            cardGrid.AddCard("玩手机", "5s", "慌+3 兴+8", cardSpawnPos3);
            Debug.Log("[GamePanel] 未找到 CardDataContainer，使用测试卡牌");
        }
    }

    private void OnCardClicked(CardSlot card)
    {
        if (cardInfoPopup == null) return;
        cardInfoPopup.SetActive(!cardInfoPopup.activeSelf);
    }

    private void OnCardDropped(CardSlot card)
    {
        Debug.Log($"[GamePanel] 卡牌生效: {card.name}");
    }
}
