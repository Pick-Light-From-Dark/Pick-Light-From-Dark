using UnityEngine;
using Game.Config;
using Game.Emotion;

namespace Game.Flow
{
    /// <summary>
    /// 游戏流程控制器
    /// 管理游戏的整体流程：开始、暂停、胜利、失败
    /// </summary>
    public class GameFlowController : SingletonAutoMono<GameFlowController>
    {
        [Header("当前关卡配置")]
        [SerializeField] private LevelConfigSO currentLevelConfig;

        [Header("游戏状态")]
        [SerializeField] private bool isPaused;
        [SerializeField] private bool isGameOver;
        [SerializeField] private float remainingTime;
        [SerializeField] private bool hasStartedFirstFrame; // 是否已经过了第一帧

        private LevelConfigSO levelConfig;
        private EmotionSystem emotionSystem;

        /// <summary>
        /// 初始化游戏流程
        /// </summary>
        public void Initialize(LevelConfigSO config)
        {
            levelConfig = config;
            emotionSystem = EmotionSystem.Instance;

            // 重置时间流速为正常
            Time.timeScale = 1f;

            // 初始化情绪值系统
            emotionSystem.Initialize(levelConfig);

            // 初始化时间
            remainingTime = levelConfig.timeLimit;

            isPaused = false;
            isGameOver = false;
            hasStartedFirstFrame = false;

            Debug.Log($"[GameFlow] 关卡 {levelConfig.levelName} 开始");
            Debug.Log($"[GameFlow] 时间限制: {remainingTime}秒, 生命值: {levelConfig.maxLives}");
            Debug.Log($"[GameFlow] 当前时间流速: {Time.timeScale}x");

            // 触发游戏开始事件
            EventCenter.Instance.EventTrigger(E_EventType.GameStart);
            EventCenter.Instance.EventTrigger(E_EventType.LevelStart, levelConfig.levelId);
        }

        void Update()
        {
            if (isGameOver || isPaused) return;

            // 跳过第一帧以避免场景加载导致的大deltaTime
            if (!hasStartedFirstFrame)
            {
                hasStartedFirstFrame = true;
                Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} 跳过第一帧，避免场景加载时间影响倒计时");
                return;
            }

            // 更新倒计时，使用安全的deltaTime
            float deltaTime = Mathf.Min(Time.deltaTime, 0.1f); // 限制最大deltaTime为0.1秒
            remainingTime -= deltaTime;

            // 调试：输出时间信息
            if (Time.frameCount <= 10 || remainingTime < 1f)
            {
                Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} Frame:{Time.frameCount} deltaTime:{Time.deltaTime:F4} clamped:{deltaTime:F4} remainingTime:{remainingTime:F2} timeScale:{Time.timeScale}");
            }

            // 检查时间耗尽
            if (remainingTime <= 0)
            {
                Debug.LogWarning($"[GameFlow] InstanceID:{GetInstanceID()} 时间耗尽! remainingTime={remainingTime:F2}");
                GameLose("时间耗尽");
            }
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (isGameOver) return;

            isPaused = true;
            Time.timeScale = 0f;
            Debug.Log("[GameFlow] 游戏暂停");
            EventCenter.Instance.EventTrigger(E_EventType.GamePause);
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            if (isGameOver) return;

            isPaused = false;
            Time.timeScale = 1f;
            Debug.Log("[GameFlow] 游戏恢复");
            EventCenter.Instance.EventTrigger(E_EventType.GameResume);
        }

        /// <summary>
        /// 游戏胜利
        /// </summary>
        public void GameWin()
        {
            if (isGameOver) return;

            isGameOver = true;
            Time.timeScale = 1f;

            // 取消所有待执行的Invoke（比如ResumeGame）
            CancelInvoke();

            Debug.Log("[GameFlow] 游戏胜利！");
            EventCenter.Instance.EventTrigger(E_EventType.LevelComplete);
            EventCenter.Instance.EventTrigger(E_EventType.GameWin);
        }

        /// <summary>
        /// 游戏失败
        /// </summary>
        public void GameLose(string reason)
        {
            if (isGameOver) return;

            isGameOver = true;
            Time.timeScale = 1f;

            // 取消所有待执行的Invoke（比如ResumeGame）
            CancelInvoke();

            Debug.Log($"[GameFlow] 游戏失败: {reason}");
            EventCenter.Instance.EventTrigger(E_EventType.GameLose, reason);
        }

        /// <summary>
        /// 玩家被抓
        /// </summary>
        public void OnPlayerCaught()
        {
            if (isGameOver)
            {
                Debug.Log("[GameFlow] 游戏已结束，忽略玩家被抓事件");
                return;
            }

            Debug.Log("[GameFlow] 处理玩家被抓逻辑");

            // 这里可以处理生命值扣除逻辑
            // 如果生命值归零则游戏失败

            // 暂停一下让玩家反应
            PauseGame();

            // 2秒后恢复游戏（或者游戏结束）
            Invoke(nameof(ResumeGame), 2f);
        }

        /// <summary>
        /// 获取剩余时间
        /// </summary>
        public float GetRemainingTime()
        {
            return remainingTime;
        }

        /// <summary>
        /// 获取关卡配置
        /// </summary>
        public LevelConfigSO GetLevelConfig()
        {
            return levelConfig;
        }

        /// <summary>
        /// 是否游戏结束
        /// </summary>
        public bool IsGameOver()
        {
            return isGameOver;
        }

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused()
        {
            return isPaused;
        }
    }
}
