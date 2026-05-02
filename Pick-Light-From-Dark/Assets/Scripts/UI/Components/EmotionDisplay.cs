using UnityEngine;
using TMPro;
using Game.Emotion;

namespace Game.UI
{
    /// <summary>
    /// 情绪值显示组件
    /// 以文本加颜文字 颜色与字号呈现 慌乱与兴奋两值
    /// 所有展示参数皆 SerializeField 暴露 便前端在 Inspector 替换
    /// </summary>
    public class EmotionDisplay : MonoBehaviour
    {
        [Header("文本组件 必填")]
        [SerializeField] private TextMeshProUGUI panicValueText;
        [SerializeField] private TextMeshProUGUI exciteValueText;

        [Header("慌乱颜文字 三档 低 中 高")]
        [SerializeField] private string panicEmoticonLow = "(´∀｀)";
        [SerializeField] private string panicEmoticonMid = "(・_・;)";
        [SerializeField] private string panicEmoticonHigh = "(>_<)";

        [Header("兴奋颜文字 三档 低 中 高")]
        [SerializeField] private string exciteEmoticonLow = "(-_-)";
        [SerializeField] private string exciteEmoticonMid = "(^_^)";
        [SerializeField] private string exciteEmoticonHigh = "(≧∀≦)";

        [Header("数值阈值 低档上限 中档上限")]
        [SerializeField] private int lowThreshold = 50;
        [SerializeField] private int highThreshold = 75;

        [Header("颜色 三档")]
        [SerializeField] private Color safeColor = new Color(0.4f, 1f, 0.4f);
        [SerializeField] private Color warningColor = new Color(1f, 0.85f, 0.2f);
        [SerializeField] private Color dangerColor = new Color(1f, 0.3f, 0.3f);

        [Header("字号 三档")]
        [SerializeField] private float safeFontSize = 36f;
        [SerializeField] private float warningFontSize = 42f;
        [SerializeField] private float dangerFontSize = 50f;

        [Header("文本标签")]
        [SerializeField] private string panicLabel = "慌乱";
        [SerializeField] private string exciteLabel = "兴奋";

        [Header("文本格式  {0}=标签 {1}=数值 {2}=颜文字")]
        [SerializeField] private string textFormat = "{0} {1}\n{2}";

        [Header("临界态额外强调")]
        [SerializeField] private bool emphasizeOnCritical = true;
        [SerializeField] private float criticalFontSize = 56f;

        private EmotionSystem emotionSystem;
        private bool isCritical;

        void Start()
        {
            emotionSystem = EmotionSystem.Instance;

            if (EventCenter.Instance != null)
            {
                EventCenter.Instance.AddEventListener<int>(E_EventType.PanicChanged, OnPanicChanged);
                EventCenter.Instance.AddEventListener<int>(E_EventType.ExciteChanged, OnExciteChanged);
                EventCenter.Instance.AddEventListener(E_EventType.EmotionCritical, OnEnterCritical);
                EventCenter.Instance.AddEventListener(E_EventType.EmotionRecovered, OnExitCritical);
            }

            Debug.Log($"[EmotionDisplay] Start panicText={(panicValueText != null)} exciteText={(exciteValueText != null)} emotionSystem={(emotionSystem != null)}");
            UpdateDisplay();
        }

        void OnDestroy()
        {
            if (EventCenter.Instance != null)
            {
                EventCenter.Instance.RemoveEventListener<int>(E_EventType.PanicChanged, OnPanicChanged);
                EventCenter.Instance.RemoveEventListener<int>(E_EventType.ExciteChanged, OnExciteChanged);
                EventCenter.Instance.RemoveEventListener(E_EventType.EmotionCritical, OnEnterCritical);
                EventCenter.Instance.RemoveEventListener(E_EventType.EmotionRecovered, OnExitCritical);
            }
        }

        /// <summary>
        /// 慌乱值变化 携 int 当前值
        /// </summary>
        private void OnPanicChanged(int panic)
        {
            UpdateDisplay();
        }

        /// <summary>
        /// 兴奋值变化 携 int 当前值
        /// </summary>
        private void OnExciteChanged(int excite)
        {
            UpdateDisplay();
        }

        /// <summary>
        /// 进入临界态 整体强调
        /// </summary>
        private void OnEnterCritical()
        {
            isCritical = true;
            UpdateDisplay();
        }

        /// <summary>
        /// 退出临界态 还原常态
        /// </summary>
        private void OnExitCritical()
        {
            isCritical = false;
            UpdateDisplay();
        }

        /// <summary>
        /// 据当前情绪值刷新两文本
        /// </summary>
        private void UpdateDisplay()
        {
            if (emotionSystem == null) return;
            var info = emotionSystem.GetEmotionInfo();

            ApplyToText(panicValueText, info.panicValue, panicLabel,
                panicEmoticonLow, panicEmoticonMid, panicEmoticonHigh);
            ApplyToText(exciteValueText, info.exciteValue, exciteLabel,
                exciteEmoticonLow, exciteEmoticonMid, exciteEmoticonHigh);
        }

        /// <summary>
        /// 据数值挑档 套用颜文字 颜色 字号 与文本
        /// </summary>
        private void ApplyToText(TMP_Text text, int value, string label,
            string emoLow, string emoMid, string emoHigh)
        {
            if (text == null) return;

            int level = GetLevel(value);
            string emoticon;
            Color color;
            float size;

            switch (level)
            {
                case 0:
                    emoticon = emoLow;
                    color = safeColor;
                    size = safeFontSize;
                    break;
                case 1:
                    emoticon = emoMid;
                    color = warningColor;
                    size = warningFontSize;
                    break;
                default:
                    emoticon = emoHigh;
                    color = dangerColor;
                    size = dangerFontSize;
                    break;
            }

            if (isCritical && emphasizeOnCritical)
            {
                color = dangerColor;
                size = criticalFontSize;
            }

            text.text = string.Format(textFormat, label, value, emoticon);
            text.color = color;
            text.fontSize = size;
        }

        /// <summary>
        /// 数值档位 0低 1中 2高
        /// </summary>
        private int GetLevel(int value)
        {
            if (value <= lowThreshold) return 0;
            if (value <= highThreshold) return 1;
            return 2;
        }

        /// <summary>
        /// 设置文本组件引用 测试用
        /// </summary>
        public void SetPanicValueText(TextMeshProUGUI text)
        {
            panicValueText = text;
        }

        public void SetExciteValueText(TextMeshProUGUI text)
        {
            exciteValueText = text;
        }
    }
}
