using UnityEngine;
using System.Collections.Generic;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// 存档读档测试器 — 简单 UI 验证存档数据是否被正确记录。
    /// </summary>
    public class SaveLoadTestRunner : MonoBehaviour
    {
        [Header("自动初始化")]
        public bool autoInitOnStart = true;

        [Header("测试数据")]
        public int testLevelId = 1;
        public string testEndingBranch = "eat";
        public int testEndingId = 6001;
        public List<int> testCardIds = new List<int> { 101, 102 };

        private bool showUI = true;
        private Rect uiRect = new Rect(10, 10, 400, 500);
        private Vector2 scrollPos;

        void Start()
        {
            if (autoInitOnStart)
            {
                EnsurePlayerDataStore();
                Debug.Log("[SaveLoadTestRunner] 初始化完成。按 F3 切换 UI 显隐。");
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                showUI = !showUI;
            }
        }

        void EnsurePlayerDataStore()
        {
            if (PlayerDataStore.Instance == null)
            {
                var go = new GameObject("PlayerDataStore");
                go.AddComponent<PlayerDataStore>();
            }
        }

        void OnGUI()
        {
            if (!showUI) return;

            uiRect = GUILayout.Window(0, uiRect, DrawUIWindow, "存档读档测试", GUILayout.Width(400));
        }

        void DrawUIWindow(int windowID)
        {
            GUILayout.Label("操作", GUI.skin.box);

            if (GUILayout.Button("模拟保存一条记录", GUILayout.Height(30)))
            {
                SimulateSave();
            }

            if (GUILayout.Button("读取并显示所有记录", GUILayout.Height(30)))
            {
                ReadAndDisplay();
            }

            if (GUILayout.Button("清除所有存档", GUILayout.Height(30)))
            {
                PlayerDataStore.Instance.ClearAllRecords();
            }

            GUILayout.Space(5);
            GUILayout.Label("存档内容", GUI.skin.box);

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(280));

            var records = PlayerDataStore.Instance.GetAllRecords();
            if (records.Count == 0)
            {
                GUILayout.Label("暂无存档记录");
            }
            else
            {
                foreach (var r in records)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.Label($"关卡: {r.levelId} | 时间: {System.DateTimeOffset.FromUnixTimeMilliseconds(r.timestamp):yyyy-MM-dd HH:mm}");
                    GUILayout.Label($"通关: {(r.isWin ? "是" : "否")} | 耗时: {r.timeUsed:F1}s");
                    GUILayout.Label($"结局分支: {r.endingBranch} | 结局ID: {r.endingId}");
                    GUILayout.Label($"卡牌使用: {r.cardUses.Count} 次");
                    foreach (var c in r.cardUses)
                    {
                        GUILayout.Label($"  - Card {c.cardId} @ {c.useTime:F1}s {(c.success ? "成功" : "失败")}");
                    }
                    GUILayout.Label($"任务目标: {r.taskGoals.Count} 个");
                    foreach (var g in r.taskGoals)
                    {
                        GUILayout.Label($"  - Card {g.targetCardId}: {g.currentCount}/{g.targetCount} [{g.state}]");
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Label($"按键: F3=显隐 | 存档路径:", GUI.skin.label);
            GUILayout.Label(Application.persistentDataPath, GUI.skin.label);

            GUI.DragWindow();
        }

        void SimulateSave()
        {
            EnsurePlayerDataStore();

            var record = new JsonLevelRecord(testLevelId);
            record.isWin = true;
            record.timeUsed = Random.Range(30f, 300f);
            record.endingBranch = testEndingBranch;
            record.endingId = testEndingId;

            foreach (var cardId in testCardIds)
            {
                record.cardUses.Add(new CardUseEntry(cardId, Random.Range(0f, record.timeUsed), true));
            }

            record.taskGoals.Add(new TaskGoalRecord(101, 1, 1, "Completed"));
            record.taskGoals.Add(new TaskGoalRecord(102, 0, 1, "Pending"));

            PlayerDataStore.Instance.SaveLevelRecord(record);
            Debug.Log($"[SaveLoadTestRunner] 模拟保存完成: Level {testLevelId}, Ending {testEndingId}");
        }

        void ReadAndDisplay()
        {
            EnsurePlayerDataStore();
            var records = PlayerDataStore.Instance.GetAllRecords();
            Debug.Log($"[SaveLoadTestRunner] 读取到 {records.Count} 条存档记录");
            foreach (var r in records)
            {
                Debug.Log($"  Level {r.levelId}, Win={r.isWin}, Ending={r.endingId}, Branch={r.endingBranch}, Cards={r.cardUses.Count}");
            }
        }

        [ContextMenu("测试：模拟保存")]
        void ContextSave() { SimulateSave(); }

        [ContextMenu("测试：读取显示")]
        void ContextRead() { ReadAndDisplay(); }
    }
}
