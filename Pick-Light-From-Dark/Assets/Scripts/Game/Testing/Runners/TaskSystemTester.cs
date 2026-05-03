using System.Collections.Generic;
using UnityEngine;
using Game.Task;
using Game.Data;
using Game.Config;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 任务系统手动测试器
    /// 测试 Initialize / HandleCardCompleted / 关卡通关判定
    /// </summary>
    public class TaskSystemTester : MonoBehaviour
    {
        [Header("测试关卡配置")]
        public LevelConfigSO testConfig;

        [Header("手动模拟卡牌完成")]
        public int simulateCardId = 1;

        [Header("UI 显示")]
        public bool showOnGUI = true;

        private TaskManager taskManager;
        private int taskCompletedCount;
        private int progressChangedCount;
        private int levelCompleteCount;

        void Start()
        {
            taskManager = TaskManager.Instance;
            EventCenter.Instance.AddEventListener<int>(E_EventType.TaskGoalCompleted, OnTaskCompleted);
            EventCenter.Instance.AddEventListener<int>(E_EventType.TaskProgressChanged, OnProgressChanged);
            EventCenter.Instance.AddEventListener(E_EventType.LevelComplete, OnLevelComplete);
            Debug.Log("[TaskSystemTester] 已就绪");
        }

        void OnDestroy()
        {
            if (EventCenter.Instance == null) return;
            EventCenter.Instance.RemoveEventListener<int>(E_EventType.TaskGoalCompleted, OnTaskCompleted);
            EventCenter.Instance.RemoveEventListener<int>(E_EventType.TaskProgressChanged, OnProgressChanged);
            EventCenter.Instance.RemoveEventListener(E_EventType.LevelComplete, OnLevelComplete);
        }

        private void OnTaskCompleted(int cardId)
        {
            taskCompletedCount++;
            Debug.Log($"[TaskSystemTester] TaskGoalCompleted cardId={cardId} 累计={taskCompletedCount}");
        }

        private void OnProgressChanged(int cardId)
        {
            progressChangedCount++;
            Debug.Log($"[TaskSystemTester] TaskProgressChanged cardId={cardId} 累计={progressChangedCount}");
        }

        private void OnLevelComplete()
        {
            levelCompleteCount++;
            Debug.Log($"[TaskSystemTester] LevelComplete 累计={levelCompleteCount}");
        }

        void OnGUI()
        {
            if (!showOnGUI) return;

            int x = 10, y = 10, w = 380, h = 24;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 320), "TaskSystem 测试器");
            y += 25;

            if (taskManager != null)
            {
                var (completed, total) = taskManager.GetOverallProgress();
                GUI.Label(new Rect(x, y, w, h), $"总体进度: {completed}/{total}"); y += h;
                var goals = taskManager.GetActiveGoals();
                if (goals != null)
                {
                    foreach (var g in goals)
                    {
                        GUI.Label(new Rect(x, y, w, h), $"  目标 cardId={g.targetCardId}  {g.currentCount}/{g.targetCount}  {g.state}"); y += h;
                    }
                }
            }
            y += 4;
            GUI.Label(new Rect(x, y, w, h), $"事件计数: 完成={taskCompletedCount} 进度={progressChangedCount} 通关={levelCompleteCount}"); y += h + 4;

            GUI.Label(new Rect(x, y, 100, h), "模拟 cardId:");
            int.TryParse(GUI.TextField(new Rect(x + 100, y, 80, h), simulateCardId.ToString()), out simulateCardId); y += h + 4;

            if (GUI.Button(new Rect(x, y, w, h), "Initialize 关卡配置")) DoInitialize(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "HandleCardCompleted (直调)")) taskManager.HandleCardCompleted(simulateCardId); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "EventTrigger CardReadComplete (走事件)"))
            {
                EventCenter.Instance.EventTrigger(E_EventType.CardReadComplete, simulateCardId);
            }
            y += h;
            if (GUI.Button(new Rect(x, y, w, h), "CheckLevelComplete")) taskManager.CheckLevelComplete(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "重置事件计数器"))
            {
                taskCompletedCount = progressChangedCount = levelCompleteCount = 0;
            }
        }

        [ContextMenu("Initialize")]
        public void DoInitialize()
        {
            if (testConfig == null)
            {
                Debug.LogError("[TaskSystemTester] testConfig 未指定");
                return;
            }
            taskManager.Initialize(testConfig);
        }
    }
}
