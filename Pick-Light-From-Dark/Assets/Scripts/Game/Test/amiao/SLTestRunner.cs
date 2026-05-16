using UnityEngine;
using System;
using System.Collections.Generic;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// SL 场景存档读档测试主控器（精简版）
    /// 测试存档数据结构、结局判定接口与读档定位
    /// 6001 由剧情选项直接触发，不在本存档系统中测试
    /// </summary>
    public class SLTestRunner : MonoBehaviour
    {
        [System.Serializable]
        public class LevelInfo
        {
            public int levelId;
            public string storyFile;
            public string displayName;
        }

        [Header("VN 控制器（为空则自动查找）")]
        public FungusVNController vnController;

        [Header("存档系统（为空则自动创建）")]
        public CrossLevelSaveSystem saveSystem;

        [Header("选关配置")]
        public List<LevelInfo> levels = new List<LevelInfo>
        {
            new LevelInfo { levelId = 1, storyFile = "Dialogue1-1", displayName = "第一关上段" },
            new LevelInfo { levelId = 2, storyFile = "Dialogue2-1", displayName = "第二关上段" },
            new LevelInfo { levelId = 3, storyFile = "Dialogue3-1", displayName = "第三关上段" },
            new LevelInfo { levelId = 4, storyFile = "Dialogue4-1", displayName = "第四关上段" },
            new LevelInfo { levelId = 5, storyFile = "Dialogue5-1", displayName = "第五关上段" }
        };

        [Header("调试")]
        public bool showGUI = true;
        public bool autoStartLevel1 = true;
        public bool injectTestDataOnStart = true;

        int currentLevelIndex = 0;
        string statusLog = "就绪";
        Rect uiRect = new Rect(10, 10, 480, 440);
        Vector2 scrollPos;

        void Start()
        {
            EnsureComponents();

            if (levels == null || levels.Count == 0)
            {
                levels = new List<LevelInfo>
                {
                    new LevelInfo { levelId = 1, storyFile = "Dialogue1-1", displayName = "第一关上段" },
                    new LevelInfo { levelId = 2, storyFile = "Dialogue2-1", displayName = "第二关上段" },
                    new LevelInfo { levelId = 3, storyFile = "Dialogue3-1", displayName = "第三关上段" },
                    new LevelInfo { levelId = 4, storyFile = "Dialogue4-1", displayName = "第四关上段" },
                    new LevelInfo { levelId = 5, storyFile = "Dialogue5-1", displayName = "第五关上段" }
                };
            }

            if (injectTestDataOnStart)
            {
                InjectTestData();
            }

            if (autoStartLevel1 && levels.Count > 0)
            {
                LoadLevel(0);
            }

            Debug.Log("[SL] 测试启动 | F5=存档 | F9=读档 | F1~F5=选关 | F3=显隐面板 | F6=判定结局 | 注入=" + injectTestDataOnStart);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                showGUI = !showGUI;

            if (Input.GetKeyDown(KeyCode.F5))
                SaveCheckpoint();

            if (Input.GetKeyDown(KeyCode.F9))
                LoadCheckpoint();

            if (Input.GetKeyDown(KeyCode.F6))
                TestEvaluateEnding();

            for (int i = 0; i < levels.Count && i < 5; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    LoadLevel(i);
            }
        }

        void EnsureComponents()
        {
            if (vnController == null)
                vnController = FindObjectOfType<FungusVNController>();

            if (saveSystem == null)
            {
                saveSystem = FindObjectOfType<CrossLevelSaveSystem>();
                if (saveSystem == null)
                {
                    var go = new GameObject("CrossLevelSaveSystem");
                    saveSystem = go.AddComponent<CrossLevelSaveSystem>();
                }
            }

            if (PlayerDataStore.Instance == null)
            {
                var go = new GameObject("PlayerDataStore");
                go.AddComponent<PlayerDataStore>();
            }
        }

        void OnGUI()
        {
            if (!showGUI) return;
            uiRect = GUILayout.Window(0, uiRect, DrawWindow, "SL 存档读档测试", GUILayout.Width(480));
        }

        void DrawWindow(int id)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(400));

            GUILayout.Label("=== 状态 ===", GUI.skin.box);
            GUILayout.Label(statusLog);
            if (vnController != null && vnController.dialogueText != null)
            {
                GUILayout.Label($"当前剧情: {vnController.dialogueText.name}");
            }

            GUILayout.Space(5);
            GUILayout.Label("=== 选关 (F1~F5) ===", GUI.skin.box);
            for (int i = 0; i < levels.Count; i++)
            {
                string label = $"F{i + 1}: {levels[i].displayName}";
                if (i == currentLevelIndex)
                    label += " [当前]";

                if (GUILayout.Button(label, GUILayout.Height(26)))
                    LoadLevel(i);
            }

            GUILayout.Space(5);
            GUILayout.Label("=== 存档操作 ===", GUI.skin.box);

            if (GUILayout.Button("F5 - 存档当前进度", GUILayout.Height(30)))
                SaveCheckpoint();

            if (GUILayout.Button("F9 - 读档（回到剧情开始）", GUILayout.Height(30)))
                LoadCheckpoint();

            if (GUILayout.Button("F6 - 测试结局判定", GUILayout.Height(30)))
                TestEvaluateEnding();

            if (GUILayout.Button("清除所有存档", GUILayout.Height(24)))
                ClearAllSaves();

            GUILayout.Space(5);
            GUILayout.Label("=== 存档信息 ===", GUI.skin.box);
            if (saveSystem != null && saveSystem.HasSave())
            {
                var cp = saveSystem.currentSave.checkpoint;
                GUILayout.Label($"关卡: {cp.currentLevelId}");
                GUILayout.Label($"剧情: {cp.storyFileName}");
                GUILayout.Label($"时间: {DateTimeOffset.FromUnixTimeMilliseconds(cp.saveTime):MM-dd HH:mm}");

                GUILayout.Label("关卡结果:", GUI.skin.box);
                foreach (var r in saveSystem.currentSave.levelResults)
                {
                    GUILayout.Label($"  Lv.{r.levelId}: 血量={r.finalLives} 2017={r.usedCard2017} 2026={r.usedCard2026}");
                }
            }
            else
            {
                GUILayout.Label("暂无存档");
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        [ContextMenu("加载第一关")]
        public void LoadLevel(int index)
        {
            EnsureComponents();

            if (index < 0 || index >= levels.Count)
            {
                Debug.LogWarning($"[SLTestRunner] 无效关卡索引: {index}");
                return;
            }

            currentLevelIndex = index;
            var level = levels[index];

            var textAsset = Resources.Load<TextAsset>($"Dialogue/{level.storyFile}");
            if (textAsset == null)
            {
                statusLog = $"错误: 未找到剧情文件 Dialogue/{level.storyFile}";
                Debug.LogWarning($"[SLTestRunner] 未找到剧情文件: Dialogue/{level.storyFile}");
                return;
            }

            if (vnController != null)
            {
                vnController.dialogueText = textAsset;
                vnController.RestartDialogue();
            }

            statusLog = $"已加载: {level.displayName}";
            Debug.Log($"[SL] 加载关卡 {level.levelId}: {level.storyFile}");
        }

        [ContextMenu("存档当前进度")]
        public void SaveCheckpoint()
        {
            EnsureComponents();

            if (vnController == null || vnController.dialogueText == null)
            {
                statusLog = "错误: VN 未初始化";
                return;
            }

            var level = levels[currentLevelIndex];

            saveSystem.SaveStoryProgress(level.levelId, level.storyFile);

            int testLives = UnityEngine.Random.Range(1, 4);
            bool has2017 = saveSystem.HasUsedCard(2017);
            bool has2026 = saveSystem.HasUsedCard(2026);
            saveSystem.RecordLevelResult(level.levelId, testLives, has2017, has2026);

            PlayerDataStore.Instance.SaveLevelRecord(new JsonLevelRecord(level.levelId) { timeUsed = Time.time });

            statusLog = $"已存档: {level.displayName}";
            Debug.Log($"[SL] 存档完成 Lv.{level.levelId}");
            PrintSaveSummary();
        }

        [ContextMenu("读档回到剧情开始")]
        public void LoadCheckpoint()
        {
            EnsureComponents();

            if (!saveSystem.HasSave())
            {
                statusLog = "无存档可读取";
                Debug.Log("[SL] 无存档");
                return;
            }

            var cp = saveSystem.LoadCheckpoint();
            Debug.Log($"[SL] 读档 Lv.{cp.currentLevelId} {cp.storyFileName}");

            int targetIndex = -1;
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].storyFile == cp.storyFileName ||
                    levels[i].levelId == cp.currentLevelId)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex >= 0)
            {
                LoadLevel(targetIndex);
                statusLog = $"已读档: {levels[targetIndex].displayName}";
            }
            else
            {
                LoadLevel(0);
                statusLog = "存档关卡未配置，默认回到第一关";
            }
            PrintSaveSummary();
        }

        [ContextMenu("测试结局判定")]
        public void TestEvaluateEnding()
        {
            EnsureComponents();
            int endingNone = saveSystem.EvaluateEnding(0);
            int endingAlone = saveSystem.EvaluateEnding(1);
            int endingFriend = saveSystem.EvaluateEnding(2);
            Debug.Log($"[SL] 结局判定 — 未选:{endingNone} | 独自:{endingAlone} | 邀请:{endingFriend}");
            statusLog = $"结局: 未选={endingNone} 独自={endingAlone} 邀请={endingFriend}";
        }

        [ContextMenu("清除所有存档")]
        public void ClearAllSaves()
        {
            saveSystem?.ClearAll();
            PlayerDataStore.Instance?.ClearAllRecords();
            statusLog = "已清除所有存档";
            Debug.Log("[SL] 已清除所有存档");
            Debug.Log($"[SL] 验证: HasSave={saveSystem?.HasSave()} | Records={PlayerDataStore.Instance?.GetAllRecords()?.Count}");
        }

        void InjectTestData()
        {
            if (saveSystem == null) EnsureComponents();

            // Lv.1: 1血通关（用于凑莫比乌斯环条件）
            saveSystem.RecordLevelResult(1, finalLives: 1);
            saveSystem.SaveStoryProgress(1, "Dialogue1-1");
            PlayerDataStore.Instance.SaveLevelRecord(new JsonLevelRecord(1));

            // Lv.2: 1血通关 + 卡牌2017
            saveSystem.RecordCardUsed(2017);
            saveSystem.RecordLevelResult(2, finalLives: 1, card2017: true);
            saveSystem.SaveStoryProgress(2, "Dialogue2-1");
            PlayerDataStore.Instance.SaveLevelRecord(new JsonLevelRecord(2));

            // Lv.3: 1血通关
            saveSystem.RecordLevelResult(3, finalLives: 1);
            saveSystem.SaveStoryProgress(3, "Dialogue3-1");
            PlayerDataStore.Instance.SaveLevelRecord(new JsonLevelRecord(3));

            // Lv.4: 不参与结局判定，但保留记录
            saveSystem.RecordLevelResult(4, finalLives: 3);
            saveSystem.SaveStoryProgress(4, "Dialogue4-1");
            PlayerDataStore.Instance.SaveLevelRecord(new JsonLevelRecord(4));

            // Lv.5: 2血（打破莫比乌斯环）+ 卡牌2026
            saveSystem.RecordCardUsed(2026);
            saveSystem.RecordLevelResult(5, finalLives: 2, card2026: true);
            saveSystem.SaveStoryProgress(5, "Dialogue5-1");
            PlayerDataStore.Instance.SaveLevelRecord(new JsonLevelRecord(5));

            Debug.Log("[SL] 测试数据已注入: Lv.1~5 各带不同血量/卡牌");
            PrintSaveSummary();
        }

        void PrintSaveSummary()
        {
            if (saveSystem == null || saveSystem.currentSave == null)
            {
                Debug.Log("[SL] 存档摘要: 无数据");
                return;
            }

            var cp = saveSystem.currentSave.checkpoint;
            string timeStr = DateTimeOffset.FromUnixTimeMilliseconds(cp.saveTime).ToLocalTime().ToString("HH:mm:ss");

            Debug.Log("[SL] ====== 存档摘要 ======");
            Debug.Log($"[SL] 当前关卡: {cp.currentLevelId} | 剧情: {cp.storyFileName} | 存档时间: {timeStr}");

            var results = saveSystem.currentSave.levelResults;
            Debug.Log($"[SL] 关卡结果: {results.Count} 条");
            foreach (var r in results)
            {
                Debug.Log($"[SL]   [Lv.{r.levelId}] 血量={r.finalLives} | 2017={r.usedCard2017} | 2026={r.usedCard2026}");
            }

            var cards = saveSystem.currentSave.endingData.cardsUsed;
            Debug.Log($"[SL] 全局卡牌: {(cards.Count > 0 ? string.Join(", ", cards) : "无")}");

            int endingId = saveSystem.EvaluateEnding();
            Debug.Log($"[SL] 结局判定(未选): {endingId}");
            endingId = saveSystem.EvaluateEnding(1);
            Debug.Log($"[SL] 结局判定(独自): {endingId}");
            endingId = saveSystem.EvaluateEnding(2);
            Debug.Log($"[SL] 结局判定(邀请): {endingId}");

            var records = PlayerDataStore.Instance.GetAllRecords();
            Debug.Log($"[SL] 历史记录: {records.Count} 条");
            for (int i = 0; i < records.Count; i++)
            {
                var r = records[i];
                Debug.Log($"[SL]   [{i + 1}] Lv.{r.levelId}");
            }
            Debug.Log("[SL] ====================");
        }
    }
}
