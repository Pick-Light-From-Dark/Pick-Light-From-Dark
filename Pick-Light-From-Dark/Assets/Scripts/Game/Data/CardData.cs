using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// 卡牌堆叠类型
    /// </summary>
    public enum CardType
    {
        NonStackable, // 不可堆叠类
        Stackable     // 可堆叠类
    }

    /// <summary>
    /// 卡牌关联类型
    /// </summary>
    public enum RelatedType
    {
        Persistent,    // 持续关联
        NonPersistent  // 非持续关联
    }

    /// <summary>
    /// 卡牌数据定义
    /// </summary>
    [Serializable]
    public class CardData
    {
        public int id;
        public string cardName;
        public string description;
        public string iconPath;

        [Header("卡牌类型")]
        public CardType cardType = CardType.NonStackable;
        public int initialStack = 1;

        [Header("读条片段")]
        public List<Segment> segments;

        [Header("情绪值变化")]
        public int panicDelta;
        public int exciteDelta;
        public int interruptPanicAdd;

        [Header("任务与关联")]
        public int bindTaskId;
        public RelatedType relatedType = RelatedType.NonPersistent;
        public List<int> relatedCardIds;

        [Header("关卡限制")]
        public List<string> allowedLevelIds; // 填 \"all\" 表示所有关卡可用

        [Header("特殊效果")]
        [TextArea(2, 4)]
        public string specialEffect;

        /// <summary>
        /// 计算总读条时长（各片段累加）
        /// </summary>
        public float CalculateTotalDuration()
        {
            float total = 0f;
            if (segments != null)
            {
                foreach (var seg in segments)
                    total += seg.duration;
            }
            return total;
        }

        /// <summary>
        /// 检查是否允许在指定关卡生成
        /// </summary>
        public bool IsAllowedInLevel(int levelId)
        {
            if (allowedLevelIds == null || allowedLevelIds.Count == 0)
                return true;
            foreach (var id in allowedLevelIds)
            {
                if (id == "all" || id == levelId.ToString())
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 卡牌实例数据
    /// </summary>
    public class CardInstance
    {
        public CardData data;
        public int instanceId;
        public bool isUsed;
        public bool isUsedSuccess;
        public float currentReadTime;
        public int currentSegmentIndex;

        public CardInstance(CardData data, int instanceId)
        {
            this.data = data;
            this.instanceId = instanceId;
            this.isUsed = false;
            this.isUsedSuccess = false;
            this.currentReadTime = 0f;
            this.currentSegmentIndex = 0;
        }

        public Segment GetCurrentSegment()
        {
            if (data.segments != null && currentSegmentIndex < data.segments.Count)
                return data.segments[currentSegmentIndex];
            return null;
        }

        public bool CanInterrupt()
        {
            Segment seg = GetCurrentSegment();
            return seg != null && seg.isInterruptible;
        }
    }
}
