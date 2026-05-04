using UnityEngine;
using TMPro;
using Game.Data;
using Game.EyeClose;

namespace Game.UI
{
    /// <summary>
    /// 闭眼状态指示组件
    /// 以文本、颜色、字号呈现当前闭眼状态、时长与时间加速提示
    /// 所有展示参数皆 SerializeField 暴露 便前端在 Inspector 替换
    /// </summary>
    public class EyeCloseDisplay : MonoBehaviour
    {
        [Header("文本组件 必填")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI warningText;

        [Header("状态文本")]
        [SerializeField] private string closedLabel = "闭眼中";
        [SerializeField] private string openLabel = "睁眼";

        [Header("颜色")]
        [SerializeField] private Color closedColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color openColor = new Color(0.4f, 1f, 0.4f);
        [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.3f);

        [Header("字号")]
        [SerializeField] private float normalFontSize = 36f;
        [SerializeField] private float warningFontSize = 50f;

        [Header("时间加速提示")]
        [SerializeField] private string accelerationFormat = "时间加速 x{0}";

        private PlayerState playerState;
        private EyeCloseSystem eyeCloseSystem;

        void Start()
        {
            playerState = PlayerState.Instance;
            eyeCloseSystem = EyeCloseSystem.Instance;

            Debug.Log($"[EyeCloseDisplay] Start playerState={(playerState != null)} eyeCloseSystem={(eyeCloseSystem != null)}");
            UpdateDisplay();
        }

        void Update()
        {
            if (playerState == null || eyeCloseSystem == null) return;

            bool isClosed = playerState.IsEyesClosed();
            float duration = eyeCloseSystem.GetEyeCloseDuration();
            bool isAccelerated = eyeCloseSystem.IsTimeAccelerated();

            UpdateDisplay(isClosed, duration, isAccelerated);
        }

        /// <summary>
        /// 据当前状态刷新三行文本
        /// </summary>
        private void UpdateDisplay(bool isClosed, float duration, bool isAccelerated)
        {
            // 状态文本
            if (statusText != null)
            {
                statusText.text = isClosed ? closedLabel : openLabel;
                statusText.color = isClosed ? closedColor : openColor;
                statusText.fontSize = normalFontSize;
            }

            // 计时文本
            if (timerText != null)
            {
                timerText.text = isClosed ? $"{duration:F1}s" : "";
                timerText.color = isClosed ? closedColor : openColor;
                timerText.fontSize = normalFontSize;
            }

            // 加速警告文本
            if (warningText != null)
            {
                if (isAccelerated)
                {
                    float multiplier = eyeCloseSystem.GetAccelerationMultiplier();
                    warningText.text = string.Format(accelerationFormat, multiplier);
                    warningText.color = warningColor;
                    warningText.fontSize = warningFontSize;
                }
                else
                {
                    warningText.text = "";
                }
            }
        }

        /// <summary>
        /// 无参版 供初始化调用
        /// </summary>
        private void UpdateDisplay()
        {
            if (playerState == null || eyeCloseSystem == null) return;
            UpdateDisplay(playerState.IsEyesClosed(), eyeCloseSystem.GetEyeCloseDuration(), eyeCloseSystem.IsTimeAccelerated());
        }
    }
}
