using System.Collections.Generic;
using System.Linq;
using Game.Config;
using Game.Data;
using UnityEngine;

namespace Game.Task
{
    public class TaskManager : SingletonAutoMono<TaskManager>
    {
        private List<TaskGoal> _activeGoals = new List<TaskGoal>();
        private LevelConfigSO _levelConfig;
        [SerializeField] private bool isInitialized = false;
        private bool _hasTriggeredLevelComplete = false;

        private void Start()
        {
            EventCenter.Instance.AddEventListener<int>(E_EventType.CardReadComplete, OnCardReadComplete);
        }

        public void Initialize(LevelConfigSO config)
        {
            _levelConfig = config;
            _activeGoals.Clear();
            _hasTriggeredLevelComplete = false;
            isInitialized = true;

            if (config.taskGoals != null && config.taskGoals.Count > 0)
            {
                foreach (var goal in config.taskGoals)
                {
                    if (goal == null) continue;

                    // 根据卡牌类型自动推导 targetCount
                    int targetCount = goal.targetCount;
                    string taskName = goal.taskName;
                    int taskId = goal.taskId;

                    var cardData = Card.CardManager.Instance.GetCardDataById(goal.targetCardId);
                    if (cardData != null)
                    {
                        if (targetCount <= 0)
                        {
                            targetCount = cardData.cardType == CardType.Stackable
                                ? cardData.initialStack
                                : 1;
                        }
                        if (string.IsNullOrEmpty(taskName))
                            taskName = cardData.cardName;
                        if (taskId <= 0)
                            taskId = goal.targetCardId;
                    }
                    else
                    {
                        Debug.LogWarning($"[TaskManager] 未找到卡牌数据 cardId={goal.targetCardId}，使用配置原始值 targetCount={targetCount}");
                    }

                    var taskGoal = new TaskGoal(goal.targetCardId, targetCount)
                    {
                        taskId = taskId,
                        taskName = taskName,
                        IsHidden = goal.IsHidden,
                        state = goal.IsHidden ? TaskState.Pending : TaskState.InProgress
                    };
                    _activeGoals.Add(taskGoal);
                    Debug.Log($"[TaskManager] 任务: id={taskGoal.taskId} name={taskGoal.taskName} cardId={taskGoal.targetCardId} count={taskGoal.currentCount}/{taskGoal.targetCount} hidden={goal.IsHidden}");
                }
                Debug.Log($"[TaskManager] 初始化完成，共 {_activeGoals.Count} 个任务目标");
            }
            else
            {
                Debug.LogWarning("[TaskManager] 关卡配置中没有任务目标 (taskGoals 为空)");
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

        /// <summary>
        /// 揭示隐藏任务（从Pending转为InProgress），在卡牌使用后调用
        /// </summary>
        public void RevealHiddenTask(int cardId)
        {
            foreach (var goal in _activeGoals)
            {
                if (goal.targetCardId == cardId && goal.IsHidden && goal.state == TaskState.Pending)
                {
                    goal.state = TaskState.InProgress;
                    EventCenter.Instance.EventTrigger(E_EventType.TaskProgressChanged, cardId);
                    Debug.Log($"[TaskManager] 隐藏任务已揭示: id={goal.taskId} name={goal.taskName}");
                    return;
                }
            }
        }

        private void OnCardReadComplete(int cardId)
        {
            if (!isInitialized)
            {
                Debug.LogWarning($"[TaskManager] 收到卡牌完成事件但未初始化，cardId={cardId}");
                return;
            }
            HandleCardCompleted(cardId);
        }

        public void CheckLevelComplete()
        {
            if (_hasTriggeredLevelComplete) return;
            if (_activeGoals.Count == 0) return;

            foreach (var goal in _activeGoals)
            {
                // 隐藏未揭示的任务不参与关卡完成判定
                if (goal.state == TaskState.Pending)
                    continue;
                if (goal.state != TaskState.Completed)
                    return;
            }
            _hasTriggeredLevelComplete = true;
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
            int total = _activeGoals.Count(g => g.state != TaskState.Pending);
            return (completed, total);
        }
    }
}
