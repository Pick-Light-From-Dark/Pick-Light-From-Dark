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

        /// <summary>本局已成功使用的卡牌ID集合（用于条件生成判定）</summary>
        private HashSet<int> usedCardIds = new HashSet<int>();

        /// <summary>卡牌2016生成是否被暂停（分享拌面使用后）</summary>
        private bool isCard2016GenerationPaused = false;

        /// <summary>暂停前2016的堆叠层数</summary>
        private int savedCard2016Stacks = 0;


        /// <summary>备选区变化回调（通知 GamePanel 刷新）</summary>
        public static event System.Action OnSelectionChanged;

        private Dictionary<int, CardData> cardDataCache = new Dictionary<int, CardData>();

        private List<CardInstance> handCards = new List<CardInstance>();
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
            usedCardIds.Clear();
            isCard2016GenerationPaused = false;
            savedCard2016Stacks = 0;

            // 预加载并缓存所有卡牌数据
            BuildCardDataCache(config.cardDataPath);

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
                            && usedCard.relatedCardIds != null)
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
                        && usedCard.relatedCardIds != null)
                    {
                        ApplyReplaceToParents(usedCard);
                    }
                }
            }

            // 2. 记录已使用的卡牌ID（用于条件生成判定）
            usedCardIds.Add(usedCard.id);

            // 3. 触发关联卡牌
            TriggerLinkedCards(usedCard);

            // 4. 处理卡牌特殊效果（条件生成、动态关联、特殊消耗等）
            HandleSpecialEffects(usedCard);

            // 5. 持续关联卡牌：使用后隐藏非关联卡牌，使用递归解析后的ID做keepSet
            if (usedCard.relatedType == RelatedType.Persistent && usedCard.relatedCardIds != null && usedCard.relatedCardIds.Count > 0)
            {
                var keepSet = new HashSet<int>(ResolveLinkedIds(usedCard.relatedCardIds));
                // 2016生成暂停时从keepSet中移除，保持隐藏
                if (isCard2016GenerationPaused)
                    keepSet.Remove(2016);
                foreach (var kvp in globalPool)
                {
                    kvp.Value.isUnlocked = keepSet.Contains(kvp.Key);
                }
            }

            // 6. 记录历史
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
        /// 递归解析关联卡牌ID，将通过顶替机制消耗的卡牌替换为其关联卡牌
        /// </summary>
        private List<int> ResolveLinkedIds(List<int> rawIds)
        {
            var result = new List<int>();
            var visited = new HashSet<int>();
            Debug.Log($"[CardManager] ResolveLinkedIds: 输入 rawIds=[{string.Join(",", rawIds)}], replaceMap keys=[{string.Join(",", replaceMap.Keys)}]");
            foreach (int id in rawIds)
                ExpandReplaceMap(id, result, visited);
            Debug.Log($"[CardManager] ResolveLinkedIds: 输出 result=[{string.Join(",", result)}]");
            return result;
        }

        private void ExpandReplaceMap(int cardId, List<int> result, HashSet<int> visited)
        {
            if (visited.Contains(cardId))
            {
                Debug.Log($"[CardManager] ExpandReplaceMap: {cardId} 已访问, 跳过");
                return;
            }
            visited.Add(cardId);

            if (replaceMap.TryGetValue(cardId, out var replacements))
            {
                Debug.Log($"[CardManager] ExpandReplaceMap: {cardId} 在replaceMap中 → 展开 [{string.Join(",", replacements)}]");
                foreach (int r in replacements)
                    ExpandReplaceMap(r, result, visited);
            }
            else
            {
                if (!result.Contains(cardId))
                {
                    result.Add(cardId);
                    Debug.Log($"[CardManager] ExpandReplaceMap: {cardId} 不在replaceMap中 → 加入结果");
                }
            }
        }

        /// <summary>
        /// 触发关联卡牌：筛选关联列表中「已解锁且剩余 > 0」的卡牌
        /// </summary>
        public void TriggerLinkedCards(CardData usedCard)
        {
            if (usedCard == null || usedCard.relatedCardIds == null) return;

            // 递归解析顶替链
            var resolvedIds = ResolveLinkedIds(usedCard.relatedCardIds);

            Debug.Log($"[CardManager] TriggerLinkedCards: usedCard={usedCard.id}({usedCard.cardName}), rawIds=[{string.Join(",", usedCard.relatedCardIds)}], resolvedIds=[{string.Join(",", resolvedIds)}], relatedType={usedCard.relatedType}");

            foreach (int linkedId in resolvedIds)
            {
                // 2016生成暂停检查（分享拌面使用后暂停，拿回拌面使用后恢复）
                if (linkedId == 2016 && isCard2016GenerationPaused)
                {
                    Debug.Log("[CardManager] TriggerLinkedCards: 2016(吃拌面) 生成已暂停，跳过");
                    continue;
                }

                var linkedData = GetCardDataById(linkedId);
                if (linkedData == null)
                {
                    Debug.LogWarning($"[CardManager] TriggerLinkedCards: 卡牌ID {linkedId} 的CardData未找到，跳过！");
                    continue;
                }

                if (globalPool.TryGetValue(linkedId, out var pooled))
                {
                    // 已在全局池中 → 解锁（可能在之前的视角切换中被隐藏）
                    pooled.isUnlocked = true;
                    if (usedCard.relatedType == RelatedType.Persistent
                        && linkedData.relatedType == RelatedType.Persistent)
                    {
                        // 仅持续关联卡牌：层数=0时重置（非持续关联卡牌不重置，由其顶替链处理）
                        if (pooled.IsDepleted)
                            pooled.remainingStacks = linkedData.initialStack;
                    }
                    Debug.Log($"[CardManager] TriggerLinkedCards: {linkedId}({linkedData.cardName}) 已在池中, isDepleted={pooled.IsDepleted}, stacks={pooled.remainingStacks}");
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

                    Debug.Log($"[CardManager] TriggerLinkedCards: {linkedId}({linkedData.cardName}) 新加入池中, stacks={stacks}");
                }
            }

            // 打印池中所有可用卡牌
            var available = GetAvailableCards();
            Debug.Log($"[CardManager] 池中可用卡牌({available.Count}张): [{string.Join(", ", available.ConvertAll(c => $"{c.id}({c.cardName})"))}]");
        }

        /// <summary>
        /// 顶替机制：被消耗的非持续关联卡牌的关联卡牌 永久加入所有父卡牌的关联列表
        /// </summary>
        private void ApplyReplaceToParents(CardData consumedCard)
        {
            Debug.Log($"[CardManager] ApplyReplaceToParents: consumedCard={consumedCard.id}, relatedCardIds before=[{string.Join(",", consumedCard.relatedCardIds ?? new List<int>())}]");
            replaceMap[consumedCard.id] = new List<int>(consumedCard.relatedCardIds ?? new List<int>());

            // 找到所有在关联列表中包含此卡牌的父卡牌
            foreach (var kvp in globalPool)
            {
                var parentData = kvp.Value.data;
                if (parentData == null || parentData.relatedCardIds == null) continue;
                if (!parentData.relatedCardIds.Contains(consumedCard.id)) continue;

                Debug.Log($"[CardManager] ApplyReplaceToParents: 找到父卡牌 {kvp.Key}({parentData.cardName}), 其relatedCardIds=[{string.Join(",", parentData.relatedCardIds)}]");

                // 移除被消耗的卡牌
                parentData.relatedCardIds.Remove(consumedCard.id);
                // 添加被消耗卡牌的关联卡牌
                foreach (int replaceId in consumedCard.relatedCardIds ?? new List<int>())
                {
                    if (!parentData.relatedCardIds.Contains(replaceId))
                        parentData.relatedCardIds.Add(replaceId);
                }
                Debug.Log($"[CardManager] ApplyReplaceToParents: 父卡牌 {kvp.Key} 修改后 relatedCardIds=[{string.Join(",", parentData.relatedCardIds)}]");
            }
            Debug.Log($"[CardManager] ApplyReplaceToParents: 完成, consumedCard.relatedCardIds after=[{string.Join(",", consumedCard.relatedCardIds ?? new List<int>())}]");
        }

        /// <summary>
        /// 将新卡牌ID加入指定父卡牌的顶替链（用于特殊效果生成的卡牌，父卡牌的replaceMap不存在则创建）
        /// </summary>
        private void AddToReplaceChain(int parentId, int newId)
        {
            if (!replaceMap.TryGetValue(parentId, out var list))
            {
                list = new List<int>();
                replaceMap[parentId] = list;
            }
            if (!list.Contains(newId))
            {
                list.Add(newId);
                Debug.Log($"[CardManager] AddToReplaceChain: 将 {newId} 加入 replaceMap[{parentId}], 当前列表=[{string.Join(",", list)}]");
            }
        }

        /// <summary>
        /// 处理卡牌特殊效果（测试案v2定义的条件生成、动态关联、特殊消耗等）
        /// </summary>
        private void HandleSpecialEffects(CardData usedCard)
        {
            switch (usedCard.id)
            {
                case 2013: // 加入酱包：如果海苔包(2014)已用过，则额外生成搅拌(2015)
                    if (usedCardIds.Contains(2014))
                    {
                        UnlockCardById(2015);
                        // 将2015加入自己和2014的replaceMap，确保通过任意路径都能解析到2015
                        AddToReplaceChain(2013, 2015);
                        AddToReplaceChain(2014, 2015);
                        Debug.Log("[CardManager] 特殊效果: 酱包(2013)+海苔包(2014)均已使用，生成搅拌拌面(2015)并加入顶替链");
                    }
                    break;

                case 2014: // 加入海苔包：如果酱包(2013)已用过，则额外生成搅拌(2015)
                    if (usedCardIds.Contains(2013))
                    {
                        UnlockCardById(2015);
                        AddToReplaceChain(2013, 2015);
                        AddToReplaceChain(2014, 2015);
                        Debug.Log("[CardManager] 特殊效果: 海苔包(2014)+酱包(2013)均已使用，生成搅拌拌面(2015)并加入顶替链");
                    }
                    break;

                case 2015: // 搅拌拌面：2016直接生成；2017加入2003导航路径；仅重置2003
                {
                    var card2002 = GetCardDataById(2002);
                    if (card2002 != null && card2002.relatedCardIds != null && !card2002.relatedCardIds.Contains(2016))
                    {
                        card2002.relatedCardIds.Add(2016);
                        Debug.Log("[CardManager] 特殊效果: 搅拌完成，2002(看向床下)关联列表新增 2016(吃拌面)");
                    }
                    var card2003 = GetCardDataById(2003);
                    if (card2003 != null && card2003.relatedCardIds != null && !card2003.relatedCardIds.Contains(2017))
                    {
                        card2003.relatedCardIds.Add(2017);
                        Debug.Log("[CardManager] 特殊效果: 搅拌完成，2003(看向床对面)关联列表新增 2017(分享拌面)");
                    }
                    if (globalPool.TryGetValue(2003, out var p2003))
                    {
                        p2003.remainingStacks = 1;
                        p2003.isUnlocked = true;
                    }
                    Debug.Log("[CardManager] 特殊效果: 搅拌完成，2003已重置");
                    break;
                }

                case 2017: // 分享拌面：2018由关联列表生成；特殊效果处理2016减少+暂停+对话
                {
                    // 减少2016(吃拌面)堆叠层数1，并从备选区隐藏
                    if (globalPool.TryGetValue(2016, out var pooled2016))
                    {
                        pooled2016.remainingStacks = Mathf.Max(0, pooled2016.remainingStacks - 1);
                        pooled2016.isUnlocked = false;
                    }
                    // 保存并暂停2016生成
                    if (!isCard2016GenerationPaused)
                    {
                        savedCard2016Stacks = pooled2016?.remainingStacks ?? 0;
                        isCard2016GenerationPaused = true;
                    }
                    // 触发局内剧情对话
                    EventCenter.Instance.EventTrigger(E_EventType.GameDialogueStart, "Dialogue2");
                    Debug.Log("[CardManager] 特殊效果: 分享拌面 → 2016层数-1且隐藏, 暂停生成, 触发对话");
                    break;
                }

                case 2018: // 拿回拌面：解除2016生成暂停，后续通过2002路径可重新生成2016
                {
                    isCard2016GenerationPaused = false;
                    Debug.Log("[CardManager] 特殊效果: 拿回拌面 → 解除2016生成暂停");
                    break;
                }
            }
        }

        /// <summary>
        /// 按ID解锁卡牌到全局池
        /// </summary>
        private void UnlockCardById(int cardId)
        {
            if (globalPool.ContainsKey(cardId)) return;

            var data = GetCardDataById(cardId);
            if (data == null) return;

            var pooled = new PooledCard
            {
                data = data,
                remainingStacks = data.cardType == CardType.Stackable ? data.initialStack : 1,
                isUnlocked = true,
                isInitial = false
            };
            globalPool.Add(cardId, pooled);
            pooledCardIds.Add(cardId);
        }

        /// <summary>
        /// 被抓时：清空备选区，重新发放初始卡牌
        /// </summary>
        public void OnCaught()
        {
            // 清空备选区（仅隐藏，不删除全局池数据）
            // 重新发放初始卡牌：解锁并重置，同时隐藏其他卡牌
            var initialSet = new HashSet<int>(levelConfig.initialCards ?? new List<int>());
            foreach (var kvp in globalPool)
            {
                if (initialSet.Contains(kvp.Key))
                {
                    kvp.Value.isUnlocked = true;
                    var data = kvp.Value.data;
                    if (data.relatedType == RelatedType.Persistent)
                        kvp.Value.remainingStacks = data.initialStack;
                }
                else
                {
                    kvp.Value.isUnlocked = false;
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
        void BuildCardDataCache(string path)
        {
            cardDataCache.Clear();
            if (!string.IsNullOrEmpty(path))
                LoadCardDataFromPath(path);
        }

        void LoadCardDataFromPath(string path)
        {
            CardDataContainer[] containers = Resources.LoadAll<CardDataContainer>(path);
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
