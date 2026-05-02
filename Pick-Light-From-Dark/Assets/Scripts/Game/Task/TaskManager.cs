using System.Collections.Generic;
using System.Linq;
using Game.Config;
using Game.Data;
using UnityEngine;

namespace Game.Task
{
    /// <summary>
    /// 任务管理器 — 管理关卡任务进度与通关判定
    /// </summary>
    public class TaskManager : SingletonAutoMono<TaskManager>
    {
        private List<TaskGoal> _activeGoals = new List<TaskGoal>();
        private LevelConfigSO _levelConfig;
        [SerializeField] private bool isInitialized = false;

        private void Start()
        {
            EventCenter.Instance.AddEventListener<int>(E_EventType.CardReadComplete, OnCardReadComplete);
        }

        public void Initialize(LevelConfigSO config)
        {
            _levelConfig = config;
            _activeGoals.Clear();
            isInitialized = true;

            if (config.taskGoals != null)
            {
                foreach (var goal in config.taskGoals)
                {
                    var taskGoal = new TaskGoal(goal.targetCardId, goal.targetCount)
                    {
                        state = TaskState.InProgress
                    };
                    _activeGoals.Add(taskGoal);
                }
                Debug.Log($"[TaskManager] 初始化完成，共 {_activeGoals.Count} 个任务目标");
            }
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveEventListener<int>(E_EventType.CardReadComplete, OnCardReadComplete);
        }

        public void HandleCardCompleted(int cardId)
        {
            foreach (var goal in _activeGoals)
            {
                if (goal.targetCardId == cardId && goal.state == TaskState.InProgress)
                {
                    goal.currentCount++;
                    var wasCompleted = goal.CheckCompleted();
                    if (wasCompleted)
                    {
                        goal.state = TaskState.Completed;
                        EventCenter.Instance.EventTrigger(E_EventType.TaskGoalCompleted, goal.targetCardId);
                    }
                    EventCenter.Instance.EventTrigger(E_EventType.TaskProgressChanged, cardId);
                    break;
                }
            }
            CheckLevelComplete();
        }

        private void OnCardReadComplete(int cardId)
        {
            // 防止在初始化前处理事件
            if (!isInitialized)
            {
                Debug.LogWarning($"[TaskManager] 收到卡牌完成事件但未初始化，cardId={cardId}");
                return;
            }
            HandleCardCompleted(cardId);
        }

        public void CheckLevelComplete()
        {
            foreach (var goal in _activeGoals)
            {
                if (goal.state != TaskState.Completed)
                    return;
            }
            EventCenter.Instance.EventTrigger(E_EventType.LevelComplete);
        }

        public IReadOnlyList<TaskGoal> GetActiveGoals()
        {
            return _activeGoals.AsReadOnly();
        }

        public TaskGoal GetGoalByCardId(int cardId)
        {
            return _activeGoals.Find(g => g.targetCardId == cardId && g.state != TaskState.Completed);
        }

        public (int completed, int total) GetOverallProgress()
        {
            int completed = _activeGoals.Count(g => g.state == TaskState.Completed);
            return (completed, _activeGoals.Count);
        }
    }
}
