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

    /*
     * StoryProgressRecord 已迁移至 Fungus Save System。
     * 保留代码作为备份，如需恢复自研 JSON 存档可取消注释。
     *
    /// <summary>
    /// 剧情进度存档记录（支持中途存档/读档）
    /// </summary>
    [Serializable]
    public class StoryProgressRecord
    {
        public int levelId;           // 所属关卡
        public string storyFileName;  // 剧情文件名
        public int lineIndex;         // 当前行号
        public long timestamp;        // 存档时间
        public bool isOpeningDone;    // 开场剧情是否已看完

        public StoryProgressRecord() { }

        public StoryProgressRecord(int levelId, string storyFileName, int lineIndex, bool isOpeningDone)
        {
            this.levelId = levelId;
            this.storyFileName = storyFileName;
            this.lineIndex = lineIndex;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            this.isOpeningDone = isOpeningDone;
        }
    }
    */

    /// <summary>
    /// 所有玩家记录的顶层容器（对应整个 JSON 文件）
    /// </summary>
    [Serializable]
    public class PlayerDataFile
    {
        public List<JsonLevelRecord> records;
        // public StoryProgressRecord storyProgress; // 已迁移至 Fungus Save System

        public PlayerDataFile()
        {
            records = new List<JsonLevelRecord>();
        }
    }
}