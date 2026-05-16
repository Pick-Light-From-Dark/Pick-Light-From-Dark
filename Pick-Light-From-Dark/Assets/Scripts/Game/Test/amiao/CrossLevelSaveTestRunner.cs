using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// 跨关卡读档测试器 — 验证精简版存档结构
    /// 只测试读档定位与结局判定必需数据（血量/卡牌）
    /// 6001 由剧情选项直接触发，不在本存档系统中测试
    /// </summary>
    public class CrossLevelSaveTestRunner : MonoBehaviour
    {
        [Header("测试剧情文本")]
        public TextAsset dialogue1;
        public TextAsset dialogue1_1;
        public TextAsset dialogue2;

        [Header("VN 控制器（运行时自动查找）")]
        public FungusVNController vnController;

        [Header("测试配置")]
        public bool autoTestOnStart = false;
        public float autoTestDelay = 2f;

        CrossLevelSaveSystem saveSystem;
        bool showUI = true;
        Rect uiRect = new Rect(10, 10, 450, 520);
        Vector2 scrollPos;
        List<string> logLines = new List<string>();

        int simulatedLevel = 0;
        string simulatedPhase = "未开始";

        void Start()
        {
            EnsureSaveSystem();
            FindVN();
            Log("[CrossSaveTest] 跨关卡存档测试器启动。按 F5 切换窗口。");

            if (autoTestOnStart)
            {
                StartCoroutine(AutoTestSequence());
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
                showUI = !showUI;
        }

        void EnsureSaveSystem()
        {
            saveSystem = FindObjectOfType<CrossLevelSaveSystem>();
            if (saveSystem == null)
            {
                var go = new GameObject("CrossLevelSaveSystem");
                saveSystem = go.AddComponent<CrossLevelSaveSystem>();
            }
        }

        void FindVN()
        {
            if (vnController == null)
                vnController = FindObjectOfType<FungusVNController>();
        }

        void OnGUI()
        {
            if (!showUI) return;
            uiRect = GUILayout.Window(0, uiRect, DrawUIWindow, "跨关卡存档测试器", GUILayout.Width(450));
        }

        void DrawUIWindow(int id)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(460));

            GUILayout.Label("=== 模拟状态 ===", GUI.skin.box);
            GUILayout.Label($"当前阶段: {simulatedPhase}");
            GUILayout.Label($"模拟关卡: {simulatedLevel}");

            GUILayout.Space(5);
            GUILayout.Label("=== 存档操作 ===", GUI.skin.box);

            GUILayout.Label("关卡进度", GUI.skin.label);
            if (GUILayout.Button("1. 模拟：存档到 Dialogue2 开始处", GUILayout.Height(28)))
            {
                SimulateSaveAtDialogue2();
            }
            if (GUILayout.Button("2. 模拟：存档到 Level1 游玩中", GUILayout.Height(28)))
            {
                SimulateSaveAtGameplay();
            }

            GUILayout.Space(3);
            GUILayout.Label("结局数据", GUI.skin.label);
            if (GUILayout.Button("3. 记录卡牌使用 2017", GUILayout.Height(24)))
            {
                saveSystem?.RecordCardUsed(2017);
                Log("[Test] 记录卡牌: 2017");
            }
            if (GUILayout.Button("4. 记录卡牌使用 2026", GUILayout.Height(24)))
            {
                saveSystem?.RecordCardUsed(2026);
                Log("[Test] 记录卡牌: 2026");
            }
            if (GUILayout.Button("5. 记录 Lv.1 结果(1血,有2017)", GUILayout.Height(24)))
            {
                saveSystem?.RecordLevelResult(1, finalLives: 1, card2017: true);
                Log("[Test] 记录 Lv.1 结果: 1血, 2017=true");
            }
            if (GUILayout.Button("6. 记录 Lv.5 结果(2血,有2026)", GUILayout.Height(24)))
            {
                saveSystem?.RecordLevelResult(5, finalLives: 2, card2026: true);
                Log("[Test] 记录 Lv.5 结果: 2血, 2026=true");
            }

            GUILayout.Space(5);
            GUILayout.Label("=== 读档与验证 ===", GUI.skin.box);
            if (GUILayout.Button("7. 读档并显示结果", GUILayout.Height(30)))
            {
                LoadAndDisplay();
            }
            if (GUILayout.Button("8. 测试结局判定", GUILayout.Height(30)))
            {
                TestEndingEvaluation();
            }
            if (GUILayout.Button("9. 执行完整跨关卡流程测试", GUILayout.Height(30)))
            {
                StartCoroutine(FullFlowTest());
            }
            if (GUILayout.Button("10. 清除所有存档", GUILayout.Height(24)))
            {
                saveSystem?.ClearAll();
                simulatedLevel = 0;
                simulatedPhase = "已清除";
                Log("[Test] 已清除所有存档");
            }

            GUILayout.Space(5);
            GUILayout.Label("=== 存档内容 ===", GUI.skin.box);
            if (saveSystem != null && saveSystem.HasSave())
            {
                var cp = saveSystem.currentSave.checkpoint;
                var ed = saveSystem.currentSave.endingData;
                GUILayout.Label($"关卡: {cp.currentLevelId} | 剧情: {cp.storyFileName}");
                GUILayout.Label($"卡牌: {string.Join(", ", ed.cardsUsed)}");
                GUILayout.Label("关卡结果:", GUI.skin.label);
                foreach (var r in saveSystem.currentSave.levelResults)
                {
                    GUILayout.Label($"  Lv.{r.levelId}: 血量={r.finalLives} 2017={r.usedCard2017} 2026={r.usedCard2026}");
                }
            }
            else
            {
                GUILayout.Label("暂无存档");
            }

            GUILayout.Space(5);
            GUILayout.Label("=== 日志 ===", GUI.skin.box);
            foreach (var line in logLines)
            {
                GUILayout.Label(line);
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        void SimulateSaveAtDialogue2()
        {
            EnsureSaveSystem();
            simulatedLevel = 2;
            simulatedPhase = "Dialogue2 剧情开始";
            string storyFile = dialogue2 != null ? dialogue2.name : "Dialogue2";
            saveSystem.SaveStoryProgress(2, storyFile, 0);
            Log($"[Test] 已存档到 Dialogue2 开始处 (Level=2, Story={storyFile})");
        }

        void SimulateSaveAtGameplay()
        {
            EnsureSaveSystem();
            simulatedLevel = 1;
            simulatedPhase = "Level1 游玩中";
            saveSystem.SaveGameplayProgress(1);
            Log("[Test] 已存档到 Level1 游玩中");
        }

        void LoadAndDisplay()
        {
            EnsureSaveSystem();
            if (!saveSystem.HasSave())
            {
                Log("[Test] 无存档可读取");
                return;
            }

            var cp = saveSystem.LoadCheckpoint();
            var ed = saveSystem.currentSave.endingData;

            Log("[Test] === 读档结果 ===");
            Log($"  关卡: {cp.currentLevelId}");
            Log($"  模式: {(cp.isInGameplay ? "游玩" : "剧情")}");
            Log($"  剧情文件: {cp.storyFileName}");
            Log($"  剧情行: {cp.storyLineIndex}");
            Log($"  卡牌记录: {ed.cardsUsed.Count} 张");

            foreach (var r in saveSystem.currentSave.levelResults)
            {
                Log($"  Lv.{r.levelId}: 血量={r.finalLives} 2017={r.usedCard2017} 2026={r.usedCard2026}");
            }

            if (!cp.isInGameplay && !string.IsNullOrEmpty(cp.storyFileName))
            {
                Log($"[Test] 验证：读档后应加载剧情 '{cp.storyFileName}' 从第 {cp.storyLineIndex} 行开始");
            }
            else if (cp.isInGameplay)
            {
                Log($"[Test] 验证：读档后应进入 Level {cp.currentLevelId} 游玩");
            }
        }

        void TestEndingEvaluation()
        {
            EnsureSaveSystem();
            int none = saveSystem.EvaluateEnding(0);
            int alone = saveSystem.EvaluateEnding(1);
            int friend = saveSystem.EvaluateEnding(2);
            Log($"[Test] 结局判定 — 未选:{none} 独自:{alone} 邀请:{friend}");
        }

        IEnumerator FullFlowTest()
        {
            Log("[Test] ====== 开始完整跨关卡流程测试 ======");

            simulatedLevel = 1;
            simulatedPhase = "Dialogue1 剧情";
            Log("[Test] Step 1: 播放 Dialogue1...");
            yield return new WaitForSeconds(0.5f);

            simulatedPhase = "Level1 游玩";
            Log("[Test] Step 2: 进入 Level1 游玩...");
            saveSystem.SaveGameplayProgress(1);
            saveSystem.RecordLevelResult(1, finalLives: 1, card2017: true);
            yield return new WaitForSeconds(0.5f);

            simulatedPhase = "Dialogue1-1 剧情";
            Log("[Test] Step 3: 进入 Dialogue1-1...");
            saveSystem.SaveStoryProgress(1, "Dialogue1-1", 0);
            yield return new WaitForSeconds(0.5f);

            simulatedLevel = 2;
            simulatedPhase = "Dialogue2 剧情（存档点）";
            Log("[Test] Step 4: 进入 Dialogue2，在此处存档...");
            saveSystem.SaveStoryProgress(2, "Dialogue2", 0);
            yield return new WaitForSeconds(0.5f);

            simulatedLevel = 3;
            simulatedPhase = "Dialogue3 剧情（新进度）";
            Log("[Test] Step 5: 继续到 Dialogue3（新进度）...");
            yield return new WaitForSeconds(0.3f);

            Log("[Test] Step 6: 读档，应回到 Dialogue2...");
            var cp = saveSystem.LoadCheckpoint();
            simulatedLevel = cp.currentLevelId;
            simulatedPhase = $"读档回到 {cp.storyFileName}";
            Log($"[Test] 读档成功: Level={cp.currentLevelId}, Story={cp.storyFileName}");

            bool hasCard = saveSystem.HasUsedCard(2017);
            Log($"[Test] Step 7: 验证卡牌保留: 2017={(hasCard ? "通过" : "失败")}");

            var r1 = saveSystem.GetLevelResult(1);
            Log($"[Test] Step 8: 验证关卡结果保留: Lv.1 血量={(r1?.finalLives.ToString() ?? "无数据")}");

            Log("[Test] ====== 测试完成 ======");
        }

        IEnumerator AutoTestSequence()
        {
            yield return new WaitForSeconds(autoTestDelay);
            Log("[Test] 自动执行完整流程测试...");
            yield return StartCoroutine(FullFlowTest());
        }

        void Log(string msg)
        {
            logLines.Add(msg);
            if (logLines.Count > 60) logLines.RemoveAt(0);
            Debug.Log(msg);
        }

        [ContextMenu("执行完整流程测试")]
        void ContextMenuFullTest()
        {
            StartCoroutine(FullFlowTest());
        }
    }
}
