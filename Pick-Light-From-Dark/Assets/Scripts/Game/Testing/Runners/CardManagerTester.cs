using UnityEngine;
using Game.Card;
using Game.Data;
using Game.Config;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 卡牌管理器手动测试器
    /// 测试 Initialize / AddCard / RemoveCard / DiscardOtherCards / 历史记录
    /// </summary>
    public class CardManagerTester : MonoBehaviour
    {
        [Header("测试关卡配置")]
        public LevelConfigSO testConfig;

        [Header("手动添加卡牌")]
        public int newCardId = 999;
        public string newCardName = "DummyCard";
        public CardType cardType = CardType.NonStackable;
        public int initialStack = 1;

        [Header("DiscardOtherCards 保留 cardId")]
        public int keepCardId = 1;

        [Header("UI 显示")]
        public bool showOnGUI = true;

        private CardManager cardManager;

        void Start()
        {
            cardManager = CardManager.Instance;
            Debug.Log("[CardManagerTester] 已就绪");
        }

        void OnGUI()
        {
            if (!showOnGUI) return;

            int x = 10, y = 10, w = 380, h = 24;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 360), "CardManager 测试器");
            y += 25;

            if (cardManager != null)
            {
                GUI.Label(new Rect(x, y, w, h), $"手牌总数: {cardManager.GetHandCardCount()}  可用: {cardManager.GetAvailableCardCount()}"); y += h;
                var hand = cardManager.GetHandCards();
                foreach (var c in hand)
                {
                    GUI.Label(new Rect(x, y, w, h), $"  - id={c.data.id} {c.data.cardName} 已用={c.isUsed}"); y += h;
                }
                var hist = cardManager.GetCardHistory();
                GUI.Label(new Rect(x, y, w, h), $"历史记录条数: {hist.Count}"); y += h + 4;
            }

            GUI.Label(new Rect(x, y, 100, h), "新卡 id:");
            int.TryParse(GUI.TextField(new Rect(x + 100, y, 60, h), newCardId.ToString()), out newCardId); y += h;
            GUI.Label(new Rect(x, y, 100, h), "新卡名:");
            newCardName = GUI.TextField(new Rect(x + 100, y, 200, h), newCardName); y += h;
            GUI.Label(new Rect(x, y, 100, h), "保留 id:");
            int.TryParse(GUI.TextField(new Rect(x + 100, y, 60, h), keepCardId.ToString()), out keepCardId); y += h + 4;

            if (GUI.Button(new Rect(x, y, w, h), "Initialize 关卡")) DoInitialize(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "AddCard (按上面字段)")) DoAddCard(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "DiscardOtherCards(keepCardId)")) cardManager.DiscardOtherCards(keepCardId); y += h;
        }

        [ContextMenu("Initialize")]
        public void DoInitialize()
        {
            if (testConfig == null)
            {
                Debug.LogError("[CardManagerTester] testConfig 未指定");
                return;
            }
            cardManager.Initialize(testConfig);
        }

        [ContextMenu("AddCard")]
        public void DoAddCard()
        {
            var data = new CardData
            {
                id = newCardId,
                cardName = newCardName,
                cardType = cardType,
                initialStack = initialStack
            };
            cardManager.AddCard(data);
        }
    }
}
