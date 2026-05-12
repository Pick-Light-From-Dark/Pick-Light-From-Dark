using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Backend
{
    /// <summary>
    /// 卡牌使用明细记录（精细粒度）
    /// </summary>
    [Serializable]
    public class CardUseEntry
    {
        public int cardId;
        public float useTime;   // 相对关卡开始的秒数
        public bool success;    // 读条是否成功

        public CardUseEntry() { }

        public CardUseEntry(int cardId, float useTime, bool success)
        {
            this.cardId = cardId;
            this.useTime = useTime;
            this.success = success;
        }
    }

    /// <summary>
    /// 任务目标汇总记录（粗粒度）
    /// </summary>
    [Serializable]
    public class TaskGoalRecord
    {
        public int targetCardId;
        public int currentCount;
        public int targetCount;
        public string state; // Pending / InProgress / Completed

        public TaskGoalRecord() { }

        public TaskGoalRecord(int targetCardId, int currentCount, int targetCount, string state)
        {
            this.targetCardId = targetCardId;
            this.currentCount = currentCount;
            this.targetCount = targetCount;
            this.state = state;
        }
    }

    /// <summary>
    /// 单条关卡记录（对应一条 JSON 记录）
    /// </summary>
    [Serializable]
    public class JsonLevelRecord
    {
        public int levelId;
        public long timestamp;        // UTC 时间戳（毫秒）
        public bool isWin;           // 是否通关
        public float timeUsed;        // 耗时（秒）
        public string endingBranch;   // 结局分支标识（如 eat1 / eat2）
        public int endingId;          // 触发的结局ID（6001~6005）
        public List<CardUseEntry> cardUses;
        public List<TaskGoalRecord> taskGoals;

        public JsonLevelRecord() { }

        public JsonLevelRecord(int levelId)
        {
            this.levelId = levelId;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            this.isWin = false;
            this.timeUsed = 0f;
            this.cardUses = new List<CardUseEntry>();
            this.taskGoals = new List<TaskGoalRecord>();
        }
    }

    // StoryProgressRecord 已迁移至 Fungus Save System。
    // 备份代码位于 Backup/StoryProgressRecord_backup.cs.txt

    /// <summary>
    /// 所有玩家记录的顶层容器（对应整个 JSON 文件）
    /// </summary>
    [Serializable]
    public class PlayerDataFile
    {
        public List<JsonLevelRecord> records;
        // StoryProgressRecord storyProgress 已迁移至 Fungus Save System。备份代码见 Backup/

        public PlayerDataFile()
        {
            records = new List<JsonLevelRecord>();
        }
    }
}