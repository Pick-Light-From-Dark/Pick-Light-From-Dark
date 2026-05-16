using UnityEngine;
using System;
using System.Collections.Generic;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// amiaoDemo 完整流程演示 — 参考 LevelFlowCoordinator 架构
    /// 真正调用 VN 控制器播放剧情，回调串联剧情→游玩→剧情流程
    /// </summary>
    public class AmiaoDemoRunner : MonoBehaviour
    {
        [System.Serializable]
        public class LevelConfig
        {
            public int levelId;
            public string openingStory;
            public string endingStory;
            public bool hasGameplay = true;
        }

        [Header("VN 控制器（为空则自动查找）")]
        public FungusVNController vnController;

        [Header("存档系统（为空则自动创建）")]
        public CrossLevelSaveSystem saveSystem;

        [Header("关卡配置")]
        public List<LevelConfig> levelConfigs = new List<LevelConfig>
        {
            new LevelConfig { levelId = 1, openingStory = "Dialogue1-1", endingStory = "Dialogue1-2b" },
            new LevelConfig { levelId = 2, openingStory = "Dialogue2-1", endingStory = "Dialogue2-2" },
            new LevelConfig { levelId = 3, openingStory = "Dialogue3-1", endingStory = "Dialogue3-2" },
            new LevelConfig { levelId = 4, openingStory = "Dialogue4-1", endingStory = "Dialogue4-2" },
            new LevelConfig { levelId = 5, openingStory = "Dialogue5-1" }
        };

        [Header("结局预制体")]
        public GameObject endingPrefab6002;
        public GameObject endingPrefab6004;
        public GameObject endingPrefab6005;

        [Header("调试")]
        public bool showGUI = true;

        enum Phase { Menu, OpeningDialogue, Gameplay, EndingDialogue, Evaluate, Ending }
        Phase currentPhase = Phase.Menu;
        int currentLevelIndex = 0;
        int selectedLives = 3;
        bool useCard2017 = false;
        bool useCard2026 = false;
        int rooftopChoice = 0;
        int currentEndingId = 0;

        Rect winRect = new Rect(20, 20, 500, 560);
        Vector2 scroll;
        string statusMsg = "";

        void Start()
        {
            EnsureSystems();
            if (vnController == null)
                vnController = FindObjectOfType<FungusVNController>();
            Debug.Log("[Demo] amiaoDemo 已启动。按 F3 显隐面板。");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                showGUI = !showGUI;
        }

        void OnGUI()
        {
            if (!showGUI) return;
            winRect = GUILayout.Window(0, winRect, DrawWindow, "amiaoDemo — 存档/结局流程演示", GUILayout.Width(500));
        }

        void DrawWindow(int id)
        {
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(500));

            GUILayout.Label($"=== 当前阶段: {currentPhase} | 关卡: {GetCurrentLevelId()} ===", GUI.skin.box);
            if (!string.IsNullOrEmpty(statusMsg))
                GUILayout.Label($"状态: {statusMsg}");

            switch (currentPhase)
            {
                case Phase.Menu:
                    DrawMenu();
                    break;
                case Phase.OpeningDialogue:
                    DrawOpeningDialogue();
                    break;
                case Phase.Gameplay:
                    DrawGameplay();
                    break;
                case Phase.EndingDialogue:
                    DrawEndingDialogue();
                    break;
                case Phase.Evaluate:
                    DrawEvaluate();
                    break;
                case Phase.Ending:
                    DrawEnding();
                    break;
            }

            GUILayout.Space(10);
            GUILayout.Label("=== 存档调试 ===", GUI.skin.box);
            if (GUILayout.Button("存档当前进度", GUILayout.Height(24))) SaveCurrentProgress();
            if (GUILayout.Button("读档（回到存档点）", GUILayout.Height(24))) LoadCheckpoint();
            if (GUILayout.Button("打印存档摘要", GUILayout.Height(24))) PrintSaveSummary();
            if (GUILayout.Button("清除所有存档", GUILayout.Height(24))) ClearAll();

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        // ========== Menu ==========

        void DrawMenu()
        {
            if (GUILayout.Button("新游戏", GUILayout.Height(40)))
            {
                saveSystem.ClearAll();
                currentLevelIndex = 0;
                rooftopChoice = 0;
                useCard2017 = false;
                useCard2026 = false;
                StartOpeningDialogue();
            }
            if (saveSystem.HasSave() && GUILayout.Button("继续游戏", GUILayout.Height(40)))
            {
                LoadCheckpoint();
                statusMsg = $"继续 Lv.{GetCurrentLevelId()}";
            }
        }

        // ========== Opening Dialogue ==========

        void DrawOpeningDialogue()
        {
            GUILayout.Label($"第 {GetCurrentLevelId()} 关 — 剧情播放中", GUI.skin.box);
            GUILayout.Label("VN 控制器正在播放剧情...");
            GUILayout.Label("剧情结束后自动进入下一阶段");
        }

        void StartOpeningDialogue()
        {
            var config = levelConfigs[currentLevelIndex];
            var textAsset = Resources.Load<TextAsset>($"Dialogue/{config.openingStory}");
            if (textAsset == null)
            {
                statusMsg = $"错误: 未找到 {config.openingStory}";
                Debug.LogWarning($"[Demo] 未找到剧情文件: Dialogue/{config.openingStory}");
                return;
            }

            vnController.dialogueText = textAsset;
            vnController.OnDialogueExit = OnOpeningDialogueExit;
            vnController.OnDialogueComplete = OnOpeningDialogueEnd;
            vnController.RestartDialogue();
            currentPhase = Phase.OpeningDialogue;
            statusMsg = $"播放剧情: {config.openingStory}";
            Debug.Log($"[Demo] 开始播放剧情: {config.openingStory}");
        }

        void OnOpeningDialogueExit(VNExitType exitType)
        {
            Debug.Log($"[Demo] 开场剧情分支: {exitType}");
            switch (exitType)
            {
                case VNExitType.Ending:
                    // 第一关"不吃"→结局一
                    ShowEnding1();
                    break;
                case VNExitType.Gameplay:
                case VNExitType.None:
                default:
                    OnOpeningDialogueEnd();
                    break;
            }
        }

        void OnOpeningDialogueEnd()
        {
            var config = levelConfigs[currentLevelIndex];
            saveSystem.SaveStoryProgress(config.levelId, config.openingStory);

            if (config.hasGameplay)
            {
                currentPhase = Phase.Gameplay;
                statusMsg = $"Lv.{config.levelId} 进入游玩";
            }
            else
            {
                StartEndingDialogue();
            }
        }

        // ========== Gameplay ==========

        void DrawGameplay()
        {
            GUILayout.Label($"第 {GetCurrentLevelId()} 关 — 游玩阶段", GUI.skin.box);

            GUILayout.Label("通关时剩余血量:");
            selectedLives = Mathf.RoundToInt(GUILayout.HorizontalSlider(selectedLives, 1, 3));
            GUILayout.Label($"{selectedLives} 点");

            GUILayout.Space(5);
            if (GetCurrentLevelId() == 2)
            {
                useCard2017 = GUILayout.Toggle(useCard2017, "使用卡牌 2017（分享泡面）");
            }
            else if (GetCurrentLevelId() == 5)
            {
                useCard2026 = GUILayout.Toggle(useCard2026, "使用卡牌 2026（寻求帮助）");
            }

            GUILayout.Space(5);
            if (GUILayout.Button("通关本关", GUILayout.Height(32)))
            {
                if (useCard2017) saveSystem.RecordCardUsed(2017);
                if (useCard2026) saveSystem.RecordCardUsed(2026);

                saveSystem.RecordLevelResult(
                    GetCurrentLevelId(),
                    selectedLives,
                    card2017: useCard2017,
                    card2026: useCard2026
                );

                statusMsg = $"Lv.{GetCurrentLevelId()} 通关，血量={selectedLives}";

                if (GetCurrentLevelId() >= 5)
                {
                    currentPhase = Phase.Evaluate;
                }
                else
                {
                    StartEndingDialogue();
                }
            }
        }

        // ========== Ending Dialogue ==========

        void DrawEndingDialogue()
        {
            GUILayout.Label($"第 {GetCurrentLevelId()} 关 — 结尾剧情播放中", GUI.skin.box);
            GUILayout.Label("VN 控制器正在播放结尾剧情...");
        }

        void StartEndingDialogue()
        {
            var config = levelConfigs[currentLevelIndex];
            if (string.IsNullOrEmpty(config.endingStory))
            {
                NextLevelOrEvaluate();
                return;
            }

            var textAsset = Resources.Load<TextAsset>($"Dialogue/{config.endingStory}");
            if (textAsset == null)
            {
                NextLevelOrEvaluate();
                return;
            }

            vnController.dialogueText = textAsset;
            vnController.OnDialogueExit = OnEndingDialogueExit;
            vnController.OnDialogueComplete = OnEndingDialogueEnd;
            vnController.RestartDialogue();
            currentPhase = Phase.EndingDialogue;
            statusMsg = $"播放结尾剧情: {config.endingStory}";
        }

        /// <summary>通用剧情加载 — 根据任意文件名加载并播放</summary>
        void StartStoryDialogue(string storyFileName)
        {
            var textAsset = Resources.Load<TextAsset>($"Dialogue/{storyFileName}");
            if (textAsset == null)
            {
                statusMsg = $"错误: 未找到 {storyFileName}";
                Debug.LogWarning($"[Demo] 未找到剧情文件: Dialogue/{storyFileName}");
                return;
            }

            vnController.dialogueText = textAsset;
            vnController.OnDialogueExit = OnGenericDialogueExit;
            vnController.OnDialogueComplete = OnGenericDialogueEnd;
            vnController.RestartDialogue();
            currentPhase = Phase.OpeningDialogue;
            statusMsg = $"播放剧情: {storyFileName}";
            Debug.Log($"[Demo] 开始播放剧情: {storyFileName}");
        }

        void OnGenericDialogueExit(VNExitType exitType)
        {
            Debug.Log($"[Demo] 剧情分支: {exitType}");
            OnGenericDialogueEnd();
        }

        void OnGenericDialogueEnd()
        {
            var config = levelConfigs[currentLevelIndex];
            saveSystem.SaveStoryProgress(config.levelId, vnController.dialogueText.name);

            if (config.hasGameplay)
            {
                currentPhase = Phase.Gameplay;
                statusMsg = $"Lv.{config.levelId} 进入游玩";
            }
            else
            {
                StartEndingDialogue();
            }
        }

        void OnEndingDialogueExit(VNExitType exitType)
        {
            switch (exitType)
            {
                case VNExitType.NextLevel:
                    NextLevelOrEvaluate();
                    break;
                default:
                    OnEndingDialogueEnd();
                    break;
            }
        }

        void OnEndingDialogueEnd()
        {
            NextLevelOrEvaluate();
        }

        void NextLevelOrEvaluate()
        {
            var config = levelConfigs[currentLevelIndex];
            if (config.levelId >= 5)
            {
                currentPhase = Phase.Evaluate;
            }
            else
            {
                currentLevelIndex++;
                useCard2017 = false;
                useCard2026 = false;
                StartOpeningDialogue();
            }
        }

        // ========== Evaluate ==========

        void DrawEvaluate()
        {
            GUILayout.Label("第五关结束 — 结局判定", GUI.skin.box);

            var r2 = saveSystem.GetLevelResult(2);
            var r5 = saveSystem.GetLevelResult(5);
            bool bothCards = (r2?.usedCard2017 == true) && (r5?.usedCard2026 == true);

            GUILayout.Label($"第二关卡牌2017: {r2?.usedCard2017 ?? false}");
            GUILayout.Label($"第五关卡牌2026: {r5?.usedCard2026 ?? false}");

            if (bothCards)
            {
                GUILayout.Label("两卡全部使用，请选择木门选项:");
                if (GUILayout.Button("独自前往"))
                {
                    rooftopChoice = 1;
                    TriggerEnding();
                }
                if (GUILayout.Button("邀请宋明月"))
                {
                    rooftopChoice = 2;
                    TriggerEnding();
                }
            }
            else
            {
                if (GUILayout.Button("自动判定结局", GUILayout.Height(32)))
                {
                    rooftopChoice = 0;
                    TriggerEnding();
                }
            }
        }

        // ========== Ending ==========

        void DrawEnding()
        {
            GUILayout.Label("结局已触发", GUI.skin.box);
            int endingId = currentEndingId;
            string name = endingId switch
            {
                6001 => "结局一：太阳照常升起",
                6002 => "结局二：莫比乌斯环",
                6004 => "结局四：星垂之夜",
                6005 => "结局五：北极星",
                _ => "未知结局"
            };
            GUILayout.Label($"结局ID: {endingId}");
            GUILayout.Label($"名称: {name}");

            if (GUILayout.Button("返回主菜单", GUILayout.Height(32)))
            {
                currentPhase = Phase.Menu;
                currentLevelIndex = 0;
                rooftopChoice = 0;
            }
        }

        void ShowEnding1()
        {
            currentPhase = Phase.Ending;
            currentEndingId = 6001;
            statusMsg = "触发结局一（6001：太阳照常升起）";
            Debug.Log("[Demo] 结局一触发");
        }

        void TriggerEnding()
        {
            currentEndingId = saveSystem.EvaluateEnding(rooftopChoice);
            currentPhase = Phase.Ending;
            statusMsg = $"触发结局 {currentEndingId}";
            Debug.Log($"[Demo] 结局判定: {currentEndingId}");

            GameObject prefab = currentEndingId switch
            {
                6002 => endingPrefab6002,
                6004 => endingPrefab6004,
                6005 => endingPrefab6005,
                _ => null
            };

            if (prefab != null)
            {
                var canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null) Instantiate(prefab, canvas.transform, false);
            }
        }

        // ========== Save / Load ==========

        void LoadCheckpoint()
        {
            if (!saveSystem.HasSave())
            {
                statusMsg = "无存档";
                return;
            }

            var cp = saveSystem.LoadCheckpoint();

            // 1. 找到关卡索引
            for (int i = 0; i < levelConfigs.Count; i++)
            {
                if (levelConfigs[i].levelId == cp.currentLevelId)
                {
                    currentLevelIndex = i;
                    break;
                }
            }
            var config = levelConfigs[currentLevelIndex];

            // 2. 重置 saveFlowchart，避免 RestartDialogue() 直接跳过
            if (vnController != null && vnController.saveFlowchart != null)
            {
                vnController.saveFlowchart.SetBooleanVariable("VN_IsOpeningDone", false);
            }

            // 3. 恢复 LevelResult 状态（血量 / 卡牌）
            var result = saveSystem.GetLevelResult(config.levelId);
            if (result != null)
            {
                selectedLives = Mathf.Clamp(result.finalLives, 1, 3);
                useCard2017 = result.usedCard2017;
                useCard2026 = result.usedCard2026;
            }
            else
            {
                selectedLives = 3;
                useCard2017 = false;
                useCard2026 = false;
            }

            // 4. 读取 PlayerData
            var playerRecord = PlayerDataStore.Instance?.GetLastRecord(config.levelId);
            string playerDataInfo = playerRecord != null
                ? $" PlayerData: time={playerRecord.timeUsed:F1}s win={playerRecord.isWin}"
                : " PlayerData: 无记录";

            // 5. 根据 storyFileName 决定进入哪个阶段
            if (string.IsNullOrEmpty(cp.storyFileName))
            {
                currentPhase = Phase.Gameplay;
                statusMsg = $"读档到 Lv.{config.levelId} 游玩中{playerDataInfo}";
            }
            else if (cp.storyFileName == config.openingStory)
            {
                StartOpeningDialogue();
                statusMsg = $"读档到 Lv.{config.levelId} 开场剧情{playerDataInfo}";
            }
            else if (cp.storyFileName == config.endingStory)
            {
                StartEndingDialogue();
                statusMsg = $"读档到 Lv.{config.levelId} 结尾剧情{playerDataInfo}";
            }
            else
            {
                StartStoryDialogue(cp.storyFileName);
                statusMsg = $"读档到 Lv.{config.levelId} 剧情 {cp.storyFileName}{playerDataInfo}";
            }

            Debug.Log($"[Demo] 读档: Lv.{config.levelId}, {cp.storyFileName}{playerDataInfo}");
        }

        void ClearAll()
        {
            saveSystem.ClearAll();
            currentLevelIndex = 0;
            currentPhase = Phase.Menu;
            rooftopChoice = 0;
            statusMsg = "存档已清除";
        }

        void SaveCurrentProgress()
        {
            var config = levelConfigs[currentLevelIndex];
            switch (currentPhase)
            {
                case Phase.OpeningDialogue:
                    saveSystem.SaveStoryProgress(config.levelId, config.openingStory);
                    statusMsg = $"已存档: Lv.{config.levelId} 开场剧情";
                    break;
                case Phase.Gameplay:
                    saveSystem.SaveGameplayProgress(config.levelId);
                    SavePlayerData(config.levelId);
                    statusMsg = $"已存档: Lv.{config.levelId} 游玩中 + PlayerData";
                    break;
                case Phase.EndingDialogue:
                    saveSystem.SaveStoryProgress(config.levelId, config.endingStory);
                    statusMsg = $"已存档: Lv.{config.levelId} 结尾剧情";
                    break;
                default:
                    statusMsg = "当前阶段无需存档";
                    break;
            }
            Debug.Log($"[Demo] {statusMsg}");
        }

        void SavePlayerData(int levelId)
        {
            var record = new JsonLevelRecord(levelId)
            {
                timeUsed = Time.time,
                isWin = false
            };
            PlayerDataStore.Instance?.SaveLevelRecord(record);
        }

        void PrintSaveSummary()
        {
            if (!saveSystem.HasSave())
            {
                Debug.Log("[Demo] 无存档");
                return;
            }
            var cp = saveSystem.currentSave.checkpoint;
            Debug.Log($"[Demo] 关卡={cp.currentLevelId} | 剧情={cp.storyFileName}");
            foreach (var r in saveSystem.currentSave.levelResults)
            {
                Debug.Log($"[Demo]   Lv.{r.levelId}: 血量={r.finalLives} 2017={r.usedCard2017} 2026={r.usedCard2026}");
            }
            int ending = saveSystem.EvaluateEnding(0);
            Debug.Log($"[Demo] 结局判定(未选): {ending}");
        }

        // ========== Helpers ==========

        void EnsureSystems()
        {
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

        int GetCurrentLevelId()
        {
            if (currentLevelIndex >= 0 && currentLevelIndex < levelConfigs.Count)
                return levelConfigs[currentLevelIndex].levelId;
            return 0;
        }
    }
}
