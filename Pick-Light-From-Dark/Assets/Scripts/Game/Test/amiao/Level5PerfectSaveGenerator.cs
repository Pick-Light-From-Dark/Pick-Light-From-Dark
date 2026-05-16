using System;
using System.Collections.Generic;
using Game.Backend;
using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// Level 5 完美存档生成器 — 一键生成满血+全卡的测试存档，并支持加载启动
    /// </summary>
    public class Level5PerfectSaveGenerator : MonoBehaviour
    {
        public enum EndingBranchOption
        {
            Ending6004_XingChui = 6004,
            Ending6005_BeiJiXing = 6005
        }

        [Header("结局配置")]
        [Tooltip("6004=星垂之夜, 6005=北极星")]
        public EndingBranchOption endingBranch = EndingBranchOption.Ending6005_BeiJiXing;

        [Tooltip("0=未选择, 1=独自前往(6004), 2=邀请宋明月(6005)")]
        [Range(0, 2)]
        public int rooftopChoice = 2;

        [Header("状态配置")]
        [Tooltip("最终血量，默认2即满血")]
        public int finalLives = 2;

        [Tooltip("是否同时记录 Level 5 初始4张卡到 PlayerDataStore")]
        public bool includeLevel5InitialCards = false;

        [Header("自动行为")]
        [Tooltip("Play 时自动注入存档")]
        public bool autoInjectOnPlay = true;

        [Tooltip("Play 时自动加载并启动 Level 5 剧情")]
        public bool autoLoadOnPlay = false;

        [Header("组件引用（留空则自动查找/创建）")]
        public CrossLevelSaveSystem saveSystem;
        public FungusVNController vnController;

        string statusLog = "就绪";

        void Start()
        {
            EnsureComponents();

            if (autoInjectOnPlay)
            {
                GenerateSave();
            }

            if (autoLoadOnPlay)
            {
                LoadAndStart();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    ClearAllSaves();
                }
                else
                {
                    GenerateSave();
                }
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                LoadAndStart();
            }
        }

        [ContextMenu("生成 Level 5 完美存档")]
        public void GenerateSave()
        {
            EnsureComponents();

            // 清理旧数据，避免重复累积
            saveSystem.ClearAll();
            PlayerDataStore.Instance.ClearAllRecords();

            // === CrossLevelSaveSystem ===
            // Lv.2: 满血 + 卡牌2017
            saveSystem.RecordLevelResult(2, finalLives: finalLives, card2017: true);
            saveSystem.SaveStoryProgress(2, "Dialogue2-1", 0);
            saveSystem.RecordCardUsed(2017);

            // Lv.5: 满血 + 卡牌2026
            saveSystem.RecordLevelResult(5, finalLives: finalLives, card2026: true);
            saveSystem.SaveStoryProgress(5, "Dialogue5-1", 0);
            saveSystem.RecordCardUsed(2026);

            // === PlayerDataStore ===
            // Lv.2 记录（简化）
            var recordLv2 = new JsonLevelRecord(2)
            {
                isWin = true,
                timeUsed = 300f,
                cardUses = new List<CardUseEntry> { new CardUseEntry(2017, 120f, true) }
            };
            PlayerDataStore.Instance.SaveLevelRecord(recordLv2);

            // Lv.5 记录（完整）
            var recordLv5 = BuildLevel5Record();
            PlayerDataStore.Instance.SaveLevelRecord(recordLv5);

            statusLog = $"已生成: {endingBranch} | 血量={finalLives} | 2026已用";
            Debug.Log($"[L5Save] 完美存档已生成 — {endingBranch} | rooftopChoice={rooftopChoice} | lives={finalLives}");
            PrintSaveSummary();
        }

        [ContextMenu("从存档启动 Level 5")]
        public void LoadAndStart()
        {
            EnsureComponents();

            if (saveSystem == null || !saveSystem.HasSave())
            {
                statusLog = "无存档，请先生成";
                Debug.LogWarning("[L5Save] 无存档可加载，请先按 F10 或点击'生成存档'");
                return;
            }

            var cp = saveSystem.LoadCheckpoint();
            if (cp.isInGameplay)
            {
                statusLog = "存档在游玩中，本工具只加载剧情入口";
                Debug.Log("[L5Save] 存档标记为游玩中，不自动加载剧情。如需剧情测试请重新生成存档。");
                return;
            }

            if (cp.currentLevelId == 5 && cp.storyFileName == "Dialogue5-1")
            {
                var textAsset = Resources.Load<TextAsset>("Dialogue/Dialogue5-1");
                if (textAsset == null)
                {
                    statusLog = "错误: 未找到 Dialogue5-1";
                    Debug.LogError("[L5Save] 未找到 Resources/Dialogue/Dialogue5-1");
                    return;
                }

                if (vnController != null)
                {
                    vnController.dialogueText = textAsset;
                    vnController.RestartDialogue();
                    statusLog = "已启动 Dialogue5-1";
                    Debug.Log("[L5Save] 已加载并启动 Dialogue5-1");
                }
                else
                {
                    statusLog = "错误: VN 控制器未找到";
                    Debug.LogError("[L5Save] FungusVNController 未找到，无法启动剧情");
                }
            }
            else
            {
                statusLog = $"存档关卡不匹配: Lv.{cp.currentLevelId}/{cp.storyFileName}";
                Debug.LogWarning($"[L5Save] 存档关卡不是 Level 5 剧情入口: {cp.storyFileName}");
            }
        }

        [ContextMenu("清除所有存档")]
        public void ClearAllSaves()
        {
            EnsureComponents();
            saveSystem?.ClearAll();
            PlayerDataStore.Instance?.ClearAllRecords();
            statusLog = "已清除所有存档";
            Debug.Log("[L5Save] 已清除所有存档");
        }

        JsonLevelRecord BuildLevel5Record()
        {
            var record = new JsonLevelRecord(5)
            {
                isWin = true,
                endingId = (int)endingBranch,
                endingBranch = (int)endingBranch == 6004 ? "rooftop" : "friend",
                timeUsed = 480f,
                cardUses = new List<CardUseEntry>(),
                taskGoals = new List<TaskGoalRecord>()
            };

            float t = 30f;

            // 关键卡 2017（第二关分享泡面）
            record.cardUses.Add(new CardUseEntry(2017, t, true));
            t += 60f;

            // 关键卡 2026（第五关寻求宋明月帮助）
            record.cardUses.Add(new CardUseEntry(2026, t, true));
            t += 60f;

            // 可选：Level 5 初始4张卡
            if (includeLevel5InitialCards)
            {
                int[] initialCards = { 2001, 2003, 2005, 2010 };
                foreach (var cid in initialCards)
                {
                    record.cardUses.Add(new CardUseEntry(cid, t, true));
                    t += 45f;
                }
            }

            // 任务目标：LevelConfig_5 中 targetCardId=2038，标记为已完成
            record.taskGoals.Add(new TaskGoalRecord(2038, 1, 1, "Completed"));

            return record;
        }

        void EnsureComponents()
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

            if (vnController == null)
            {
                vnController = FindObjectOfType<FungusVNController>();
            }

            if (PlayerDataStore.Instance == null)
            {
                var go = new GameObject("PlayerDataStore");
                go.AddComponent<PlayerDataStore>();
            }
        }

        void PrintSaveSummary()
        {
            if (saveSystem == null || saveSystem.currentSave == null)
            {
                Debug.Log("[L5Save] 存档摘要: 无数据");
                return;
            }

            var cp = saveSystem.currentSave.checkpoint;
            string timeStr = DateTimeOffset.FromUnixTimeMilliseconds(cp.saveTime).ToLocalTime().ToString("HH:mm:ss");

            Debug.Log("[L5Save] ====== 存档摘要 ======");
            Debug.Log($"[L5Save] 当前关卡: {cp.currentLevelId} | 剧情: {cp.storyFileName} | 时间: {timeStr}");

            var results = saveSystem.currentSave.levelResults;
            Debug.Log($"[L5Save] 关卡结果: {results.Count} 条");
            foreach (var r in results)
            {
                Debug.Log($"[L5Save]   [Lv.{r.levelId}] 血量={r.finalLives} | 2017={r.usedCard2017} | 2026={r.usedCard2026}");
            }

            var cards = saveSystem.currentSave.endingData.cardsUsed;
            Debug.Log($"[L5Save] 全局卡牌: {(cards.Count > 0 ? string.Join(", ", cards) : "无")}");

            int endingNone = saveSystem.EvaluateEnding(0);
            int endingAlone = saveSystem.EvaluateEnding(1);
            int endingFriend = saveSystem.EvaluateEnding(2);
            Debug.Log($"[L5Save] 结局判定 — 未选:{endingNone} | 独自:{endingAlone} | 邀请:{endingFriend}");

            var records = PlayerDataStore.Instance.GetAllRecords();
            Debug.Log($"[L5Save] 历史记录: {records.Count} 条");
            for (int i = 0; i < records.Count; i++)
            {
                var r = records[i];
                Debug.Log($"[L5Save]   [{i + 1}] Lv.{r.levelId} endingId={r.endingId} cards={r.cardUses?.Count}");
            }
            Debug.Log("[L5Save] ====================");
        }

        void OnGUI()
        {
            if (!Application.isEditor && !Debug.isDebugBuild) return;

            Rect r = new Rect(Screen.width - 260, 10, 250, 160);
            GUILayout.BeginArea(r, GUI.skin.box);
            GUILayout.Label("<b>L5 完美存档</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            GUILayout.Label($"状态: {statusLog}");
            GUILayout.Space(4);
            if (GUILayout.Button("F10 - 生成存档", GUILayout.Height(24))) GenerateSave();
            if (GUILayout.Button("F11 - 从存档启动", GUILayout.Height(24))) LoadAndStart();
            if (GUILayout.Button("Shift+F10 - 清除存档", GUILayout.Height(24))) ClearAllSaves();
            GUILayout.EndArea();
        }
    }
}
