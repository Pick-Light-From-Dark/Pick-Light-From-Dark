using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Config;

namespace Game.Card
{
    /// <summary>
    /// 全局卡牌池中的卡牌状态
    /// </summary>
    public class PooledCard
    {
        public CardData data;
        public int remainingStacks;   // 剩余层数（可堆叠类）或 1/0（不可堆叠类）
        public bool isUnlocked;       // 是否已解锁（用于非持续关联判断）
        public bool isInitial;        // 是否为初始卡牌

        public bool IsDepleted => remainingStacks <= 0;
    }

    /// <summary>
    /// 卡牌管理器 — 管理全局卡牌池、发牌、关联、顶替、堆叠
    /// </summary>
    public class CardManager : SingletonAutoMono<CardManager>
    {
        [Header("全局卡牌池")]
        [SerializeField] private List<int> pooledCardIds = new List<int>();
        private Dictionary<int, PooledCard> globalPool = new Dictionary<int, PooledCard>();

        [Header("历史记录")]
        [SerializeField] private List<CardUseRecord> cardHistory = new List<CardUseRecord>();

        /// <summary>已解锁的非持续关联卡牌ID集合（用于判断"是否已被解锁过"）</summary>
        private HashSet<int> unlockedNonPersistentIds = new HashSet<int>();

        /// <summary>非持续关联卡牌的持久层数（跨盖回被子循环保留）</summary>
        private Dictionary<int, int> savedNonPersistentStacks = new Dictionary<int, int>();

        /// <summary>顶替映射：被消耗的卡牌ID → 其关联卡牌ID列表（永久生效）</summary>
        private Dictionary<int, List<int>> replaceMap = new Dictionary<int, List<int>>();

        private LevelConfigSO levelConfig;

        /// <summary>备选区变化回调（通知 GamePanel 刷新）</summary>
        public static event System.Action OnSelectionChanged;

        private Dictionary<int, CardData> cardDataCache = new Dictionary<int, CardData>();

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
        /// 初始化全局卡牌池
        /// </summary>
        public void Initialize(LevelConfigSO config)
        {
            if (config == null)
            {
                Debug.LogError("[CardManager] Initialize 失败：config 为 null");
                return;
            }

            levelConfig = config;
            globalPool.Clear();
            unlockedNonPersistentIds.Clear();
            savedNonPersistentStacks.Clear();
            replaceMap.Clear();
            cardHistory.Clear();
            pooledCardIds.Clear();

            handCards = new List<CardInstance>();
            cardHistory = new List<CardUseRecord>();
            nextInstanceId = 1;

            // 预加载并缓存所有卡牌数据
            BuildCardDataCache();

            // 发放初始卡牌
            DealInitialCards();

            // 通知UI刷新备选区
            OnSelectionChanged?.Invoke();

            Debug.Log($"[CardManager] 初始化完成，全局池 {globalPool.Count} 张卡牌");
        }

        /// <summary>
        /// 发放初始卡牌到全局卡牌池
        /// </summary>
        private void DealInitialCards()
        {
            if (levelConfig == null || levelConfig.initialCards == null) return;

            foreach (int cardId in levelConfig.initialCards)
            {
                var data = GetCardDataById(cardId);
                if (data == null) continue;

                if (globalPool.ContainsKey(cardId))
                {
                    // 持续关联类：重置为初始层数
                    if (data.relatedType == RelatedType.Persistent)
                    {
                        globalPool[cardId].remainingStacks = data.initialStack;
                        globalPool[cardId].isUnlocked = true;
                        globalPool[cardId].isInitial = true;
                    }
                }
                else
                {
                    var pooled = new PooledCard
                    {
                        data = data,
                        remainingStacks = data.cardType == CardType.Stackable ? data.initialStack : 1,
                        isUnlocked = true,
                        isInitial = true
                    };
                    globalPool.Add(cardId, pooled);
                    pooledCardIds.Add(cardId);
                }
            }
        }

        /// <summary>
        /// 获取当前备选区应显示的卡牌数据列表（已解锁 + 剩余 > 0），按ID升序
        /// </summary>
        public List<CardData> GetAvailableCards()
        {
            var result = new List<CardData>();
            foreach (var kvp in globalPool.OrderBy(k => k.Key))
            {
                if (kvp.Value.isUnlocked && !kvp.Value.IsDepleted)
                    result.Add(kvp.Value.data);
            }
            return result;
        }

        /// <summary>
        /// 获取指定卡牌在全局池中的剩余层数
        /// </summary>
        public int GetRemainingStacks(int cardId)
        {
            globalPool.TryGetValue(cardId, out var pooled);
            return pooled != null ? pooled.remainingStacks : 0;
        }

        /// <summary>
        /// 添加卡牌到手牌（关联触发时调用）
        /// </summary>
        public void AddCard(CardData cardData)
        {
            if (cardData == null) return;

            if (globalPool.ContainsKey(cardData.id))
            {
                // 非持续关联：已存在则不可逆，不增加层数
                if (cardData.relatedType == RelatedType.NonPersistent)
                    return;
                var existing = globalPool[cardData.id];
                if (cardData.cardType == CardType.Stackable)
                    existing.remainingStacks += cardData.initialStack;
            }
            else
            {
                var pooled = new PooledCard
                {
                    data = cardData,
                    remainingStacks = cardData.cardType == CardType.Stackable ? cardData.initialStack : 1,
                    isUnlocked = true,
                    isInitial = false
                };
                globalPool.Add(cardData.id, pooled);
                pooledCardIds.Add(cardData.id);
            }
        }

        /// <summary>
        /// 卡牌读条完成 — 消耗卡牌并触发关联
        /// </summary>
        public void OnCardUsed(CardData usedCard)
        {
            if (usedCard == null) return;

            // 1. 消耗卡牌
            if (usedCard.cardType == CardType.Stackable)
            {
                // 可堆叠类：扣除层数
                if (globalPool.TryGetValue(usedCard.id, out var pooled))
                {
                    pooled.remainingStacks--;
                    if (pooled.IsDepleted)
                    {
                        // 非持续关联被消耗时触发顶替
                        if (usedCard.relatedType == RelatedType.NonPersistent
                            && usedCard.relatedCardIds != null
                            && usedCard.relatedCardIds.Count > 0)
                        {
                            ApplyReplaceToParents(usedCard);
                        }
                    }
                }
            }
            else
            {
                // 不可堆叠类：标记为已消耗
                if (globalPool.TryGetValue(usedCard.id, out var pooled))
                {
                    pooled.remainingStacks = 0;
                    if (usedCard.relatedType == RelatedType.NonPersistent
                        && usedCard.relatedCardIds != null
                        && usedCard.relatedCardIds.Count > 0)
                    {
                        ApplyReplaceToParents(usedCard);
                    }
                }
            }

            // 2. 触发关联卡牌
            TriggerLinkedCards(usedCard);

            // 3. 盖回被子(2011)：使用后只保留关联卡牌，其余从池中移除
            if (usedCard.id == 2011 && usedCard.relatedCardIds != null)
            {
                var keepSet = new HashSet<int>(usedCard.relatedCardIds);
                var toRemove = new List<int>();
                foreach (var kvp in globalPool)
                {
                    if (!keepSet.Contains(kvp.Key))
                    {
                        // 非持续关联卡牌：移除前保存当前层数，下次生成时恢复
                        if (kvp.Value.data.relatedType == RelatedType.NonPersistent)
                            savedNonPersistentStacks[kvp.Key] = kvp.Value.remainingStacks;
                        toRemove.Add(kvp.Key);
                    }
                }
                foreach (var id in toRemove)
                {
                    globalPool.Remove(id);
                    pooledCardIds.Remove(id);
                }
            }

            // 4. 记录历史
            RecordCardUse(usedCard, true);

            OnSelectionChanged?.Invoke();
        }

        /// <summary>
        /// 卡牌被打断 — 退回全局池，不触发关联，不触发顶替
        /// </summary>
        public void OnCardInterrupted(CardData card)
        {
            if (card == null) return;

            if (card.cardType == CardType.Stackable)
            {
                if (globalPool.TryGetValue(card.id, out var pooled))
                {
                    pooled.remainingStacks++;
                }
            }

            RecordCardUse(card, false);
            OnSelectionChanged?.Invoke();
        }

        /// <summary>
        /// 触发关联卡牌：筛选关联列表中「已解锁且剩余 > 0」的卡牌
        /// </summary>
        public void TriggerLinkedCards(CardData usedCard)
        {
            if (usedCard == null || usedCard.relatedCardIds == null) return;

            foreach (int linkedId in usedCard.relatedCardIds)
            {
                var linkedData = GetCardDataById(linkedId);
                if (linkedData == null) continue;

                if (globalPool.TryGetValue(linkedId, out var pooled))
                {
                    // 已在全局池中
                    if (usedCard.relatedType == RelatedType.Persistent)
                    {
                        // 持续关联：层数=0时重置
                        if (pooled.IsDepleted)
                            pooled.remainingStacks = linkedData.initialStack;
                    }
                    // 非持续关联：已解锁则不做任何操作
                }
                else
                {
                    // 不在全局池中 → 首次解锁（或盖回被子后重新生成）
                    int stacks;
                    if (linkedData.relatedType == RelatedType.NonPersistent
                        && savedNonPersistentStacks.TryGetValue(linkedId, out int saved))
                    {
                        stacks = saved;
                        savedNonPersistentStacks.Remove(linkedId);
                    }
                    else
                    {
                        stacks = linkedData.cardType == CardType.Stackable
                            ? linkedData.initialStack : 1;
                    }

                    var newPooled = new PooledCard
                    {
                        data = linkedData,
                        remainingStacks = stacks,
                        isUnlocked = true,
                        isInitial = false
                    };
                    globalPool.Add(linkedId, newPooled);
                    pooledCardIds.Add(linkedId);

                    if (linkedData.relatedType == RelatedType.NonPersistent)
                        unlockedNonPersistentIds.Add(linkedId);
                }
            }
        }

        /// <summary>
        /// 顶替机制：被消耗的非持续关联卡牌的关联卡牌 永久加入所有父卡牌的关联列表
        /// </summary>
        private void ApplyReplaceToParents(CardData consumedCard)
        {
            replaceMap[consumedCard.id] = new List<int>(consumedCard.relatedCardIds ?? new List<int>());

            // 找到所有在关联列表中包含此卡牌的父卡牌
            foreach (var kvp in globalPool)
            {
                var parentData = kvp.Value.data;
                if (parentData == null || parentData.relatedCardIds == null) continue;
                if (!parentData.relatedCardIds.Contains(consumedCard.id)) continue;

                // 移除被消耗的卡牌
                parentData.relatedCardIds.Remove(consumedCard.id);
                // 添加被消耗卡牌的关联卡牌
                foreach (int replaceId in consumedCard.relatedCardIds ?? new List<int>())
                {
                    if (!parentData.relatedCardIds.Contains(replaceId))
                        parentData.relatedCardIds.Add(replaceId);
                }
            }
        }

        /// <summary>
        /// 被抓时：清空备选区，重新发放初始卡牌
        /// </summary>
        public void OnCaught()
        {
            // 清空备选区（仅隐藏，不删除全局池数据）
            // 重新发放初始卡牌
            foreach (int cardId in levelConfig.initialCards ?? new List<int>())
            {
                if (globalPool.TryGetValue(cardId, out var pooled))
                {
                    var data = pooled.data;
                    if (data.relatedType == RelatedType.Persistent)
                    {
                        // 持续关联类：统一重置为初始层数
                        pooled.remainingStacks = data.initialStack;
                    }
                }
            }

            OnSelectionChanged?.Invoke();
        }

        /// <summary>
        /// 完全重置（退出关卡/重进时）
        /// </summary>
        public void FullReset()
        {
            globalPool.Clear();
            unlockedNonPersistentIds.Clear();
            savedNonPersistentStacks.Clear();
            replaceMap.Clear();
            cardHistory.Clear();
            pooledCardIds.Clear();
        }

        /// <summary>
        /// 获取卡牌数据（从缓存中）
        /// </summary>
        public CardData GetCardDataById(int cardId)
        {
            if (cardDataCache.TryGetValue(cardId, out CardData data))
            {
                return data;
            }

            Debug.LogWarning($"[CardManager] 未找到ID为 {cardId} 的卡牌数据");
            return null;
        }

        /// <summary>
        /// 记录卡牌使用历史
        /// </summary>
        public void RecordCardUse(CardData card, bool success)
        {
            if (card == null) return;
            var record = new CardUseRecord(card.id, card.cardName, success);
            cardHistory.Add(record);
        }

        private void RecordCardUse(CardInstance instance, bool success)
        {
            if (instance == null || instance.data == null) return;
            var record = new CardUseRecord(instance.data.id, instance.data.cardName, success);
            cardHistory.Add(record);
        }

        /// <summary>
        /// 获取卡牌使用历史
        /// </summary>
        public List<CardUseRecord> GetCardHistory()
        {
            return new List<CardUseRecord>(cardHistory);
        }

        /// <summary>
        /// 获取手牌数量（全局池中已解锁且未耗尽的卡牌数）
        /// </summary>
        public int GetAvailableCardCount()
        {
            int count = 0;
            foreach (var kvp in globalPool)
            {
                if (kvp.Value.isUnlocked && !kvp.Value.IsDepleted)
                    count++;
            }
            return count;
        }

        // ==================== 向后兼容（测试器用）====================

        /// <summary>
        /// 获取全局池中卡牌总数（兼容旧API）
        /// </summary>
        public int GetHandCardCount()
        {
            return globalPool.Count;
        }

        /// <summary>
        /// 获取全局池中所有卡牌的 CardInstance 列表（兼容旧API）
        /// </summary>
        public List<CardInstance> GetHandCards()
        {
            var list = new List<CardInstance>();
            foreach (var kvp in globalPool)
            {
                var inst = new CardInstance(kvp.Value.data, kvp.Key);
                inst.isUsed = kvp.Value.IsDepleted;
                list.Add(inst);
            }
            return list;
        }

        /// <summary>
        /// 移除卡牌（兼容旧API）
        /// </summary>
        public void RemoveCard(CardInstance instance)
        {
            if (instance == null || instance.data == null) return;
            if (globalPool.TryGetValue(instance.data.id, out var pooled))
            {
                pooled.remainingStacks = 0;
            }
        }

        /// <summary>
        /// 预加载并缓存所有卡牌数据
        /// </summary>
        void BuildCardDataCache()
        {
            cardDataCache.Clear();
            CardDataContainer[] containers = Resources.LoadAll<CardDataContainer>("TestData");
            foreach (var container in containers)
            {
                if (container.cardData != null && !cardDataCache.ContainsKey(container.cardData.id))
                {
                    cardDataCache.Add(container.cardData.id, container.cardData);
                }
            }
        }

        /// <summary>
        /// 弃牌：保留指定卡牌，移除其他（兼容旧API）
        /// </summary>
        public void DiscardOtherCards(int keepCardId)
        {
            foreach (var kvp in globalPool)
            {
                if (kvp.Key != keepCardId && !kvp.Value.IsDepleted)
                {
                    kvp.Value.remainingStacks = 0;
                }
            }
            Debug.Log($"[CardManager] 弃牌: 保留卡牌ID {keepCardId}");
            OnSelectionChanged?.Invoke();
        }
    }
}
