using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 结局分支判定系统 — 根据游玩过程中的状态数据判定应触发的结局
    /// 提供清晰的后端调用接口，供 EndingManager / LevelFlowCoordinator 使用
    /// </summary>
    public class EndingBranchSystem : MonoBehaviour
    {
        [Header("结局条件配置（按优先级排序）")]
        public List<EndingBranchCondition> branchConditions = new List<EndingBranchCondition>();

        [Header("调试")]
        public bool logEvaluation = true;

        // 游玩过程数据（由后端系统注入）
        private GameplayRecord gameplayRecord;

        /// <summary>存档系统预留接口：游玩数据更新时触发</summary>
        public System.Action<GameplayRecord> OnGameplayRecordUpdated;

        void Awake()
        {
            EnsureDefaultConditions();
        }

        /// <summary>
        /// 后端接口：初始化并注入游玩记录
        /// </summary>
        public void Initialize(GameplayRecord record)
        {
            gameplayRecord = record;
            if (logEvaluation)
                Debug.Log($"[EndingBranchSystem] 初始化完成 关卡={record?.levelId} 分支数={record?.branchChoices?.Count}");
        }

        /// <summary>
        /// 后端接口【主入口】：根据当前游玩记录判定结局
        /// 返回结局ID（6001~6005），无匹配时返回默认结局
        /// </summary>
        public int EvaluateEnding()
        {
            if (gameplayRecord == null)
            {
                Debug.LogWarning("[EndingBranchSystem] gameplayRecord 未设置，返回默认结局 6001");
                return GetDefaultEndingId();
            }

            // 按优先级顺序匹配条件
            for (int i = 0; i < branchConditions.Count; i++)
            {
                var condition = branchConditions[i];
                if (condition == null) continue;
                if (MatchesCondition(condition))
                {
                    if (logEvaluation)
                        Debug.Log($"[EndingBranchSystem] 匹配条件[{i}]: 结局 {condition.endingId} ({condition.comment})");
                    return condition.endingId;
                }
            }

            // 兜底：根据关键分支推断
            int inferredId = InferEndingFromBranches();
            if (inferredId > 0)
            {
                if (logEvaluation)
                    Debug.Log($"[EndingBranchSystem] 分支推断结局: {inferredId}");
                return inferredId;
            }

            // 默认结局
            if (logEvaluation)
                Debug.Log($"[EndingBranchSystem] 无匹配条件，返回默认结局 {GetDefaultEndingId()}");
            return GetDefaultEndingId();
        }

        /// <summary>
        /// 后端接口：直接设置游玩记录并判定（一次性调用）
        /// </summary>
        public int EvaluateEnding(GameplayRecord record)
        {
            Initialize(record);
            return EvaluateEnding();
        }

        /// <summary>
        /// 后端接口：检查指定结局是否可触发
        /// </summary>
        public bool CanTriggerEnding(int endingId)
        {
            if (gameplayRecord == null) return false;
            foreach (var condition in branchConditions)
            {
                if (condition != null && condition.endingId == endingId && MatchesCondition(condition))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 后端接口：获取所有满足条件的结局ID列表（可能多个）
        /// </summary>
        public List<int> GetAllPossibleEndings()
        {
            var result = new List<int>();
            if (gameplayRecord == null) return result;
            foreach (var condition in branchConditions)
            {
                if (condition != null && MatchesCondition(condition) && !result.Contains(condition.endingId))
                    result.Add(condition.endingId);
            }
            return result;
        }

        /// <summary>
        /// 后端接口：更新游玩记录中的分支选择
        /// </summary>
        public void RecordBranchChoice(string choiceId)
        {
            gameplayRecord?.AddBranchChoice(choiceId);
            OnGameplayRecordUpdated?.Invoke(gameplayRecord);
        }

        /// <summary>
        /// 后端接口：更新游玩记录中的卡牌使用
        /// </summary>
        public void RecordCardUsed(int cardId)
        {
            gameplayRecord?.AddUsedCard(cardId);
            OnGameplayRecordUpdated?.Invoke(gameplayRecord);
        }

        /// <summary>
        /// 后端接口：更新游玩记录中的道具收集
        /// </summary>
        public void RecordItemCollected(string itemId)
        {
            gameplayRecord?.AddCollectedItem(itemId);
            OnGameplayRecordUpdated?.Invoke(gameplayRecord);
        }

        /// <summary>
        /// 后端接口：设置最终状态值
        /// </summary>
        public void SetFinalState(int lives, int emotion)
        {
            if (gameplayRecord != null)
            {
                gameplayRecord.finalLives = lives;
                gameplayRecord.finalEmotion = emotion;
                OnGameplayRecordUpdated?.Invoke(gameplayRecord);
            }
        }

        /// <summary>
        /// 获取当前游玩记录（供存档系统读取）
        /// </summary>
        public GameplayRecord GetGameplayRecord()
        {
            return gameplayRecord;
        }

        bool MatchesCondition(EndingBranchCondition condition)
        {
            if (gameplayRecord == null) return false;

            // 关卡检查
            if (condition.requiredLevel > 0 && gameplayRecord.levelId != condition.requiredLevel)
                return false;

            // 分支检查
            if (!string.IsNullOrEmpty(condition.requiredBranchChoice))
            {
                bool hasBranch = false;
                foreach (var choice in gameplayRecord.branchChoices)
                {
                    if (choice == condition.requiredBranchChoice)
                    {
                        hasBranch = true;
                        break;
                    }
                }
                if (!hasBranch) return false;
            }

            // 排除分支检查
            if (!string.IsNullOrEmpty(condition.excludedBranchChoice))
            {
                foreach (var choice in gameplayRecord.branchChoices)
                {
                    if (choice == condition.excludedBranchChoice)
                        return false;
                }
            }

            // 生命值检查
            if (gameplayRecord.finalLives < condition.minLives || gameplayRecord.finalLives > condition.maxLives)
                return false;

            // 情绪值检查
            if (gameplayRecord.finalEmotion < condition.minEmotion || gameplayRecord.finalEmotion > condition.maxEmotion)
                return false;

            // 必须使用的卡牌检查
            foreach (var cardId in condition.requiredUsedCards)
            {
                if (!gameplayRecord.usedCards.Contains(cardId))
                    return false;
            }

            // 禁止使用的卡牌检查
            foreach (var cardId in condition.excludedUsedCards)
            {
                if (gameplayRecord.usedCards.Contains(cardId))
                    return false;
            }

            // 关键道具检查
            foreach (var item in condition.requiredItems)
            {
                if (!gameplayRecord.collectedItems.Contains(item))
                    return false;
            }

            return true;
        }

        int InferEndingFromBranches()
        {
            if (gameplayRecord == null || gameplayRecord.branchChoices == null) return 0;

            foreach (var choice in gameplayRecord.branchChoices)
            {
                switch (choice)
                {
                    case "not_eat": return 6001;
                    case "confused": return 6002;
                    case "internet": return 6003;
                    case "rooftop": return 6004;
                    case "friend": return 6005;
                }
            }
            return 0;
        }

        int GetDefaultEndingId() => 6001;

        /// <summary>
        /// 若条件列表为空，自动填充默认的5结局条件
        /// </summary>
        void EnsureDefaultConditions()
        {
            if (branchConditions == null || branchConditions.Count == 0)
            {
                branchConditions = new List<EndingBranchCondition>
                {
                    new EndingBranchCondition
                    {
                        endingId = 6001,
                        comment = "结局一：太阳照常升起 — 第一关选择不吃",
                        requiredBranchChoice = "not_eat",
                        requiredLevel = 1
                    },
                    new EndingBranchCondition
                    {
                        endingId = 6002,
                        comment = "结局二：莫比乌斯环 — 迷茫/未拿到钥匙",
                        requiredBranchChoice = "confused",
                        requiredLevel = 5,
                        maxLives = 2
                    },
                    new EndingBranchCondition
                    {
                        endingId = 6003,
                        comment = "结局三：人心不足蛇吞象 — 网吧结局",
                        requiredBranchChoice = "internet",
                        requiredLevel = 5,
                        requiredItems = new List<string> { "rooftop_key" }
                    },
                    new EndingBranchCondition
                    {
                        endingId = 6004,
                        comment = "结局四：星垂之夜 — 独自上天台",
                        requiredBranchChoice = "rooftop",
                        requiredLevel = 5,
                        minEmotion = 60,
                        requiredItems = new List<string> { "rooftop_key" }
                    },
                    new EndingBranchCondition
                    {
                        endingId = 6005,
                        comment = "结局五：北极星 — 邀友同行",
                        requiredBranchChoice = "friend",
                        requiredLevel = 5,
                        minEmotion = 60,
                        requiredItems = new List<string> { "rooftop_key" }
                    }
                };
            }
        }
    }

    /// <summary>
    /// 游玩记录 — 汇总一局游戏中的关键数据，供结局判定使用
    /// 可被序列化存档
    /// </summary>
    [System.Serializable]
    public class GameplayRecord
    {
        [Tooltip("关卡ID")]
        public int levelId;

        [Tooltip("分支选择记录（如 eat / not_eat / confused / internet / rooftop / friend）")]
        public List<string> branchChoices = new List<string>();

        [Tooltip("最终生命值")]
        public int finalLives;

        [Tooltip("最终情绪值")]
        public int finalEmotion;

        [Tooltip("使用的卡牌ID列表")]
        public List<int> usedCards = new List<int>();

        [Tooltip("收集的关键道具（如 rooftop_key）")]
        public List<string> collectedItems = new List<string>();

        [Tooltip("通关时间（秒）")]
        public float completionTime;

        public void AddBranchChoice(string choiceId)
        {
            if (!string.IsNullOrEmpty(choiceId) && !branchChoices.Contains(choiceId))
                branchChoices.Add(choiceId);
        }

        public void AddUsedCard(int cardId)
        {
            if (!usedCards.Contains(cardId))
                usedCards.Add(cardId);
        }

        public void AddCollectedItem(string itemId)
        {
            if (!string.IsNullOrEmpty(itemId) && !collectedItems.Contains(itemId))
                collectedItems.Add(itemId);
        }
    }

    /// <summary>
    /// 结局分支条件 — Inspector 可配置
    /// </summary>
    [System.Serializable]
    public class EndingBranchCondition
    {
        [Tooltip("触发的结局ID（6001~6005）")]
        public int endingId = 6001;

        [Tooltip("备注说明")]
        public string comment = "";

        [Tooltip("必须完成的关卡ID（0=不检查）")]
        public int requiredLevel = 0;

        [Tooltip("必须选择的分支（如 eat / not_eat / confused / internet / rooftop / friend）")]
        public string requiredBranchChoice = "";

        [Tooltip("必须排除的分支（选择了此分支则不能触发）")]
        public string excludedBranchChoice = "";

        [Tooltip("最小生命值（包含）")]
        public int minLives = 0;

        [Tooltip("最大生命值（包含）")]
        public int maxLives = 999;

        [Tooltip("最小情绪值（包含）")]
        public int minEmotion = 0;

        [Tooltip("最大情绪值（包含）")]
        public int maxEmotion = 999;

        [Tooltip("必须使用的卡牌ID列表（为空则不检查）")]
        public List<int> requiredUsedCards = new List<int>();

        [Tooltip("禁止使用的卡牌ID列表（使用了则排除）")]
        public List<int> excludedUsedCards = new List<int>();

        [Tooltip("必须收集的关键道具（如 rooftop_key）")]
        public List<string> requiredItems = new List<string>();
    }
}
