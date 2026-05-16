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
        public float savedReadTime;   // 打断时保存的读条进度（saveProgressOnInterrupt）
        public int savedSegmentIndex; // 打断时保存的片段索引
        public List<int> runtimeRelatedCardIds; // 运行时关联列表副本（避免修改ScriptableObject）

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

        /// <summary>运行时额外关联：卡牌ID → 临时追加的关联ID列表（影响生成+保留）</summary>
        private Dictionary<int, List<int>> runtimeExtraAssociations = new Dictionary<int, List<int>>();

        /// <summary>运行时保留关联：卡牌ID → 仅保留可见、不触发生成的ID列表</summary>
        private Dictionary<int, List<int>> runtimeKeepOnly = new Dictionary<int, List<int>>();

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
            runtimeExtraAssociations.Clear();
            runtimeKeepOnly.Clear();
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
                        var existing = globalPool[cardId];
                        existing.remainingStacks = data.initialStack;
                        existing.isUnlocked = true;
                        existing.isInitial = true;
                        existing.runtimeRelatedCardIds = data.relatedCardIds != null
                            ? new List<int>(data.relatedCardIds) : new List<int>();
                    }
                }
                else
                {
                    var pooled = new PooledCard
                    {
                        data = data,
                        remainingStacks = data.cardType == CardType.Stackable ? data.initialStack : 1,
                        isUnlocked = true,
                        isInitial = true,
                        runtimeRelatedCardIds = data.relatedCardIds != null
                            ? new List<int>(data.relatedCardIds) : new List<int>()
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
            var effectiveIds = GetEffectiveRelatedIds(usedCard);
            if (usedCard.relatedType == RelatedType.Persistent && effectiveIds != null && effectiveIds.Count > 0)
            {
                var keepSet = new HashSet<int>(ResolveLinkedIds(effectiveIds));
                // 合并运行时额外关联（仅本局有效，不修改ScriptableObject）
                if (runtimeExtraAssociations.TryGetValue(usedCard.id, out var extras))
                {
                    foreach (int extra in extras)
                        keepSet.Add(extra);
                }
                if (runtimeKeepOnly.TryGetValue(usedCard.id, out var keepOnlyList))
                {
                    foreach (int id in keepOnlyList)
                        keepSet.Add(id);
                }
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

            // remainingStacks 在 OnCardUsed 成功时才 -- ，打断无需回退
            RecordCardUse(card, false);
            OnSelectionChanged?.Invoke();
        }

        /// <summary>保存读条进度（打断时调用）</summary>
        public void SaveReadProgress(int cardId, float readTime, int segmentIndex)
        {
            if (globalPool.TryGetValue(cardId, out var pooled))
            {
                pooled.savedReadTime = readTime;
                pooled.savedSegmentIndex = segmentIndex;
            }
        }

        /// <summary>读取并清除已保存的读条进度</summary>
        public bool PopSavedReadProgress(int cardId, out float readTime, out int segmentIndex)
        {
            if (globalPool.TryGetValue(cardId, out var pooled)
                && pooled.savedReadTime > 0f)
            {
                readTime = pooled.savedReadTime;
                segmentIndex = pooled.savedSegmentIndex;
                pooled.savedReadTime = 0f;
                pooled.savedSegmentIndex = 0;
                return true;
            }
            readTime = 0f;
            segmentIndex = 0;
            return false;
        }

        /// <summary>清除已保存的读条进度（读条完成时调用）</summary>
        public void ClearSavedReadProgress(int cardId)
        {
            if (globalPool.TryGetValue(cardId, out var pooled))
            {
                pooled.savedReadTime = 0f;
                pooled.savedSegmentIndex = 0;
            }
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
            var effectiveIds = GetEffectiveRelatedIds(usedCard);
            if (usedCard == null || effectiveIds == null) return;

            // 递归解析顶替链（使用运行时副本）
            var resolvedIds = ResolveLinkedIds(effectiveIds);
            // 合并运行时额外关联（仅本局有效）
            if (runtimeExtraAssociations.TryGetValue(usedCard.id, out var extras))
            {
                foreach (int extra in extras)
                    if (!resolvedIds.Contains(extra))
                        resolvedIds.Add(extra);
            }

            Debug.Log($"[CardManager] TriggerLinkedCards: usedCard={usedCard.id}({usedCard.cardName}), effectiveIds=[{string.Join(",", effectiveIds)}], resolvedIds=[{string.Join(",", resolvedIds)}], relatedType={usedCard.relatedType}");

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
                        isInitial = false,
                        runtimeRelatedCardIds = linkedData.relatedCardIds != null
                            ? new List<int>(linkedData.relatedCardIds) : new List<int>()
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
        /// 获取卡牌的运行时关联列表（优先返回PooledCard中的运行时副本，避免修改ScriptableObject）
        /// </summary>
        private List<int> GetEffectiveRelatedIds(CardData data)
        {
            if (data == null) return new List<int>();
            if (globalPool.TryGetValue(data.id, out var pooled) && pooled.runtimeRelatedCardIds != null)
                return pooled.runtimeRelatedCardIds;
            return data.relatedCardIds ?? new List<int>();
        }

        /// <summary>
        /// 顶替机制：被消耗的非持续关联卡牌的关联卡牌 永久加入所有父卡牌的关联列表
        /// </summary>
        private void ApplyReplaceToParents(CardData consumedCard)
        {
            var consumedIds = GetEffectiveRelatedIds(consumedCard);
            Debug.Log($"[CardManager] ApplyReplaceToParents: consumedCard={consumedCard.id}, relatedCardIds before=[{string.Join(",", consumedIds)}]");
            replaceMap[consumedCard.id] = new List<int>(consumedIds);

            // 找到所有在关联列表中包含此卡牌的父卡牌（读取运行时副本）
            foreach (var kvp in globalPool)
            {
                var parentIds = GetEffectiveRelatedIds(kvp.Value.data);
                if (parentIds == null || !parentIds.Contains(consumedCard.id)) continue;

                Debug.Log($"[CardManager] ApplyReplaceToParents: 找到父卡牌 {kvp.Key}({kvp.Value.data.cardName}), 其relatedCardIds=[{string.Join(",", parentIds)}]");

                // 修改运行时副本，不再修改ScriptableObject
                parentIds.Remove(consumedCard.id);
                foreach (int replaceId in consumedIds)
                {
                    if (!parentIds.Contains(replaceId))
                        parentIds.Add(replaceId);
                }
                Debug.Log($"[CardManager] ApplyReplaceToParents: 父卡牌 {kvp.Key} 修改后 relatedCardIds=[{string.Join(",", parentIds)}]");
            }
            Debug.Log($"[CardManager] ApplyReplaceToParents: 完成, consumedCard.relatedCardIds after=[{string.Join(",", consumedIds)}]");
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
        /// 添加运行时额外关联（仅本局有效，不修改ScriptableObject资产）
        /// </summary>
        private void AddRuntimeAssociation(int cardId, int extraId)
        {
            if (!runtimeExtraAssociations.TryGetValue(cardId, out var list))
            {
                list = new List<int>();
                runtimeExtraAssociations[cardId] = list;
            }
            if (!list.Contains(extraId))
            {
                list.Add(extraId);
                Debug.Log($"[CardManager] AddRuntimeAssociation: {extraId} 加入 runtimeExtras[{cardId}], 列表=[{string.Join(",", list)}]");
            }
        }

        /// <summary>仅保留可见，不触发生成</summary>
        private void AddKeepOnly(int cardId, int keepId)
        {
            if (!runtimeKeepOnly.TryGetValue(cardId, out var list))
            {
                list = new List<int>();
                runtimeKeepOnly[cardId] = list;
            }
            if (!list.Contains(keepId))
            {
                list.Add(keepId);
                Debug.Log($"[CardManager] AddKeepOnly: {keepId} 加入 keepOnly[{cardId}]");
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
                        AddKeepOnly(2002, 2015);
                        Debug.Log("[CardManager] 特殊效果: 酱包(2013)+海苔包(2014)均已使用，生成搅拌拌面(2015)，加入2002运行时关联");
                    }
                    break;

                case 2014: // 加入海苔包：如果酱包(2013)已用过，则额外生成搅拌(2015)
                    if (usedCardIds.Contains(2013))
                    {
                        UnlockCardById(2015);
                        AddKeepOnly(2002, 2015);
                        Debug.Log("[CardManager] 特殊效果: 海苔包(2014)+酱包(2013)均已使用，生成搅拌拌面(2015)，加入2002运行时关联");
                    }
                    break;

                case 2015: // 搅拌拌面：2016由关联列表生成；2017加入2003运行时关联；重置2003
                {
                    AddRuntimeAssociation(2003, 2017);
                    AddKeepOnly(2002, 2016); // 2016也加入2002关联，避免经由2011→2002后丢失
                    if (globalPool.TryGetValue(2003, out var p2003))
                    {
                        p2003.remainingStacks = 1;
                        p2003.isUnlocked = true;
                    }
                    Debug.Log("[CardManager] 特殊效果: 搅拌完成，2003已重置，2017已加入2003运行时关联");
                    break;
                }

                case 2017: // 分享拌面：2018由关联列表生成；特殊效果处理2016减少+暂停+对话
                {
                    // 揭示隐藏任务（请宋明月吃泡面）
                    Task.TaskManager.Instance.RevealHiddenTask(2017);
                    AddKeepOnly(2003, 2018); // 2018仅属于2003视角，切换回来后保留
                    if (globalPool.TryGetValue(2016, out var pooled2016))
                    {
                        pooled2016.remainingStacks = Mathf.Max(0, pooled2016.remainingStacks - 1);
                        pooled2016.isUnlocked = false;
                        // 通知任务系统：2016消耗一层（等同于使用一次吃拌面）
                        Task.TaskManager.Instance.HandleCardCompleted(2016);
                    }
                    // 保存并暂停2016生成
                    if (!isCard2016GenerationPaused)
                    {
                        savedCard2016Stacks = pooled2016?.remainingStacks ?? 0;
                        isCard2016GenerationPaused = true;
                    }
                    // 触发局内剧情对话
                    EventCenter.Instance.EventTrigger(E_EventType.GameDialogueStart, "Dialogue/Dialogue2");
                    Debug.Log("[CardManager] 特殊效果: 分享拌面 → 2016层数-1且隐藏, 暂停生成, 触发对话");
                    break;
                }

                case 2018: // 拿回拌面：解除2016生成暂停（2016仅在2002视角下可见）
                {
                    isCard2016GenerationPaused = false;
                    Debug.Log("[CardManager] 特殊效果: 拿回拌面 → 解除2016生成暂停");
                    break;
                }

                case 2025:
                {
                    if (levelConfig != null && levelConfig.levelId == 1005)
                    {
                        // 第五夜：拿走卫生纸 → 将2038(前往厕所)加入2005(下床)的运行时关联
                        AddRuntimeAssociation(2005, 2038);
                        Debug.Log("[CardManager] 特殊效果: 拿走卫生纸(2025) → 2005(下床)运行时关联新增 2038(前往厕所)");
                    }
                    else
                    {
                        // 第三夜：吃面包 → 使用后将2026(拿走面包)隐藏且不再生成
                        if (globalPool.TryGetValue(2026, out var pooled2026))
                        {
                            pooled2026.remainingStacks = 0;
                            pooled2026.isUnlocked = false;
                        }
                        Debug.Log("[CardManager] 特殊效果: 吃面包(2025) → 隐藏2026(拿走面包)");
                    }
                    break;
                }

                case 2026:
                {
                    // 揭示隐藏任务
                    Task.TaskManager.Instance.RevealHiddenTask(2026);
                    if (levelConfig != null && levelConfig.levelId == 1005)
                    {
                        // 第五夜：寻求宋明月帮助 → 将2038(前往厕所)加入2005(下床)的运行时关联
                        AddRuntimeAssociation(2005, 2038);
                        Debug.Log("[CardManager] 特殊效果: 寻求宋明月帮助(2026) → 2005(下床)运行时关联新增 2038(前往厕所)");

                        // 设置对话背景：5012(宋明月大图)，对话结束后回到5011(看向床对面宋明月)
                        GamePanel.DialogueBackgroundOverride = 5012;
                        GamePanel.PostDialogueBackgroundOverride = 5011;

                        // 触发局内对话
                        EventCenter.Instance.EventTrigger(E_EventType.GameDialogueStart, "Dialogue/Dialogue5-3");
                    }
                    else
                    {
                        // 第三夜：拿走面包 → 如果2025(吃面包)未被使用过，将2025加入2001的关联列表
                        if (!usedCardIds.Contains(2025))
                        {
                            var card2001 = GetCardDataById(2001);
                            var ids2001 = GetEffectiveRelatedIds(card2001);
                            if (ids2001 != null && !ids2001.Contains(2025))
                            {
                                ids2001.Add(2025);
                                Debug.Log("[CardManager] 特殊效果: 拿走面包(2026) → 2025未用过，将2025加入2001(掀开被子)的关联列表");
                            }
                            if (globalPool.TryGetValue(2025, out var pooled2025))
                            {
                                pooled2025.isUnlocked = false;
                                Debug.Log("[CardManager] 特殊效果: 拿走面包(2026) → 隐藏2025，需经2030→2001路径重新获取");
                            }
                            var card2024 = GetCardDataById(2024);
                            var ids2024 = GetEffectiveRelatedIds(card2024);
                            if (ids2024 != null && ids2024.Contains(2025))
                            {
                                ids2024.Remove(2025);
                                Debug.Log("[CardManager] 特殊效果: 拿走面包(2026) → 从2024(打开储物柜)关联列表中移除2025");
                            }
                        }
                        else
                        {
                            Debug.Log("[CardManager] 特殊效果: 拿走面包(2026) → 2025已用过，不执行关联注入");
                        }
                    }
                    break;
                }

                case 2040: // 前往走廊：暂停老师的查寝逻辑
                {
                    Game.AI.TeacherAI.IsPatrolPaused = true;
                    Debug.Log("[CardManager] 特殊效果: 前往走廊(2040) → 暂停老师查寝");
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
                isInitial = false,
                runtimeRelatedCardIds = data.relatedCardIds != null
                    ? new List<int>(data.relatedCardIds) : new List<int>()
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
            runtimeExtraAssociations.Clear();
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
