using UnityEngine;
using Game.Data;
using Game.Config;
using Game.Emotion;

namespace Game.Card
{
    /// <summary>
    /// 卡牌读条系统
    /// 管理卡牌的拖拽、读条、打断逻辑
    /// </summary>
    public class CardReadingSystem : MonoBehaviour
    {
        [Header("当前读条状态")]
        [SerializeField] private bool isReading;
        [SerializeField] private CardInstance currentCard;
        [SerializeField] private float readTime;
        [SerializeField] private int currentSegmentIndex;

        private LevelConfigSO levelConfig;

        /// <summary>
        /// 初始化卡牌读条系统
        /// </summary>
        public void Initialize(LevelConfigSO config)
        {
            levelConfig = config;
            isReading = false;
            Debug.Log("[CardReadingSystem] 初始化完成");
        }

        /// <summary>
        /// 开始读条
        /// </summary>
        public void StartReading(CardInstance card)
        {
            if (isReading)
            {
                Debug.LogWarning("[CardReadingSystem] 已有卡牌在读条中");
                return;
            }

            currentCard = card;
            readTime = 0f;
            currentSegmentIndex = 0;
            isReading = true;

            Debug.Log($"[CardReadingSystem] 开始读条: {card.data.cardName}");

            // 触发读条开始事件
            EventCenter.Instance.EventTrigger(E_EventType.CardReadStart, card);
        }

        void Update()
        {
            if (!isReading || currentCard == null)
                return;

            // 更新读条时间
            readTime += Time.deltaTime;

            // 检查是否切换到下一个片段
            Segment currentSegment = GetCurrentSegment();
            if (currentSegment != null)
            {
                float segmentStartTime = GetSegmentStartTime(currentSegmentIndex);
                if (readTime >= segmentStartTime + currentSegment.duration)
                {
                    // 进入下一个片段
                    currentSegmentIndex++;
                    Debug.Log($"[CardReadingSystem] 进入片段 {currentSegmentIndex}");

                    // 检查是否完成
                    if (currentSegmentIndex >= currentCard.data.segments.Count)
                    {
                        CompleteReading();
                    }
                }
            }
        }

        /// <summary>
        /// 打断读条
        /// </summary>
        public bool InterruptReading()
        {
            if (!isReading)
                return false;

            // 检查是否可以打断
            if (!CanInterrupt())
            {
                Debug.LogWarning("[CardReadingSystem] 当前片段不可打断");
                return false;
            }

            Debug.Log($"[CardReadingSystem] 读条被打断: {currentCard.data.cardName}");

            // 增加慌乱值
            EmotionSystem.Instance.ChangePanic(currentCard.data.interruptPanicAdd);

            // 触发打断事件
            EventCenter.Instance.EventTrigger(E_EventType.CardReadInterrupt, currentCard);

            // 清除读条
            ClearReading();

            return true;
        }

        /// <summary>
        /// 完成读条
        /// </summary>
        void CompleteReading()
        {
            Debug.Log($"[CardReadingSystem] 读条完成: {currentCard.data.cardName}");

            // 应用卡牌效果
            ApplyCardEffect(currentCard);

            // 标记卡牌已使用
            currentCard.isUsed = true;
            currentCard.isUsedSuccess = true;

            // 触发完成事件
            EventCenter.Instance.EventTrigger(E_EventType.CardReadComplete, currentCard.data.id);

            // 清除读条
            ClearReading();
        }

        /// <summary>
        /// 应用卡牌效果
        /// </summary>
        void ApplyCardEffect(CardInstance card)
        {
            // 修改情绪值
            if (card.data.panicDelta != 0)
            {
                EmotionSystem.Instance.ChangePanic(card.data.panicDelta);
            }
            if (card.data.exciteDelta != 0)
            {
                EmotionSystem.Instance.ChangeExcite(card.data.exciteDelta);
            }

            Debug.Log($"[CardReadingSystem] 应用效果: 慌乱{card.data.panicDelta} 兴奋{card.data.exciteDelta}");
        }

        /// <summary>
        /// 清除读条
        /// </summary>
        void ClearReading()
        {
            currentCard = null;
            readTime = 0f;
            currentSegmentIndex = 0;
            isReading = false;
        }

        /// <summary>
        /// 获取当前片段
        /// </summary>
        public Segment GetCurrentSegment()
        {
            if (!isReading || currentCard == null)
                return null;

            if (currentSegmentIndex < currentCard.data.segments.Count)
                return currentCard.data.segments[currentSegmentIndex];

            return null;
        }

        /// <summary>
        /// 是否可以打断
        /// </summary>
        public bool CanInterrupt()
        {
            Segment seg = GetCurrentSegment();
            return seg != null && seg.isInterruptible;
        }

        /// <summary>
        /// 是否正在读条
        /// </summary>
        public bool IsReading()
        {
            return isReading;
        }

        /// <summary>
        /// 获取当前卡牌
        /// </summary>
        public CardInstance GetCurrentCard()
        {
            return currentCard;
        }

        /// <summary>
        /// 获取读条进度 (0-1)
        /// </summary>
        public float GetProgress()
        {
            if (!isReading || currentCard == null)
                return 0f;

            float totalDuration = currentCard.data.CalculateTotalDuration();
            if (totalDuration <= 0)
                return 0f;

            return Mathf.Clamp01(readTime / totalDuration);
        }

        /// <summary>
        /// 获取片段开始时间
        /// </summary>
        float GetSegmentStartTime(int segmentIndex)
        {
            if (!isReading || currentCard == null)
                return 0f;

            float time = 0f;
            for (int i = 0; i < segmentIndex && i < currentCard.data.segments.Count; i++)
            {
                time += currentCard.data.segments[i].duration;
            }
            return time;
        }
    }
}
