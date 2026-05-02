using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Emotion;
using Game.Config;

namespace Game.UI
{
    /// <summary>
    /// 情绪值显示组件
    /// </summary>
    public class EmotionDisplay : MonoBehaviour
    {
        [Header("慌乱值显示 (Panic Value)")]
        [SerializeField] private TextMeshProUGUI panicValueText;
        [SerializeField] private Image panicBackground;

        [Header("兴奋值显示 (Excite Value)")]
        [SerializeField] private TextMeshProUGUI exciteValueText;
        [SerializeField] private Image exciteBackground;

        [Header("颜色配置")]
        [SerializeField] private Color safeColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;

        private EmotionSystem emotionSystem;
        private LevelConfigSO levelConfig;

        void Start()
        {
            // 获取情绪系统引用
            emotionSystem = EmotionSystem.Instance;

            // 从GameFlowController获取关卡配置
            var gameFlow = Game.Flow.GameFlowController.Instance;
            if (gameFlow != null)
            {
                levelConfig = gameFlow.GetLevelConfig();
            }

            // 注册事件监听
            if (EventCenter.Instance != null)
            {
                EventCenter.Instance.AddEventListener(E_EventType.PanicChanged, OnPanicChanged);
                EventCenter.Instance.AddEventListener(E_EventType.ExciteChanged, OnExciteChanged);
            }

            // 初始显示
            UpdateDisplay();
        }

        void OnDestroy()
        {
            // 移除事件监听
            if (EventCenter.Instance != null)
            {
                EventCenter.Instance.RemoveEventListener(E_EventType.PanicChanged, OnPanicChanged);
                EventCenter.Instance.RemoveEventListener(E_EventType.ExciteChanged, OnExciteChanged);
            }
        }

        /// <summary>
        /// 慌乱值变化事件
        /// </summary>
        private void OnPanicChanged()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// 兴奋值变化事件
        /// </summary>
        private void OnExciteChanged()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (emotionSystem == null) return;

            var emotionInfo = emotionSystem.GetEmotionInfo();

            // 更新慌乱值
            if (panicValueText != null)
            {
                panicValueText.text = $"{emotionInfo.panicValue:F0}";
            }

            // 更新兴奋值
            if (exciteValueText != null)
            {
                exciteValueText.text = $"{emotionInfo.exciteValue:F0}";
            }

            // 更新背景颜色
            UpdateBackgroundColor();
        }

        /// <summary>
        /// 更新背景颜色
        /// </summary>
        private void UpdateBackgroundColor()
        {
            if (emotionSystem == null || levelConfig == null) return;

            var emotionInfo = emotionSystem.GetEmotionInfo();
            float totalEmotion = emotionInfo.panicValue + emotionInfo.exciteValue;
            float criticalValue = levelConfig.criticalValue;

            Color targetColor = safeColor;

            if (totalEmotion > criticalValue)
            {
                targetColor = dangerColor;
            }
            else if (totalEmotion > 50)
            {
                targetColor = warningColor;
            }

            if (panicBackground != null)
            {
                panicBackground.color = targetColor;
            }

            if (exciteBackground != null)
            {
                exciteBackground.color = targetColor;
            }
        }

        /// <summary>
        /// 设置文本组件引用
        /// </summary>
        public void SetPanicValueText(TextMeshProUGUI text)
        {
            panicValueText = text;
        }

        public void SetExciteValueText(TextMeshProUGUI text)
        {
            exciteValueText = text;
        }

        public void SetPanicBackground(Image image)
        {
            panicBackground = image;
        }

        public void SetExciteBackground(Image image)
        {
            exciteBackground = image;
        }
    }
}
