using UnityEngine;
using TMPro;
using Game.Flow;
using Game.Config;

namespace Game.UI
{
    /// <summary>
    /// 倒计时显示组件
    /// </summary>
    public class TimerDisplay : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private TextMeshProUGUI timerText;

        private GameFlowController gameFlow;
        private LevelConfigSO levelConfig;

        void Start()
        {
            // 获取引用
            gameFlow = GameFlowController.Instance;
            if (gameFlow != null && gameFlow.GetLevelConfig() != null)
            {
                levelConfig = gameFlow.GetLevelConfig();
            }
        }

        void Update()
        {
            if (gameFlow == null || levelConfig == null) return;

            // 如果游戏暂停或结束，不更新显示
            if (gameFlow.IsPaused() || gameFlow.IsGameOver()) return;

            // 更新倒计时显示
            float remaining = gameFlow.GetRemainingTime();
            float total = levelConfig.timeLimit;

            if (timerText != null)
            {
                timerText.text = $"{remaining:F0}/{total:F0}";
            }
        }

        /// <summary>
        /// 设置文本组件引用
        /// </summary>
        public void SetTimerText(TextMeshProUGUI text)
        {
            timerText = text;
        }
    }
}
