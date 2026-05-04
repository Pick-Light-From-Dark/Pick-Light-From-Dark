using UnityEngine;
using Game.Config;
using Game.Flow;
using Game.Emotion;
using Game.AI;

namespace Game.Demo
{
    /// <summary>
    /// Demo 关卡控制器 - 展示完整的游戏流程
    ///
    /// 功能演示：
    /// 1. 初始化各系统（EmotionSystem, TaskManager, TeacherAI 等）
    /// 2. 玩家操作（C 键闭眼，Space 使用卡牌）
    /// 3. 教师AI巡逻（手电筒检查）
    /// 4. 任务进度（卡牌使用 → 任务完成）
    /// 5. 情绪值变化（卡牌增加，闭眼减少）
    /// 6. 通关/失败判定
    /// </summary>
    public class DemoController : MonoBehaviour
    {
        [Header("关卡配置")]
        [Tooltip("Demo 关卡配置（使用 TestData/TestLevelConfig）")]
        public LevelConfigSO demoLevelConfig;

        [Header("调试信息")]
        [SerializeField]
        private bool isInitialized = false;

        void Start()
        {
            if (demoLevelConfig == null)
            {
                // 尝试自动加载测试配置
                demoLevelConfig = Resources.Load<LevelConfigSO>("TestData/TestLevelConfig");
            }

            if (demoLevelConfig != null)
            {
                InitializeDemo();
            }
            else
            {
                Debug.LogError("[DemoController] 未找到关卡配置，请在 Inspector 中配置 demoLevelConfig");
            }
        }

        void InitializeDemo()
        {
            Debug.Log("=== Demo 关卡初始化 ===");

            // 初始化游戏流程控制器
            GameFlowController.Instance.Initialize(demoLevelConfig);

            // 初始化教师AI（场景中的 MonoBehaviour，由启动器负责配置）
            TeacherAI teacherAI = FindFirstObjectByType<TeacherAI>();
            if (teacherAI != null)
            {
                teacherAI.Initialize(demoLevelConfig);
            }

            isInitialized = true;

            Debug.Log($"[DemoController] 关卡: {demoLevelConfig.levelName}");
            Debug.Log($"[DemoController] 时间限制: {demoLevelConfig.timeLimit}秒");
            Debug.Log($"[DemoController] 临界情绪值: {demoLevelConfig.criticalValue}");
            Debug.Log($"[DemoController] 任务数量: {demoLevelConfig.taskGoals.Count}");

            ShowDemoInstructions();
        }

        void ShowDemoInstructions()
        {
            Debug.Log("\n=== Demo 操作说明 ===");
            Debug.Log("按 I 键：初始化游戏（如未自动初始化）");
            Debug.Log("按 Space 键：使用当前卡牌（ID 由 TaskSystemDebugger 控制）");
            Debug.Log("按 1/2 键：切换卡牌 ID");
            Debug.Log("按 C 键：切换闭眼状态（闭眼10秒后触发时间加速）");
            Debug.Log("按 S 键：模拟游戏胜利");
            Debug.Log("按 L 键：模拟游戏失败（时间耗尽）");
            Debug.Log("按 R 键：重新加载 Demo 关卡");
            Debug.Log("=========================\n");
        }

        void Update()
        {
            if (!isInitialized) return;

            // 按 R 键重新加载 Demo
            if (Input.GetKeyDown(KeyCode.R))
            {
                ReloadDemo();
            }

            // 按 L 键模拟失败
            if (Input.GetKeyDown(KeyCode.L))
            {
                SimulateGameLose();
            }
        }

        void ReloadDemo()
        {
            Debug.Log("[DemoController] 重新加载 Demo 关卡...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        void SimulateGameLose()
        {
            Debug.Log("[DemoController] 模拟游戏失败（时间耗尽）");
            GameFlowController.Instance.GameLose("时间耗尽");
        }
    }
}
