using UnityEngine;
using System.Collections.Generic;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// SL 场景存档读档测试主控器
    /// 在场景中自动初始化第一关 VN，提供键盘存档/读档/选关测试
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

        int currentLevelIndex = 0;
        string statusLog = "就绪";
        Rect uiRect = new Rect(10, 10, 460, 420);
        Vector2 scrollPos;

        void Start()
        {
            EnsureComponents();

            // 若 Inspector 未配置关卡，填充默认值
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

            if (autoStartLevel1 && levels.Count > 0)
            {
                LoadLevel(0);
            }

            Debug.Log("[SLTestRunner] 存档读档测试启动 | F5=存档 | F9=读档 | F1~F5=选关 | F3=显隐面板");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                showGUI = !showGUI;

            if (Input.GetKeyDown(KeyCode.F5))
                SaveCheckpoint();

            if (Input.GetKeyDown(KeyCode.F9))
                LoadCheckpoint();

            // 数字键 1~5 选关
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
            uiRect = GUILayout.Window(0, uiRect, DrawWindow, "SL 存档读档测试", GUILayout.Width(460));
        }

        void DrawWindow(int id)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(380));

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

            if (GUILayout.Button("清除所有存档", GUILayout.Height(24)))
                ClearAllSaves();

            GUILayout.Space(5);
            GUILayout.Label("=== 存档信息 ===", GUI.skin.box);
            if (saveSystem != null && saveSystem.HasSave())
            {
                var cp = saveSystem.currentSave.checkpoint;
                GUILayout.Label($"关卡: {cp.currentLevelId}");
                GUILayout.Label($"剧情: {cp.storyFileName}");
                GUILayout.Label($"模式: {(cp.isInGameplay ? "游玩中" : "剧情中")}");
                GUILayout.Label($"时间: {System.DateTimeOffset.FromUnixTimeMilliseconds(cp.saveTime):MM-dd HH:mm}");
            }
            else
            {
                GUILayout.Label("暂无存档");
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        /// <summary>加载指定关卡剧情</summary>
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

            statusLog = $"已加载: {level.displayName} ({level.storyFile})";
            Debug.Log($"[SLTestRunner] 加载关卡 {level.levelId}: {level.storyFile}");
        }

        /// <summary>存档当前进度</summary>
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

            // 1. CrossLevelSaveSystem 存档
            saveSystem.SaveStoryProgress(level.levelId, level.storyFile, 0);

            // 2. 同时保存到 PlayerDataStore 作为关卡记录
            var record = new JsonLevelRecord(level.levelId);
            record.isWin = false;
            record.timeUsed = Time.time;
            record.endingBranch = level.storyFile;
            PlayerDataStore.Instance.SaveLevelRecord(record);

            statusLog = $"已存档: {level.displayName}";
            Debug.Log($"[SLTestRunner] 存档完成: Level={level.levelId}, Story={level.storyFile}");
        }

        /// <summary>读档：回到当前存档关卡的剧情开始</summary>
        [ContextMenu("读档回到剧情开始")]
        public void LoadCheckpoint()
        {
            EnsureComponents();

            if (!saveSystem.HasSave())
            {
                statusLog = "无存档可读取";
                Debug.Log("[SLTestRunner] 无存档");
                return;
            }

            var cp = saveSystem.LoadCheckpoint();
            Debug.Log($"[SLTestRunner] 读档: Level={cp.currentLevelId}, Story={cp.storyFileName}");

            // 根据存档的 storyFileName 找到对应关卡索引
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
                statusLog = $"已读档: {levels[targetIndex].displayName} (剧情开始)";
            }
            else
            {
                // 未找到匹配关卡，默认回到第一关
                LoadLevel(0);
                statusLog = $"存档关卡 {cp.storyFileName} 未配置，默认回到第一关";
            }
        }

        [ContextMenu("清除所有存档")]
        public void ClearAllSaves()
        {
            saveSystem?.ClearAll();
            PlayerDataStore.Instance?.ClearAllRecords();
            statusLog = "已清除所有存档";
            Debug.Log("[SLTestRunner] 已清除所有存档");
        }
    }
}
