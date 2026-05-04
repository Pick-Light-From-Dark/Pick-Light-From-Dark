using UnityEngine;
using Game.Config;
using Game.Flow;

namespace Game.Testing
{
    /// <summary>
    /// 任务系统调试器 — 按键触发事件，验证 TaskManager/LevelRecordManager/PlayerDataStore
    /// </summary>
    public class TaskSystemDebugger : MonoBehaviour
    {
        [Header("调试配置")]
        [Tooltip("当前选中的卡牌 ID")]
        public int currentCardID = 1001;

        [Header("关卡配置")]
        [Tooltip("关卡配置资源（用于初始化）")]
        public LevelConfigSO levelConfig;

        void Update()
        {
            // I: 初始化游戏（触发 GameFlowController.Initialize）
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (levelConfig != null)
                {
                    GameFlowController.Instance.Initialize(levelConfig);
                    Debug.Log($"[TaskSystemDebugger] 初始化游戏，关卡: {levelConfig.levelName} (ID={levelConfig.levelId})");
                }
                else
                {
                    // 如果没有在 Inspector 中配置，尝试加载默认测试配置
                    var testConfig = Resources.Load<LevelConfigSO>("TestData/TestLevelConfig");
                    if (testConfig != null)
                    {
                        GameFlowController.Instance.Initialize(testConfig);
                        Debug.Log($"[TaskSystemDebugger] 初始化游戏（使用默认测试配置），关卡: {testConfig.levelName} (ID={testConfig.levelId})");
                    }
                    else
                    {
                        Debug.LogError("[TaskSystemDebugger] 未找到关卡配置，请在 Inspector 中配置 levelConfig 或确保 TestData/TestLevelConfig.asset 存在");
                    }
                }
            }

            // Space: 触发 CardReadComplete 事件
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EventCenter.Instance.EventTrigger(E_EventType.CardReadComplete, currentCardID);
                Debug.Log($"[TaskSystemDebugger] 触发 CardReadComplete, cardID={currentCardID}");
            }

            // S: 触发 GameWin 事件，验证 LevelRecordManager 存盘
            if (Input.GetKeyDown(KeyCode.S))
            {
                EventCenter.Instance.EventTrigger(E_EventType.GameWin);
                Debug.Log("[TaskSystemDebugger] 触发 GameWin, 验证 LevelRecordManager 存盘");
            }

            // 1: 切换 currentCardID 为 1001
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentCardID = 1001;
                Debug.Log($"[TaskSystemDebugger] 切换 currentCardID → {currentCardID}");
            }

            // 2: 切换 currentCardID 为 1002
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentCardID = 1002;
                Debug.Log($"[TaskSystemDebugger] 切换 currentCardID → {currentCardID}");
            }
        }
    }
}