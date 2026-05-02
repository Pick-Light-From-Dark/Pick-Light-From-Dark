using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
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
        public List<Segment> segments;
        public int panicDelta;
        public int exciteDelta;
        public int interruptPanicAdd;

        public float CalculateTotalDuration()
        {
            float total = 0f;
            foreach (var seg in segments)
                total += seg.duration;
            return total;
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
            if (currentSegmentIndex < data.segments.Count)
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
