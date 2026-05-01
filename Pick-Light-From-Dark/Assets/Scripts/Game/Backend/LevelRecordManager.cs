using System;
using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Backend
{
    /// <summary>
    /// 关卡记录管理器 — 与 Unity 游戏逻辑对接
    /// 监听事件，收集本关数据，关卡结束时写入 PlayerDataStore
    /// </summary>
    public class LevelRecordManager : SingletonAutoMono<LevelRecordManager>
    {
        private JsonLevelRecord _currentRecord;
        private float _levelStartTime;
        private bool _isRecording;

        private void Start()
        {
            EventCenter.Instance.AddEventListener<int>(E_EventType.CardReadComplete, OnCardReadComplete);
            EventCenter.Instance.AddEventListener(E_EventType.LevelStart, OnLevelStart);
            EventCenter.Instance.AddEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.AddEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

        private void OnDestroy()
        {
            EventCenter.Instance.RemoveEventListener<int>(E_EventType.CardReadComplete, OnCardReadComplete);
            EventCenter.Instance.RemoveEventListener(E_EventType.LevelStart, OnLevelStart);
            EventCenter.Instance.RemoveEventListener(E_EventType.GameWin, OnGameWin);
            EventCenter.Instance.RemoveEventListener<string>(E_EventType.GameLose, OnGameLose);
        }

        /// <summary>
        /// 关卡开始时由外界调用（也可通过 LevelStart 事件自动触发）
        /// </summary>
        public void StartRecording(int levelId)
        {
            _currentRecord = new JsonLevelRecord(levelId);
            _levelStartTime = Time.time;
            _isRecording = true;
            SyncTaskGoals();
            Debug.Log($"[LevelRecordManager] 开始记录关卡 {levelId}");
        }

        /// <summary>
        /// 外部手动结束录制（通常由 GameWin / GameLose 触发）
        /// </summary>
        public void EndRecording(bool isWin)
        {
            if (!_isRecording || _currentRecord == null) return;

            _currentRecord.timeUsed = Time.time - _levelStartTime;
            _currentRecord.isWin = isWin;
            SyncTaskGoals();
            PlayerDataStore.Instance.SaveLevelRecord(_currentRecord);
            _isRecording = false;
            Debug.Log($"[LevelRecordManager] 关卡结束 isWin={isWin} 耗时={_currentRecord.timeUsed:F1}s");
        }

        private void OnLevelStart()
        {
            var config = UnityEngine.Resources.Load<Game.Config.LevelConfigSO>("LevelConfigs/Level_1");
            if (config != null)
                StartRecording(config.levelId);
        }

        private void OnCardReadComplete(int cardId)
        {
            if (!_isRecording || _currentRecord == null) return;

            float useTime = Time.time - _levelStartTime;
            var entry = new CardUseEntry(cardId, useTime, true);
            _currentRecord.cardUses.Add(entry);

            // 同步任务目标状态
            var goal = System.Task.TaskManager.Instance.GetGoalByCardId(cardId);
            if (goal != null)
            {
                SyncSingleGoal(goal);
            }

            Debug.Log($"[LevelRecordManager] 记录卡牌使用 cardId={cardId} useTime={useTime:F1}s");
        }

        private void OnGameWin()
        {
            EndRecording(true);
        }

        private void OnGameLose(string reason)
        {
            EndRecording(false);
        }

        /// <summary>
        /// 从 TaskManager 同步所有任务目标到当前记录
        /// </summary>
        private void SyncTaskGoals()
        {
            if (_currentRecord == null) return;

            _currentRecord.taskGoals.Clear();
            var goals = System.Task.TaskManager.Instance.GetActiveGoals();
            foreach (var goal in goals)
            {
                var record = new TaskGoalRecord(
                    goal.targetCardId,
                    goal.currentCount,
                    goal.targetCount,
                    goal.state.ToString()
                );
                _currentRecord.taskGoals.Add(record);
            }
        }

        /// <summary>
        /// 同步单个任务目标（卡牌使用后增量更新）
        /// </summary>
        private void SyncSingleGoal(TaskGoal goal)
        {
            // 若已有该卡牌的记录则更新，否则新增
            var existing = _currentRecord.taskGoals.Find(g => g.targetCardId == goal.targetCardId);
            if (existing != null)
            {
                existing.currentCount = goal.currentCount;
                existing.state = goal.state.ToString();
            }
            else
            {
                _currentRecord.taskGoals.Add(new TaskGoalRecord(
                    goal.targetCardId,
                    goal.currentCount,
                    goal.targetCount,
                    goal.state.ToString()
                ));
            }
        }
    }
}