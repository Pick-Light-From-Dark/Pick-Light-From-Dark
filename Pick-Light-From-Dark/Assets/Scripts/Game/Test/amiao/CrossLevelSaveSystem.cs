using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// 跨关卡全局存档系统 — 精简版
    /// 只保留结局判定与读档定位必需数据
    /// 6001(太阳照常升起)由剧情选项直接触发，与本存档无关
    /// </summary>
    public class CrossLevelSaveSystem : MonoBehaviour
    {
        public static CrossLevelSaveSystem Instance { get; private set; }

        [Header("当前存档")]
        public CrossLevelSaveData currentSave;

        [Header("调试")]
        public bool logOperations = true;

        const string SAVE_KEY = "CrossLevelSave_v3";

        void Awake()
        {
            Instance = this;
            LoadFromDisk();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ========== 关卡进度操作 ==========

        /// <summary>保存当前剧情进度</summary>
        public void SaveStoryProgress(int levelId, string storyFileName)
        {
            EnsureData();
            currentSave.checkpoint = new LevelCheckpoint
            {
                currentLevelId = levelId,
                storyFileName = storyFileName,
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            SaveToDisk();
            Log($"[Save] 剧情进度存档: Level={levelId}, Story={storyFileName}");
        }

        /// <summary>保存当前游玩进度</summary>
        public void SaveGameplayProgress(int levelId)
        {
            EnsureData();
            currentSave.checkpoint = new LevelCheckpoint
            {
                currentLevelId = levelId,
                storyFileName = "",
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            SaveToDisk();
            Log($"[Save] 游玩进度存档: Level={levelId}");
        }

        /// <summary>读取关卡进度</summary>
        public LevelCheckpoint LoadCheckpoint()
        {
            EnsureData();
            return currentSave.checkpoint;
        }

        /// <summary>自动保存当前关卡进度（从场景名推断，供 UI 存档按钮兜底用）</summary>
        public void SaveCurrentProgress()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Level"))
            {
                string levelStr = sceneName.Substring(5);
                if (int.TryParse(levelStr, out int levelId))
                {
                    SaveGameplayProgress(levelId);
                }
            }
        }

        // ========== 卡牌记录（全局快速查询） ==========

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

        /// <summary>是否使用过指定卡牌</summary>
        public bool HasUsedCard(int cardId)
        {
            EnsureData();
            return currentSave.endingData.cardsUsed.Contains(cardId);
        }

        // ========== 关卡结果记录（结局判定用） ==========

        /// <summary>记录关卡结果（供 LevelFlowCoordinator 等游玩部分调用）</summary>
        public void RecordLevelResult(int levelId, int finalLives, bool card2017 = false, bool card2026 = false)
        {
            EnsureData();
            var result = currentSave.levelResults.Find(r => r.levelId == levelId);
            if (result == null)
            {
                result = new LevelResult { levelId = levelId };
                currentSave.levelResults.Add(result);
            }
            result.finalLives = finalLives;
            result.usedCard2017 = card2017;
            result.usedCard2026 = card2026;
            SaveToDisk();
            Log($"[Save] 记录关卡结果: Lv.{levelId}, Lives={finalLives}, 2017={card2017}, 2026={card2026}");
        }

        /// <summary>获取指定关卡结果</summary>
        public LevelResult GetLevelResult(int levelId)
        {
            EnsureData();
            return currentSave.levelResults.Find(r => r.levelId == levelId);
        }

        // ========== 结局判定接口（供游玩部分调用） ==========

        /// <summary>
        /// 判定第五关结局（6002/6004/6005）
        /// 6001 由第一关剧情选项直接触发，不经过本接口
        /// </summary>
        /// <param name="rooftopChoice">0=未选择, 1=独自前往(6004), 2=邀请宋明月(6005)</param>
        public int EvaluateEnding(int rooftopChoice = 0)
        {
            var r1 = GetLevelResult(1);
            var r2 = GetLevelResult(2);
            var r3 = GetLevelResult(3);
            var r5 = GetLevelResult(5);

            bool HasResult(LevelResult r) => r != null;

            // P0: 6002 莫比乌斯环 — 1/2/3/5关 finalLives 全=1
            if (HasResult(r1) && HasResult(r2) && HasResult(r3) && HasResult(r5) &&
                r1.finalLives == 1 && r2.finalLives == 1 && r3.finalLives == 1 && r5.finalLives == 1)
            {
                Log("[Ending] 判定结果: 6002 莫比乌斯环");
                return 6002;
            }

            // P1: 基础条件 — 至少一关>1
            bool anyGreaterThanOne = (r1?.finalLives > 1) || (r2?.finalLives > 1) || (r3?.finalLives > 1) || (r5?.finalLives > 1);
            if (!anyGreaterThanOne) return 0;

            bool bothCards = (r2?.usedCard2017 == true) && (r5?.usedCard2026 == true);

            if (!bothCards)
            {
                Log("[Ending] 判定结果: 6004 星垂之夜（未全卡）");
                return 6004;
            }

            // 两卡都用，根据木门选项
            if (rooftopChoice == 1)
            {
                Log("[Ending] 判定结果: 6004 星垂之夜（独自）");
                return 6004;
            }
            if (rooftopChoice == 2)
            {
                Log("[Ending] 判定结果: 6005 北极星");
                return 6005;
            }

            Log("[Ending] 条件满足但未选择木门选项");
            return 0; // 需要弹出选项
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

        /// <summary>是否在游玩中（storyFileName 为空表示游玩中）</summary>
        public bool IsInGameplay()
        {
            EnsureData();
            return string.IsNullOrEmpty(currentSave.checkpoint.storyFileName);
        }

        /// <summary>获取存档摘要（用于 UI 显示）</summary>
        public string GetSaveSummary()
        {
            if (!HasSave()) return "无存档";
            var cp = currentSave.checkpoint;
            var time = DateTimeOffset.FromUnixTimeMilliseconds(cp.saveTime).LocalDateTime;
            return $"Lv.{cp.currentLevelId} | {cp.storyFileName} | {time:MM-dd HH:mm}";
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
            if (currentSave.levelResults == null)
                currentSave.levelResults = new List<LevelResult>();
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
        public List<LevelResult> levelResults;

        public CrossLevelSaveData()
        {
            checkpoint = new LevelCheckpoint();
            endingData = new EndingAccumulatedData();
            levelResults = new List<LevelResult>();
        }
    }

    /// <summary>关卡进度检查点</summary>
    [Serializable]
    public class LevelCheckpoint
    {
        public int currentLevelId;        // 当前关卡
        public string storyFileName;      // 当前剧情文件名（空字符串=游玩中）
        public long saveTime;             // 存档时间戳
    }

    /// <summary>结局相关累积数据（精简：仅保留全局卡牌记录）</summary>
    [Serializable]
    public class EndingAccumulatedData
    {
        public List<int> cardsUsed;

        public EndingAccumulatedData()
        {
            cardsUsed = new List<int>();
        }
    }

    /// <summary>单关卡结果（用于跨关卡结局判定）</summary>
    [Serializable]
    public class LevelResult
    {
        public int levelId;
        public int finalLives;
        public bool usedCard2017;
        public bool usedCard2026;
    }
}
