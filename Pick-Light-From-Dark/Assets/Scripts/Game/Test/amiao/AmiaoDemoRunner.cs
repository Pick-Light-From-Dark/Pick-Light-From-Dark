using UnityEngine;
using System;
using System.Collections.Generic;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// amiaoDemo 完整流程演示 — 存档+结局系统串联
    /// 自包含脚本，挂载到场景任意 GameObject 即可运行
    /// </summary>
    public class AmiaoDemoRunner : MonoBehaviour
    {
        [Header("剧情文本")]
        public TextAsset[] dialogueTexts = new TextAsset[5];

        [Header("结局预制体")]
        public GameObject endingPrefab6002;
        public GameObject endingPrefab6004;
        public GameObject endingPrefab6005;

        CrossLevelSaveSystem saveSystem;
        FungusVNController vnController;

        enum Phase { Menu, Dialogue, Gameplay, Evaluate, Ending }
        Phase currentPhase = Phase.Menu;
        int currentLevel = 0;
        int selectedLives = 3;
        bool useCard2017 = false;
        bool useCard2026 = false;
        int rooftopChoice = 0;

        Rect winRect = new Rect(20, 20, 500, 560);
        Vector2 scroll;
        string statusMsg = "";

        void Start()
        {
            EnsureSystems();
            Debug.Log("[Demo] amiaoDemo 已启动。按 F3 显隐面板。");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                enabled = !enabled;
        }

        void OnGUI()
        {
            winRect = GUILayout.Window(0, winRect, DrawWindow, "amiaoDemo — 存档/结局流程演示", GUILayout.Width(500));
        }

        void DrawWindow(int id)
        {
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(500));

            GUILayout.Label($"=== 当前阶段: {currentPhase} | 关卡: {currentLevel} ===", GUI.skin.box);
            if (!string.IsNullOrEmpty(statusMsg))
                GUILayout.Label($"状态: {statusMsg}");

            switch (currentPhase)
            {
                case Phase.Menu:
                    DrawMenu();
                    break;
                case Phase.Dialogue:
                    DrawDialogue();
                    break;
                case Phase.Gameplay:
                    DrawGameplay();
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
            if (GUILayout.Button("打印存档摘要", GUILayout.Height(24))) PrintSaveSummary();
            if (GUILayout.Button("读档（回到存档点）", GUILayout.Height(24))) LoadCheckpoint();
            if (GUILayout.Button("清除所有存档", GUILayout.Height(24))) ClearAll();

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        void DrawMenu()
        {
            if (GUILayout.Button("新游戏", GUILayout.Height(40)))
            {
                saveSystem.ClearAll();
                currentLevel = 1;
                currentPhase = Phase.Dialogue;
                statusMsg = "开始新游戏";
            }
            if (saveSystem.HasSave() && GUILayout.Button("继续游戏", GUILayout.Height(40)))
            {
                var cp = saveSystem.LoadCheckpoint();
                currentLevel = cp.currentLevelId;
                currentPhase = cp.isInGameplay ? Phase.Gameplay : Phase.Dialogue;
                statusMsg = $"继续 Lv.{currentLevel}";
            }
        }

        void DrawDialogue()
        {
            GUILayout.Label($"第 {currentLevel} 关 — 剧情阶段", GUI.skin.box);

            var textAsset = currentLevel >= 1 && currentLevel <= 5
                ? dialogueTexts[currentLevel - 1]
                : null;

            if (textAsset != null)
            {
                GUILayout.Label(textAsset.text.Substring(0, Mathf.Min(200, textAsset.text.Length)) + "...");
            }
            else
            {
                GUILayout.Label("（未配置剧情文本，使用模拟剧情）");
            }

            if (GUILayout.Button("进入游玩", GUILayout.Height(32)))
            {
                string fileName = textAsset != null ? textAsset.name : $"Dialogue{currentLevel}-1";
                saveSystem.SaveStoryProgress(currentLevel, fileName, 0);
                currentPhase = Phase.Gameplay;
                statusMsg = $"Lv.{currentLevel} 进入游玩";
            }
        }

        void DrawGameplay()
        {
            GUILayout.Label($"第 {currentLevel} 关 — 游玩阶段", GUI.skin.box);

            GUILayout.Label("通关时剩余血量:");
            selectedLives = Mathf.RoundToInt(GUILayout.HorizontalSlider(selectedLives, 1, 3));
            GUILayout.Label($"{selectedLives} 点");

            GUILayout.Space(5);
            if (currentLevel == 2)
            {
                useCard2017 = GUILayout.Toggle(useCard2017, "使用卡牌 2017（分享泡面）");
            }
            else if (currentLevel == 5)
            {
                useCard2026 = GUILayout.Toggle(useCard2026, "使用卡牌 2026（寻求帮助）");
            }

            GUILayout.Space(5);
            if (GUILayout.Button("通关本关", GUILayout.Height(32)))
            {
                if (useCard2017) saveSystem.RecordCardUsed(2017);
                if (useCard2026) saveSystem.RecordCardUsed(2026);

                saveSystem.RecordLevelResult(
                    currentLevel,
                    selectedLives,
                    card2017: useCard2017,
                    card2026: useCard2026
                );

                statusMsg = $"Lv.{currentLevel} 通关，血量={selectedLives}";

                if (currentLevel >= 5)
                {
                    currentPhase = Phase.Evaluate;
                }
                else
                {
                    currentLevel++;
                    currentPhase = Phase.Dialogue;
                    useCard2017 = false;
                    useCard2026 = false;
                }
            }
        }

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

        void DrawEnding()
        {
            GUILayout.Label("结局已触发", GUI.skin.box);
            int endingId = saveSystem.EvaluateEnding(rooftopChoice);
            string name = endingId switch
            {
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
                currentLevel = 0;
                rooftopChoice = 0;
            }
        }

        void TriggerEnding()
        {
            int endingId = saveSystem.EvaluateEnding(rooftopChoice);
            currentPhase = Phase.Ending;
            statusMsg = $"触发结局 {endingId}";
            Debug.Log($"[Demo] 结局判定: {endingId}");

            GameObject prefab = endingId switch
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

        void LoadCheckpoint()
        {
            if (!saveSystem.HasSave())
            {
                statusMsg = "无存档";
                return;
            }
            var cp = saveSystem.LoadCheckpoint();
            currentLevel = cp.currentLevelId;
            currentPhase = cp.isInGameplay ? Phase.Gameplay : Phase.Dialogue;
            statusMsg = $"读档到 Lv.{currentLevel}";
            Debug.Log($"[Demo] 读档: Lv.{currentLevel}, {cp.storyFileName}");
        }

        void ClearAll()
        {
            saveSystem.ClearAll();
            currentLevel = 0;
            currentPhase = Phase.Menu;
            statusMsg = "存档已清除";
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
            vnController = FindObjectOfType<FungusVNController>();
        }
    }
}
