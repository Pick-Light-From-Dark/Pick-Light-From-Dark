using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// 跨关卡全局存档系统 — 两路存储方案
    /// 第一路：关卡进度（时间线定位）
    /// 第二路：结局相关累积数据（分支/卡牌/道具/状态）
    /// </summary>    public class CrossLevelSaveSystem : MonoBehaviour
    {
        public static CrossLevelSaveSystem Instance { get; private set; }

        [Header("当前存档")]
        public CrossLevelSaveData currentSave;

        [Header("调试")]
        public bool logOperations = true;

        const string SAVE_KEY = "CrossLevelSave_v1";

        void Awake()
        {
            Instance = this;
            LoadFromDisk();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ========== 第一路：关卡进度操作 ==========

        /// <summary>保存当前剧情进度</summary>
        public void SaveStoryProgress(int levelId, string storyFileName, int lineIndex = 0)
        {
            EnsureData();
            currentSave.checkpoint = new LevelCheckpoint
            {
                currentLevelId = levelId,
                storyFileName = storyFileName,
                storyLineIndex = lineIndex,
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                isInGameplay = false
            };
            SaveToDisk();
            Log($"[Save] 剧情进度存档: Level={levelId}, Story={storyFileName}, Line={lineIndex}");
        }

        /// <summary>保存当前游玩进度</summary>
        public void SaveGameplayProgress(int levelId, int lives, int emotion)
        {
            EnsureData();
            currentSave.checkpoint = new LevelCheckpoint
            {
                currentLevelId = levelId,
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                isInGameplay = true
            };
            currentSave.endingData.lastLives = lives;
            currentSave.endingData.lastEmotion = emotion;
            SaveToDisk();
            Log($"[Save] 游玩进度存档: Level={levelId}, Lives={lives}, Emotion={emotion}");
        }

        /// <summary>读取关卡进度</summary>
        public LevelCheckpoint LoadCheckpoint()
        {
            EnsureData();
            return currentSave.checkpoint;
        }

        // ========== 第二路：结局数据操作 ==========

        /// <summary>记录分支选择</summary>
        public void RecordBranchChoice(string choiceId)
        {
            EnsureData();
            if (!currentSave.endingData.branchChoices.Contains(choiceId))
            {
                currentSave.endingData.branchChoices.Add(choiceId);
                SaveToDisk();
                Log($"[Save] 记录分支: {choiceId}");
            }
        }

        /// <summary>记录卡牌使用</summary>
        public void RecordCardUsed(int cardId)
        {
            EnsureData();
            if (!currentSave.endingData.cardsUsed.Contains(cardId))
            {
                currentSave.endingData.cardsUsed.Add(cardId);
                SaveToDisk();
                Log($"[Save] 记录卡牌: {cardId}");
            }
        }

        /// <summary>记录道具收集</summary>
        public void RecordItemCollected(string itemId)
        {
            EnsureData();
            if (!currentSave.endingData.itemsCollected.Contains(itemId))
            {
                currentSave.endingData.itemsCollected.Add(itemId);
                SaveToDisk();
                Log($"[Save] 记录道具: {itemId}");
            }
        }

        /// <summary>更新最终状态</summary>
        public void UpdateFinalState(int lives, int emotion)
        {
            EnsureData();
            currentSave.endingData.lastLives = lives;
            currentSave.endingData.lastEmotion = emotion;
            SaveToDisk();
        }

        /// <summary>读取结局累积数据</summary>
        public EndingAccumulatedData LoadEndingData()
        {
            EnsureData();
            return currentSave.endingData;
        }

        // ========== 整档操作 ==========

        /// <summary>清除所有跨关卡存档</summary>
        public void ClearAll()
        {
            currentSave = new CrossLevelSaveData();
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Log("[Save] 已清除所有跨关卡存档");
        }

        /// <summary>是否有存档</summary>
        public bool HasSave()
        {
            return currentSave != null && currentSave.checkpoint != null && currentSave.checkpoint.currentLevelId > 0;
        }

        /// <summary>获取存档摘要（用于 UI 显示）</summary>
        public string GetSaveSummary()
        {
            if (!HasSave()) return "无存档";
            var cp = currentSave.checkpoint;
            var time = DateTimeOffset.FromUnixTimeMilliseconds(cp.saveTime).LocalDateTime;
            string mode = cp.isInGameplay ? "游玩中" : "剧情中";
            return $"Lv.{cp.currentLevelId} {mode} | {cp.storyFileName} | {time:MM-dd HH:mm}";
        }

        // ========== 内部 ==========

        void EnsureData()
        {
            if (currentSave == null)
                currentSave = new CrossLevelSaveData();
            if (currentSave.checkpoint == null)
                currentSave.checkpoint = new LevelCheckpoint();
            if (currentSave.endingData == null)
                currentSave.endingData = new EndingAccumulatedData();
        }

        void SaveToDisk()
        {
            EnsureData();
            string json = JsonUtility.ToJson(currentSave, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        void LoadFromDisk()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                currentSave = JsonUtility.FromJson<CrossLevelSaveData>(json);
                if (currentSave == null) currentSave = new CrossLevelSaveData();
            }
            else
            {
                currentSave = new CrossLevelSaveData();
            }
        }

        void Log(string msg)
        {
            if (logOperations)
                Debug.Log(msg);
        }
    }

    // ========== 数据结构 ==========

    [Serializable]
    public class CrossLevelSaveData
    {
        public LevelCheckpoint checkpoint;
        public EndingAccumulatedData endingData;

        public CrossLevelSaveData()
        {
            checkpoint = new LevelCheckpoint();
            endingData = new EndingAccumulatedData();
        }
    }

    /// <summary>第一路：关卡进度检查点</summary>
    [Serializable]
    public class LevelCheckpoint
    {
        public int currentLevelId;        // 当前关卡ID
        public string storyFileName;      // 当前剧情文件名
        public int storyLineIndex;        // 剧情行索引
        public long saveTime;             // 存档时间戳
        public bool isInGameplay;         // true=游玩中, false=剧情中
    }

    /// <summary>第二路：结局相关累积数据</summary>
    [Serializable]
    public class EndingAccumulatedData
    {
        public List<string> branchChoices;
        public List<int> cardsUsed;
        public List<string> itemsCollected;
        public int lastLives;
        public int lastEmotion;

        public EndingAccumulatedData()
        {
            branchChoices = new List<string>();
            cardsUsed = new List<int>();
            itemsCollected = new List<string>();
            lastLives = 3;
            lastEmotion = 50;
        }
    }
}
