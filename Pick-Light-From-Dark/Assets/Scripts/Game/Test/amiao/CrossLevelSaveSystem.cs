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
        public static CrossLevelSaveSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CrossLevelSaveSystem>();
                    if (_instance == null)
                    {
                        var go = new GameObject("CrossLevelSaveSystem");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<CrossLevelSaveSystem>();
                    }
                }
                return _instance;
            }
        }
        private static CrossLevelSaveSystem _instance;

        [Header("当前存档")]
        public CrossLevelSaveData currentSave;

        [Header("调试")]
        public bool logOperations = true;

        const string SAVE_KEY = "CrossLevelSave_v3";

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFromDisk();
            _ = TutorialManager.Instance; // 触发引导系统初始化
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        // ========== 关卡进度操作 ==========

        /// <summary>保存当前剧情进度</summary>
        public void SaveStoryProgress(int levelId, string storyFileName, int lineIndex = 0)
        {
            EnsureData();
            currentSave.checkpoint = new LevelCheckpoint
            {
                currentLevelId = levelId,
                storyFileName = storyFileName,
                storyLineIndex = lineIndex,
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
                saveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                isInGameplay = true
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

        // ========== 卡牌记录（全局+本关） ==========

        private HashSet<int> cardsUsedThisLevel = new HashSet<int>();

        /// <summary>记录卡牌使用（全局持久化 + 本关追踪）</summary>
        public void RecordCardUsed(int cardId)
        {
            EnsureData();
            if (!currentSave.endingData.cardsUsed.Contains(cardId))
            {
                currentSave.endingData.cardsUsed.Add(cardId);
                SaveToDisk();
                Log($"[Save] 记录卡牌: {cardId}");
            }
            cardsUsedThisLevel.Add(cardId);
        }

        /// <summary>是否使用过指定卡牌（全局，跨关卡）</summary>
        public bool HasUsedCard(int cardId)
        {
            EnsureData();
            return currentSave.endingData.cardsUsed.Contains(cardId);
        }

        /// <summary>是否在本关使用过指定卡牌</summary>
        public bool HasUsedCardThisLevel(int cardId)
        {
            return cardsUsedThisLevel.Contains(cardId);
        }

        /// <summary>清除本关卡牌使用记录（关卡开始时调用）</summary>
        public void ClearCardsUsedThisLevel()
        {
            cardsUsedThisLevel.Clear();
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
        /// 判定第五关结局（6002/6003/6004）
        /// 6001 太阳照常升起 由第一关剧情选项直接触发，不经过本接口
        /// </summary>
        public int EvaluateEnding()
        {
            var r1 = GetLevelResult(1);
            var r2 = GetLevelResult(2);
            var r3 = GetLevelResult(3);
            var r5 = GetLevelResult(5);

            bool HasResult(LevelResult r) => r != null;

            // P0: 结局二 6002 莫比乌斯环 — 1/2/3/5关 finalLives 全=1，不论其他条件
            if (HasResult(r1) && HasResult(r2) && HasResult(r3) && HasResult(r5) &&
                r1.finalLives == 1 && r2.finalLives == 1 && r3.finalLives == 1 && r5.finalLives == 1)
            {
                Log("[Ending] 判定: 结局二 莫比乌斯环（四关血量全=1）");
                PreEvaluatedEndingId = 6002;
                return 6002;
            }

            // P1: 至少一关血量 > 1
            bool anyGreaterThanOne = (r1?.finalLives > 1) || (r2?.finalLives > 1) || (r3?.finalLives > 1) || (r5?.finalLives > 1);
            if (!anyGreaterThanOne) { PreEvaluatedEndingId = 0; return 0; }

            bool card2017Used = r2?.usedCard2017 == true;
            bool card2026Used = r5?.usedCard2026 == true;

            // 只用了一张卡 → 结局三 6003 星垂之夜
            if (card2017Used != card2026Used) // XOR: 只用了一张
            {
                Log("[Ending] 判定: 结局三 星垂之夜（仅使用一张关键卡）");
                PreEvaluatedEndingId = 6003;
                return 6003;
            }

            // 两张卡都用 → 结局四 6004 北极星
            if (card2017Used && card2026Used)
            {
                Log("[Ending] 判定: 结局四 北极星（两卡全用）");
                PreEvaluatedEndingId = 6004;
                return 6004;
            }

            // 两卡都没用 → 结局三 6003 星垂之夜
            Log("[Ending] 判定: 结局三 星垂之夜（两卡均未使用，默认触发）");
            PreEvaluatedEndingId = 6003;
            return 6003;
        }

        /// <summary>结局预判结果（0=未判定或无法判定）</summary>
        [NonSerialized] public int PreEvaluatedEndingId;

        // ========== 整档操作 ==========

        /// <summary>清除所有跨关卡存档</summary>
        public void ClearAll()
        {
            currentSave = new CrossLevelSaveData();
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Log("[Save] 已清除所有跨关卡存档");
        }

        /// <summary>重置结局相关状态（新游戏开始时调用），不影响关卡进度存档</summary>
        public void ResetEndingState()
        {
            PreEvaluatedEndingId = 0;
            cardsUsedThisLevel.Clear();
            if (currentSave != null)
            {
                currentSave.levelResults?.Clear();
                currentSave.endingData?.cardsUsed?.Clear();
            }
            Log("[Save] 已重置结局状态（保留存档进度）");
        }

        /// <summary>游戏通关时调用，重置所有游戏进度（不影响设置）</summary>
        public void MarkGameCompleted()
        {
            ClearAll();
            Log("[Save] 游戏通关，已重置全部进度（设置保留）");
        }

        // ========== 引导系统 ==========

        /// <summary>指定关卡引导是否已完成</summary>
        public bool HasCompletedTutorial(int levelId)
        {
            EnsureData();
            bool has = currentSave.completedTutorials.Contains(levelId);
            Log($"[Save] 查询引导状态 Level{levelId}: {(has ? "已完成" : "未完成")}, 已完成的: [{string.Join(",", currentSave.completedTutorials)}]");
            return has;
        }

        /// <summary>标记关卡引导已完成（写入存档永久标记）</summary>
        public void MarkTutorialCompleted(int levelId)
        {
            EnsureData();
            if (!currentSave.completedTutorials.Contains(levelId))
            {
                currentSave.completedTutorials.Add(levelId);
                SaveToDisk();
                Debug.Log($"[Save] ★ 引导已标记完成: Level{levelId}, 已完成的: [{string.Join(",", currentSave.completedTutorials)}], PlayerPrefs已保存");
            }
            else
            {
                Log($"[Save] 引导 Level{levelId} 已标记过，跳过");
            }
        }

        /// <summary>重置指定关卡引导（测试用）</summary>
        public void ResetTutorial(int levelId)
        {
            EnsureData();
            currentSave.completedTutorials.Remove(levelId);
            SaveToDisk();
            Log($"[Save] 引导已重置: Level{levelId}");
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
            if (currentSave.completedTutorials == null)
                currentSave.completedTutorials = new List<int>();
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
        public List<int> completedTutorials;

        public CrossLevelSaveData()
        {
            checkpoint = new LevelCheckpoint();
            endingData = new EndingAccumulatedData();
            levelResults = new List<LevelResult>();
            completedTutorials = new List<int>();
        }
    }

    /// <summary>关卡进度检查点</summary>
    [Serializable]
    public class LevelCheckpoint
    {
        public int currentLevelId;
        public string storyFileName;
        public int storyLineIndex;
        public long saveTime;
        public bool isInGameplay;
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
