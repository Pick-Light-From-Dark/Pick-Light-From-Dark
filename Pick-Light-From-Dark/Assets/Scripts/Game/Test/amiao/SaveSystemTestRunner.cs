using System.Collections.Generic;
using UnityEngine;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// 存档系统测试脚本 — 验证存档/读档功能及数据显示
    /// 挂载到场景中任意 GameObject 即可运行测试
    /// </summary>
    public class SaveSystemTestRunner : MonoBehaviour
    {
        [Header("按键说明（运行时）")]
        [Tooltip("S = 生成模拟存档并保存")]
        public KeyCode saveKey = KeyCode.S;

        [Tooltip("L = 读取并显示所有存档")]
        public KeyCode loadKey = KeyCode.L;

        [Tooltip("C = 清除所有存档")]
        public KeyCode clearKey = KeyCode.C;

        [Tooltip("T = 测试结局记录")]
        public KeyCode endingKey = KeyCode.T;

        [Header("测试配置")]
        [Tooltip("自动在Start时生成测试存档")]
        public bool autoGenerateOnStart = true;

        [Tooltip("显示详细日志")]
        public bool verboseLog = true;

        void Start()
        {
            // 确保 PlayerDataStore 已初始化
            if (PlayerDataStore.Instance == null)
            {
                var go = new GameObject("PlayerDataStore");
                go.AddComponent<PlayerDataStore>();
            }

            if (autoGenerateOnStart)
            {
                Invoke(nameof(GenerateTestRecords), 0.5f);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(saveKey))
                GenerateTestRecords();

            if (Input.GetKeyDown(loadKey))
                DisplayAllRecords();

            if (Input.GetKeyDown(clearKey))
                ClearAllRecords();

            if (Input.GetKeyDown(endingKey))
                TestEndingRecord();
        }

        /// <summary>
        /// 生成模拟存档记录
        /// </summary>
        [ContextMenu("生成模拟存档")]
        public void GenerateTestRecords()
        {
            // 记录1：第一夜通关，结局一
            var record1 = new JsonLevelRecord(1001)
            {
                isWin = true,
                timeUsed = 245.5f,
                endingBranch = "eat1",
                endingId = 6001,
                cardUses = new List<CardUseEntry>
                {
                    new CardUseEntry(2001, 12.5f, true),
                    new CardUseEntry(2008, 45.0f, true),
                    new CardUseEntry(2010, 120.0f, true)
                },
                taskGoals = new List<TaskGoalRecord>
                {
                    new TaskGoalRecord(2008, 1, 1, "Completed")
                }
            };
            PlayerDataStore.Instance.SaveLevelRecord(record1);

            // 记录2：第二夜通关，结局二
            var record2 = new JsonLevelRecord(1002)
            {
                isWin = true,
                timeUsed = 312.0f,
                endingBranch = "share",
                endingId = 6002,
                cardUses = new List<CardUseEntry>
                {
                    new CardUseEntry(2001, 8.0f, true),
                    new CardUseEntry(2012, 55.0f, true),
                    new CardUseEntry(2015, 180.0f, true),
                    new CardUseEntry(2017, 250.0f, true)
                },
                taskGoals = new List<TaskGoalRecord>
                {
                    new TaskGoalRecord(2012, 1, 1, "Completed"),
                    new TaskGoalRecord(2015, 1, 1, "Completed")
                }
            };
            PlayerDataStore.Instance.SaveLevelRecord(record2);

            // 记录3：第三夜失败
            var record3 = new JsonLevelRecord(1003)
            {
                isWin = false,
                timeUsed = 120.0f,
                endingId = 0,
                cardUses = new List<CardUseEntry>
                {
                    new CardUseEntry(2001, 15.0f, true),
                    new CardUseEntry(2005, 80.0f, false)
                },
                taskGoals = new List<TaskGoalRecord>
                {
                    new TaskGoalRecord(2005, 0, 1, "Pending")
                }
            };
            PlayerDataStore.Instance.SaveLevelRecord(record3);

            // 记录4：第五夜通关，结局五
            var record4 = new JsonLevelRecord(1005)
            {
                isWin = true,
                timeUsed = 480.0f,
                endingBranch = "corridor",
                endingId = 6005,
                cardUses = new List<CardUseEntry>
                {
                    new CardUseEntry(2001, 10.0f, true),
                    new CardUseEntry(2025, 200.0f, true),
                    new CardUseEntry(2040, 350.0f, true)
                },
                taskGoals = new List<TaskGoalRecord>
                {
                    new TaskGoalRecord(2025, 1, 1, "Completed"),
                    new TaskGoalRecord(2040, 1, 1, "Completed")
                }
            };
            PlayerDataStore.Instance.SaveLevelRecord(record4);

            Debug.Log("[SaveSystemTestRunner] 已生成4条模拟存档记录");
        }

        /// <summary>
        /// 读取并显示所有存档数据
        /// </summary>
        [ContextMenu("显示所有存档")]
        public void DisplayAllRecords()
        {
            var records = PlayerDataStore.Instance.GetAllRecords();
            if (records == null || records.Count == 0)
            {
                Debug.LogWarning("[SaveSystemTestRunner] 无存档数据");
                return;
            }

            Debug.Log($"========== 存档数据汇总（共 {records.Count} 条）==========");
            foreach (var r in records)
            {
                string status = r.isWin ? "通关" : "失败";
                string endingName = r.endingId > 0 ? $"结局{r.endingId}" : "无结局";
                string timeStr = System.DateTimeOffset.FromUnixTimeMilliseconds(r.timestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm");

                Debug.Log($"\n[关卡 {r.levelId}] {status} | 耗时 {r.timeUsed:F1}s | {endingName} | 存档时间 {timeStr}");

                if (verboseLog)
                {
                    if (r.cardUses != null && r.cardUses.Count > 0)
                    {
                        Debug.Log($"  卡牌使用({r.cardUses.Count}次):");
                        foreach (var c in r.cardUses)
                            Debug.Log($"    - 卡牌ID:{c.cardId} 时间:{c.useTime:F1}s 成功:{c.success}");
                    }

                    if (r.taskGoals != null && r.taskGoals.Count > 0)
                    {
                        Debug.Log($"  任务目标({r.taskGoals.Count}个):");
                        foreach (var t in r.taskGoals)
                            Debug.Log($"    - 目标卡牌:{t.targetCardId} 进度:{t.currentCount}/{t.targetCount} 状态:{t.state}");
                    }
                }
            }
            Debug.Log("========== 存档数据汇总结束 ==========");
        }

        /// <summary>
        /// 清除所有存档
        /// </summary>
        [ContextMenu("清除所有存档")]
        public void ClearAllRecords()
        {
            PlayerDataStore.Instance.ClearAllRecords();
            Debug.Log("[SaveSystemTestRunner] 已清除所有存档");
        }

        /// <summary>
        /// 测试结局记录功能
        /// </summary>
        [ContextMenu("测试结局记录")]
        public void TestEndingRecord()
        {
            // 确保 LevelRecordManager 已初始化
            if (LevelRecordManager.Instance == null)
            {
                var go = new GameObject("LevelRecordManager");
                go.AddComponent<LevelRecordManager>();
            }

            LevelRecordManager.Instance.StartRecording(9999);
            LevelRecordManager.Instance.RecordEnding(6003);
            LevelRecordManager.Instance.EndRecording(true);

            // 验证
            var records = PlayerDataStore.Instance.GetRecordsByLevel(9999);
            if (records.Count > 0)
            {
                var last = records[records.Count - 1];
                if (last.endingId == 6003)
                    Debug.Log($"[SaveSystemTestRunner] 结局记录测试通过：endingId={last.endingId}");
                else
                    Debug.LogError($"[SaveSystemTestRunner] 结局记录测试失败：expected 6003, got {last.endingId}");
            }
        }
    }
}
