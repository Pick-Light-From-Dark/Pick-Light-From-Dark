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
        [Header("游戏状态")]
        [SerializeField] private bool isPaused;
        [SerializeField] private bool isGameOver;
        [SerializeField] private float remainingTime;
        [SerializeField] private int currentLives;
        [SerializeField] private bool hasStartedFirstFrame; // 是否已经过了第一帧
        [SerializeField] private bool isInitialized = false; // 是否已经初始化

        private LevelConfigSO levelConfig;
        private EmotionSystem emotionSystem;
        private Task.TaskManager taskManager;
        private static int lastUpdateFrame = -1; // 用于多实例检测：记录上次统计的帧号
        private static int updateCount = 0;      // 当前帧内 Update 被调用的次数

        /// <summary>
        /// 初始化游戏流程
        /// </summary>
        public void Initialize(LevelConfigSO config)
        {
            if (config == null)
            {
                Debug.LogError("[GameFlow] Initialize 失败：levelConfig 为 null");
                return;
            }

            Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} === Initialize 开始 ===");
            Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} 当前状态: isInitialized={isInitialized}, isGameOver={isGameOver}, isPaused={isPaused}, hasStartedFirstFrame={hasStartedFirstFrame}, remainingTime={remainingTime:F2}");

            levelConfig = config;
            emotionSystem = EmotionSystem.Instance;
            taskManager = Task.TaskManager.Instance;

            // 重置时间流速为正常
            Time.timeScale = 1f;

            // 初始化情绪值系统
            emotionSystem.Initialize(levelConfig);

            // 初始化任务系统
            taskManager.Initialize(levelConfig);

            // 初始化玩家状态（按 levelConfig.initialInBed 重置卧床/闭眼，避免单例跨关残留）
            Game.Data.PlayerState.Instance.Initialize(levelConfig);

            // 初始化闭眼系统（让 LevelConfigSO 的 eyeClose 阈值/倍率参数生效，并复位加速状态）
            Game.EyeClose.EyeCloseSystem.Instance.Initialize(levelConfig);

            // 初始化卡牌管理器（重置 handCards / cardHistory / nextInstanceId，并发放初始卡牌）
            Game.Card.CardManager.Instance.Initialize(levelConfig);

            // 初始化时间
            remainingTime = levelConfig.timeLimit;

            // 初始化生命值
            currentLives = levelConfig.maxLives;

            isPaused = false;
            isGameOver = false;
            hasStartedFirstFrame = false;

            // 标记为已初始化
            isInitialized = true;

            // 取消所有待执行的Invoke
            CancelInvoke();

            // 重置多实例检测计数器
            updateCount = 0;

            Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} 关卡 {levelConfig.levelName} 开始");
            Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} 时间限制: {remainingTime}秒, 生命值: {levelConfig.maxLives}");
            Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} 当前时间流速: {Time.timeScale}x");
            Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} 初始化后状态: isInitialized={isInitialized}, isGameOver={isGameOver}, hasStartedFirstFrame={hasStartedFirstFrame}, remainingTime={remainingTime:F2}");

            // 触发游戏开始事件
            EventCenter.Instance.EventTrigger(E_EventType.GameStart);
            EventCenter.Instance.EventTrigger(E_EventType.LevelStart, levelConfig.levelId);
        }

        void Start()
        {
            EventCenter.Instance.AddEventListener(E_EventType.LevelComplete, OnLevelComplete);
        }

        void OnDestroy()
        {
            EventCenter.Instance.RemoveEventListener(E_EventType.LevelComplete, OnLevelComplete);
        }

        private void OnLevelComplete()
        {
            GameWin();
        }

        void Update()
        {
            // 如果还未初始化，不执行任何逻辑
            if (!isInitialized) return;

            if (isGameOver || isPaused) return;

            // 检测是否有多个实例在同时运行（仅在前10帧检测）
            if (Time.frameCount <= 10)
            {
                if (Time.frameCount != lastUpdateFrame)
                {
                    lastUpdateFrame = Time.frameCount;
                    updateCount = 0;
                }
                updateCount++;
                if (updateCount > 1)
                {
                    // 查找所有GameFlowController组件
                    GameFlowController[] allControllers = FindObjectsByType<GameFlowController>(FindObjectsSortMode.None);
                    Debug.LogError($"[GameFlow] InstanceID:{GetInstanceID()} Frame:{Time.frameCount} 检测到Update被多次调用！当前updateCount={updateCount}");
                    Debug.LogError($"[GameFlow] 当前场景中有 {allControllers.Length} 个GameFlowController组件：");
                    for (int i = 0; i < allControllers.Length; i++)
                    {
                        Debug.LogError($"[GameFlow]   [{i}] InstanceID:{allControllers[i].GetInstanceID()} GameObject:{allControllers[i].gameObject.name} Active:{allControllers[i].gameObject.activeInHierarchy}");
                    }
                }
            }

            // 跳过第一帧以避免场景加载导致的大deltaTime
            if (!hasStartedFirstFrame)
            {
                hasStartedFirstFrame = true;
                Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} Frame:{Time.frameCount} 跳过第一帧，避免场景加载时间影响倒计时");
                Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} Frame:{Time.frameCount} remainingTime初始化为: {remainingTime:F2}");
                return;
            }

            // 更新倒计时，使用安全的deltaTime
            float deltaTime = Mathf.Min(Time.deltaTime, 0.1f); // 限制最大deltaTime为0.1秒
            float timeBefore = remainingTime;
            remainingTime -= deltaTime;
            float timeAfter = remainingTime;

            // 调试：输出时间信息
            if (Time.frameCount <= 10 || remainingTime < 1f)
            {
                Debug.Log($"[GameFlow] InstanceID:{GetInstanceID()} Frame:{Time.frameCount} deltaTime:{Time.deltaTime:F4} clamped:{deltaTime:F4} timeBefore:{timeBefore:F2} timeAfter:{timeAfter:F2} timeScale:{Time.timeScale}");
            }

            // 检查时间耗尽
            if (remainingTime <= 0)
            {
                Debug.LogWarning($"[GameFlow] InstanceID:{GetInstanceID()} Frame:{Time.frameCount} 时间耗尽! remainingTime={remainingTime:F2} deltaTime={deltaTime:F4}");
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

            // 取消所有待执行的Invoke和协程（比如ResumeGame的延迟恢复）
            CancelInvoke();
            StopAllCoroutines();

            Debug.Log("[GameFlow] 游戏胜利！");
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

            // 取消所有待执行的Invoke和协程（比如ResumeGame的延迟恢复）
            CancelInvoke();
            StopAllCoroutines();

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

            // 扣除生命值
            currentLives--;
            Debug.Log($"[GameFlow] 玩家被抓，生命值剩余: {currentLives}/{levelConfig.maxLives}");

            // 生命值耗尽 → 游戏失败
            if (currentLives <= 0)
            {
                GameLose("生命值耗尽");
                return;
            }

            // 还有生命值：暂停 + 2秒后恢复（让玩家反应）
            PauseGame();
            StartCoroutine(ResumeAfterDelayRealtime(2f));
        }

        /// <summary>
        /// 延迟恢复游戏（使用真实时间，不受 timeScale=0 影响）
        /// </summary>
        private System.Collections.IEnumerator ResumeAfterDelayRealtime(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (!isGameOver)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// 获取剩余生命值
        /// </summary>
        public int GetCurrentLives()
        {
            return currentLives;
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
