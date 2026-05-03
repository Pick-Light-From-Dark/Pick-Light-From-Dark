using UnityEngine;
using Game.Config;

namespace Game.Emotion
{
    /// <summary>
    /// 情绪值信息
    /// </summary>
    public struct EmotionInfo
    {
        public int panicValue;
        public int exciteValue;
        public int totalValue;
        public int criticalValue;

        public override string ToString()
        {
            return $"慌乱:{panicValue} 兴奋:{exciteValue} 总和:{totalValue} 临界:{criticalValue}";
        }
    }

    /// <summary>
    /// 情绪值系统
    /// 管理玩家的慌乱值和兴奋值
    /// </summary>
    public class EmotionSystem : SingletonAutoMono<EmotionSystem>
    {
        [SerializeField] private int panicValue;
        [SerializeField] private int exciteValue;

        private int minPanic = 30, maxPanic = 100;
        private int minExcite = 30, maxExcite = 100;
        private int criticalValue;
        private bool wasCritical;
        private float accumulatedDecrease; // 累积小数部分避免 RoundToInt 截断
        private bool _isNotifying; // 防止事件回调中递归修改情绪值

        /// <summary>
        /// 初始化情绪值系统
        /// </summary>
        public void Initialize(LevelConfigSO levelConfig)
        {
            if (levelConfig == null)
            {
                Debug.LogError("[EmotionSystem] Initialize 失败：levelConfig 为 null");
                return;
            }

            panicValue = levelConfig.initialPanic;
            exciteValue = levelConfig.initialExcite;
            criticalValue = levelConfig.criticalValue;
            wasCritical = false;
            accumulatedDecrease = 0f;

            Debug.Log($"[EmotionSystem] 初始化 - 慌乱:{panicValue} 兴奋:{exciteValue} 临界:{criticalValue}");
            NotifyEmotionChanged();
        }

        /// <summary>
        /// 修改慌乱值
        /// </summary>
        public void ChangePanic(int delta)
        {
            int oldValue = panicValue;
            panicValue = Mathf.Clamp(panicValue + delta, minPanic, maxPanic);
            Debug.Log($"[EmotionSystem] 慌乱值变化: {oldValue} -> {panicValue} (delta:{delta})");
            NotifyEmotionChanged();
        }

        /// <summary>
        /// 修改兴奋值
        /// </summary>
        public void ChangeExcite(int delta)
        {
            int oldValue = exciteValue;
            exciteValue = Mathf.Clamp(exciteValue + delta, minExcite, maxExcite);
            Debug.Log($"[EmotionSystem] 兴奋值变化: {oldValue} -> {exciteValue} (delta:{delta})");
            NotifyEmotionChanged();
        }

        /// <summary>
        /// 检查是否超过临界值
        /// </summary>
        public bool IsCaughtByCriticalValue()
        {
            bool isCritical = (panicValue + exciteValue) >= criticalValue;
            if (isCritical)
            {
                Debug.Log($"[EmotionSystem] 情绪值超标！总和:{panicValue + exciteValue} >= 临界:{criticalValue}");
            }
            return isCritical;
        }

        /// <summary>
        /// 获取当前情绪值信息
        /// </summary>
        public EmotionInfo GetEmotionInfo()
        {
            return new EmotionInfo
            {
                panicValue = this.panicValue,
                exciteValue = this.exciteValue,
                totalValue = this.panicValue + this.exciteValue,
                criticalValue = this.criticalValue
            };
        }

        /// <summary>
        /// 获取情绪值总和
        /// </summary>
        public int GetTotalEmotion()
        {
            return panicValue + exciteValue;
        }

        private void NotifyEmotionChanged()
        {
            if (_isNotifying) return;

            _isNotifying = true;
            EmotionInfo info = GetEmotionInfo();
            // 触发事件
            EventCenter.Instance.EventTrigger(E_EventType.EmotionChanged, info);
            EventCenter.Instance.EventTrigger(E_EventType.PanicChanged, panicValue);
            EventCenter.Instance.EventTrigger(E_EventType.ExciteChanged, exciteValue);

            // 临界跨界检测
            bool isCritical = (panicValue + exciteValue) >= criticalValue;
            if (isCritical && !wasCritical)
            {
                Debug.Log("[EmotionSystem] 进入临界态 触发 EmotionCritical");
                EventCenter.Instance.EventTrigger(E_EventType.EmotionCritical);
            }
            else if (!isCritical && wasCritical)
            {
                Debug.Log("[EmotionSystem] 退出临界态 触发 EmotionRecovered");
                EventCenter.Instance.EventTrigger(E_EventType.EmotionRecovered);
            }
            wasCritical = isCritical;
            _isNotifying = false;
        }

        /// <summary>
        /// 闭眼时持续降低情绪值（累积小数部分避免 RoundToInt 截断）
        /// </summary>
        public void DecreaseEmotionWhileEyeClose(float deltaTime, float decreaseRate = 5f)
        {
            accumulatedDecrease += decreaseRate * deltaTime;
            int decrease = Mathf.FloorToInt(accumulatedDecrease);
            if (decrease > 0)
            {
                accumulatedDecrease -= decrease;
                if (panicValue > minPanic)
                {
                    ChangePanic(-decrease);
                }
                if (exciteValue > minExcite)
                {
                    ChangeExcite(-decrease);
                }
            }
        }
    }
}
