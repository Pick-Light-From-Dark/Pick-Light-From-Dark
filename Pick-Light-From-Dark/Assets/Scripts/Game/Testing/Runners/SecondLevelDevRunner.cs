using UnityEngine;
using Game.Config;
using Game.Flow;
using Game.AI;
using Game.Card;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 第二夜完整场景运行器
    /// 自动聚合所有后端系统，提供一键初始化第二夜（测试案v2）的入口
    /// </summary>
    public class SecondLevelDevRunner : MonoBehaviour
    {
        [Header("关卡配置")]
        [Tooltip("留空则运行时自动创建临时配置")]
        public LevelConfigSO levelConfig;

        [Header("自动初始化")]
        [Tooltip("Play 时自动调用 InitializeSecondLevel")]
        public bool autoInitOnStart = true;

        [Header("依赖组件（自动获取）")]
        [Tooltip("如场景无 DevModeController，将自动创建")]
        public DevModeController devMode;

        [Header("UI")]
        public bool showOnGUI = true;
        public KeyCode initKey = KeyCode.F2;

        private GameFlowController gameFlow;
        private TeacherAI teacherAI;
        private CardReadingSystem cardReadingSystem;

        void Awake()
        {
            EnsureSingletons();
            EnsureSceneComponents();
        }

        void Start()
        {
            if (autoInitOnStart)
            {
                InitializeSecondLevel();
            }
            else
            {
                Debug.Log("[SecondLevelDevRunner] 按 F2 或点 OnGUI 按钮初始化第二夜");
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(initKey))
            {
                InitializeSecondLevel();
            }
        }

        void OnGUI()
        {
            if (!showOnGUI) return;

            int x = Screen.width - 390, y = 10, w = 380, h = 24;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 200), "第二夜运行器 (SecondLevelDevRunner)");
            y += 25;

            GUI.Label(new Rect(x, y, w, h), $"GameFlow: {(gameFlow != null ? "OK" : "MISSING")}"); y += h;
            GUI.Label(new Rect(x, y, w, h), $"TeacherAI: {(teacherAI != null ? teacherAI.GetCurrentState().ToString() : "MISSING")}"); y += h;
            GUI.Label(new Rect(x, y, w, h), $"CardReading: {(cardReadingSystem != null ? "OK" : "MISSING")}"); y += h;
            GUI.Label(new Rect(x, y, w, h), $"DevMode: {(devMode != null ? "OK" : "MISSING")}"); y += h + 4;

            if (GUI.Button(new Rect(x, y, w, h), $"[{initKey}] 初始化第二夜（测试案v2）"))
            {
                InitializeSecondLevel();
            }
            y += h;

            if (gameFlow != null)
            {
                GUI.Label(new Rect(x, y, w, h), $"剩余时间: {gameFlow.GetRemainingTime():F1}s  生命: {gameFlow.GetCurrentLives()}"); y += h;
            }
        }

        /// <summary>
        /// 一键初始化第二夜
        /// 优先从 Resources/Config/LevelConfig_2 加载持久化配置资产，
        /// 找不到时回退到 DevModeController 硬编码默认值。
        /// </summary>
        [ContextMenu("InitializeSecondLevel")]
        public void InitializeSecondLevel()
        {
            // 1. 尝试加载持久化配置资产
            if (levelConfig == null)
            {
                levelConfig = Resources.Load<LevelConfigSO>("Config/LevelConfig_2");
                if (levelConfig != null)
                {
                    Debug.Log("[SecondLevelDevRunner] 从 Resources/Config/LevelConfig_2 加载关卡配置");
                }
            }

            if (levelConfig != null)
            {
                // 使用持久化资产初始化
                gameFlow = GameFlowController.Instance;
                gameFlow?.Initialize(levelConfig);

                if (teacherAI != null)
                {
                    teacherAI.Initialize(levelConfig);
                }
                else
                {
                    Debug.LogWarning("[SecondLevelDevRunner] 场景中未找到 TeacherAI，教师巡逻不会启动");
                }

                if (cardReadingSystem != null)
                {
                    cardReadingSystem.Initialize(levelConfig);
                }
            }
            else
            {
                // 回退：创建临时配置并用 DevModeController 写入默认值
                Debug.Log("[SecondLevelDevRunner] 未找到持久化配置资产，使用硬编码默认值");
                levelConfig = ScriptableObject.CreateInstance<LevelConfigSO>();

                if (devMode == null)
                {
                    devMode = gameObject.GetComponent<DevModeController>();
                    if (devMode == null)
                    {
                        devMode = gameObject.AddComponent<DevModeController>();
                    }
                }
                devMode.levelConfig = levelConfig;
                devMode.InitializeSecondLevel();

                if (teacherAI != null)
                {
                    teacherAI.Initialize(levelConfig);
                }

                if (cardReadingSystem != null)
                {
                    cardReadingSystem.Initialize(levelConfig);
                }
            }

            // 确保 DevMode 面板可以监控当前关卡
            if (devMode == null)
            {
                devMode = gameObject.GetComponent<DevModeController>();
                if (devMode == null)
                {
                    devMode = gameObject.AddComponent<DevModeController>();
                }
            }
            devMode.levelConfig = levelConfig;

            Debug.Log("[SecondLevelDevRunner] 第二夜初始化完成");
        }

        /// <summary>
        /// 确保所有单例系统已就绪
        /// </summary>
        void EnsureSingletons()
        {
            gameFlow = GameFlowController.Instance;
            var emotion = Game.Emotion.EmotionSystem.Instance;
            var playerState = Game.Data.PlayerState.Instance;
            var eyeClose = Game.EyeClose.EyeCloseSystem.Instance;
            var cardMgr = CardManager.Instance;
            var taskMgr = Game.Task.TaskManager.Instance;

            Debug.Log($"[SecondLevelDevRunner] 单例系统就绪: GameFlow={gameFlow != null}, Emotion={emotion != null}, PlayerState={playerState != null}, EyeClose={eyeClose != null}, CardMgr={cardMgr != null}, TaskMgr={taskMgr != null}");
        }

        /// <summary>
        /// 确保场景中存在 TeacherAI 和 CardReadingSystem
        /// </summary>
        void EnsureSceneComponents()
        {
            teacherAI = FindFirstObjectByType<TeacherAI>();
            if (teacherAI == null)
            {
                GameObject teacherGO = new GameObject("TeacherAI");
                teacherAI = teacherGO.AddComponent<TeacherAI>();
                Debug.Log("[SecondLevelDevRunner] 自动创建 TeacherAI");
            }

            cardReadingSystem = FindFirstObjectByType<CardReadingSystem>();
            if (cardReadingSystem == null)
            {
                GameObject cardGO = new GameObject("CardReadingSystem");
                cardReadingSystem = cardGO.AddComponent<CardReadingSystem>();
                Debug.Log("[SecondLevelDevRunner] 自动创建 CardReadingSystem");
            }

            devMode = FindFirstObjectByType<DevModeController>();
            if (devMode == null)
            {
                devMode = gameObject.AddComponent<DevModeController>();
                Debug.Log("[SecondLevelDevRunner] 自动创建 DevModeController");
            }

            var textGame = FindFirstObjectByType<TextGameController>();
            if (textGame == null)
            {
                textGame = gameObject.AddComponent<TextGameController>();
                Debug.Log("[SecondLevelDevRunner] 自动创建 TextGameController");
            }
        }
    }
}
