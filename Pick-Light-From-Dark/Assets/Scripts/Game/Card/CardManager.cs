using UnityEngine;
using System.Collections.Generic;
using Game.Data;
using Game.Config;

namespace Game.Card
{
    /// <summary>
    /// 卡牌管理器
    /// 管理手牌、发牌、弃牌、关联卡牌
    /// </summary>
    public class CardManager : SingletonAutoMono<CardManager>
    {
        [Header("手牌")]
        [SerializeField] private List<CardInstance> handCards;

        [Header("历史记录")]
        [SerializeField] private List<CardUseRecord> cardHistory;

        private LevelConfigSO levelConfig;
        private int nextInstanceId = 1;

        public class CardUseRecord
        {
            public int cardId;
            public string cardName;
            public bool success;
            public float useTime;

            public CardUseRecord(int cardId, string cardName, bool success)
            {
                this.cardId = cardId;
                this.cardName = cardName;
                this.success = success;
                this.useTime = Time.time;
            }
        }

        /// <summary>
        /// 初始化卡牌管理器
        /// </summary>
        public void Initialize(LevelConfigSO config)
        {
            levelConfig = config;
            handCards = new List<CardInstance>();
            cardHistory = new List<CardUseRecord>();
            nextInstanceId = 1;

            // 发放初始卡牌
            DealInitialCards();

            Debug.Log($"[CardManager] 初始化完成，发放了 {handCards.Count} 张初始卡牌");
        }

        /// <summary>
        /// 发放初始卡牌
        /// </summary>
        void DealInitialCards()
        {
            if (levelConfig == null || levelConfig.initialCards == null)
                return;

            foreach (int cardId in levelConfig.initialCards)
            {
                CardData cardData = GetCardDataById(cardId);
                if (cardData != null)
                {
                    CardInstance instance = new CardInstance(cardData, nextInstanceId++);
                    handCards.Add(instance);
                    Debug.Log($"[CardManager] 发放卡牌: {cardData.cardName}");
                }
            }
        }

        /// <summary>
        /// 添加卡牌到手牌
        /// </summary>
        public void AddCard(CardData cardData)
        {
            if (cardData == null) return;

            CardInstance instance = new CardInstance(cardData, nextInstanceId++);
            handCards.Add(instance);

            Debug.Log($"[CardManager] 添加卡牌: {cardData.cardName}");
        }

        /// <summary>
        /// 移除卡牌
        /// </summary>
        public void RemoveCard(CardInstance instance)
        {
            if (handCards.Remove(instance))
            {
                Debug.Log($"[CardManager] 移除卡牌: {instance.data.cardName}");
            }
        }

        /// <summary>
        /// 弃牌逻辑：使用指定卡牌后，隐藏其他卡牌
        /// </summary>
        public void DiscardOtherCards(int keepCardId)
        {
            List<CardInstance> toRemove = new List<CardInstance>();

            foreach (var card in handCards)
            {
                if (card.data.id != keepCardId && !card.isUsed)
                {
                    toRemove.Add(card);
                }
            }

            foreach (var card in toRemove)
            {
                handCards.Remove(card);
            }

            if (toRemove.Count > 0)
            {
                Debug.Log($"[CardManager] 弃牌: 移除了 {toRemove.Count} 张卡牌，保留卡牌ID {keepCardId}");
            }
        }

        /// <summary>
        /// 触发关联卡牌
        /// </summary>
        public void TriggerLinkedCards(CardData usedCard)
        {
            // 这里需要根据卡牌配置触发关联卡牌
            // 暂时先空实现，等待卡牌配置完善
            Debug.Log($"[CardManager] 检查关联卡牌: {usedCard.cardName}");
        }

        /// <summary>
        /// 记录卡牌使用历史
        /// </summary>
        public void RecordCardUse(CardInstance card, bool success)
        {
            CardUseRecord record = new CardUseRecord(card.data.id, card.data.cardName, success);
            cardHistory.Add(record);

            Debug.Log($"[CardManager] 记录卡牌使用: {card.data.cardName} - {(success ? "成功" : "失败")}");
        }

        /// <summary>
        /// 获取所有手牌
        /// </summary>
        public List<CardInstance> GetHandCards()
        {
            return new List<CardInstance>(handCards);
        }

        /// <summary>
        /// 获取未使用的手牌
        /// </summary>
        public List<CardInstance> GetAvailableCards()
        {
            List<CardInstance> available = new List<CardInstance>();
            foreach (var card in handCards)
            {
                if (!card.isUsed)
                {
                    available.Add(card);
                }
            }
            return available;
        }

        /// <summary>
        /// 获取手牌数量
        /// </summary>
        public int GetHandCardCount()
        {
            return handCards.Count;
        }

        /// <summary>
        /// 获取可用手牌数量
        /// </summary>
        public int GetAvailableCardCount()
        {
            int count = 0;
            foreach (var card in handCards)
            {
                if (!card.isUsed)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 根据实例ID获取卡牌
        /// </summary>
        public CardInstance GetCardByInstanceId(int instanceId)
        {
            foreach (var card in handCards)
            {
                if (card.instanceId == instanceId)
                {
                    return card;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取卡牌数据（从配置中）
        /// </summary>
        CardData GetCardDataById(int cardId)
        {
            // 从Resources加载所有卡牌数据容器
            CardDataContainer[] containers = Resources.LoadAll<CardDataContainer>("TestData");

            foreach (var container in containers)
            {
                if (container.cardData != null && container.cardData.id == cardId)
                {
                    return container.cardData;
                }
            }

            Debug.LogWarning($"[CardManager] 未找到ID为 {cardId} 的卡牌数据");
            return null;
        }

        /// <summary>
        /// 获取卡牌使用历史
        /// </summary>
        public List<CardUseRecord> GetCardHistory()
        {
            return new List<CardUseRecord>(cardHistory);
        }
    }
}
