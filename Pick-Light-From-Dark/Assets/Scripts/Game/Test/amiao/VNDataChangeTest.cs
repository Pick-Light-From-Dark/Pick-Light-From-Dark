using UnityEngine;
using Fungus;
using System;
using System.Collections;

namespace Game.Test
{
    /// <summary>
    /// VN PlayerData 变化测试器 — Phase 3
    /// 在 VN 段开头改变 player data，存档读档时在控制台显示数据变化。
    /// 快捷键：F5=存档 | F9=读档 | F7=修改数据 | F3=显隐面板
    /// </summary>
    public class VNDataChangeTest : MonoBehaviour
    {
        [Header("VN 控制器")]
        public FungusVNController vnController;

        [Header("数据管理器")]
        public TestPlayerDataManager dataManager;

        [Header("存档系统")]
        public CrossLevelSaveSystem saveSystem;
        public VN_SaveFlowchartSetup saveSetup;

        [Header("数据变化配置")]
        public int day1LivesChange = -1;
        public int day1EmotionChange = 10;
        public int day2LivesChange = -2;
        public int day2EmotionChange = 20;

        [Header("调试显示")]
        public bool showGUI = true;

        int currentSegmentIndex = 0;
        string[] segmentNames = new[] { "day1-1", "day2-1" };
        int[] segmentLevels = new[] { 1, 2 };
        Rect uiRect = new Rect(10, 10, 480, 460);
        string statusLog = "就绪";
        string dataChangeLog = "";

        void Start()
        {
            EnsureComponents();
            Debug.Log("[VNDataChangeTest] Phase 3 测试器启动 | F5=存档 | F9=读档 | F7=修改数据 | F3=显隐面板");
            statusLog = "就绪 - 当前段: day1-1";
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                showGUI = !showGUI;

            if (Input.GetKeyDown(KeyCode.F5))
                SaveCheckpoint();

            if (Input.GetKeyDown(KeyCode.F9))
                LoadCheckpoint();

            if (Input.GetKeyDown(KeyCode.F7))
                ApplyDataChange();

            if (Input.GetKeyDown(KeyCode.F6))
                SwitchSegment();
        }

        void EnsureComponents()
        {
            if (vnController == null)
                vnController = FindObjectOfType<FungusVNController>();

            if (dataManager == null)
            {
                dataManager = FindObjectOfType<TestPlayerDataManager>();
                if (dataManager == null)
                {
                    var go = new GameObject("TestPlayerDataManager");
                    dataManager = go.AddComponent<TestPlayerDataManager>();
                }
            }

            if (saveSystem == null)
            {
                saveSystem = FindObjectOfType<CrossLevelSaveSystem>();
                if (saveSystem == null)
                {
                    var go = new GameObject("CrossLevelSaveSystem");
                    saveSystem = go.AddComponent<CrossLevelSaveSystem>();
                }
            }

            if (saveSetup == null)
            {
                saveSetup = FindObjectOfType<VN_SaveFlowchartSetup>();
                if (saveSetup == null)
                {
                    var go = new GameObject("VN_SaveFlowchartSetup");
                    saveSetup = go.AddComponent<VN_SaveFlowchartSetup>();
                }
            }
        }

        void OnGUI()
        {
            if (!showGUI) return;
            uiRect = GUILayout.Window(0, uiRect, DrawWindow, "Phase 3: PlayerData JSON 存档变化测试", GUILayout.Width(480));
        }

        void DrawWindow(int id)
        {
            GUILayout.Label("=== 状态 ===", GUI.skin.box);
            GUILayout.Label($"当前段: {segmentNames[currentSegmentIndex]} (索引 {currentSegmentIndex})");
            GUILayout.Label(statusLog);

            GUILayout.Space(5);
            GUILayout.Label("=== 当前 PlayerData ===", GUI.skin.box);

            if (dataManager != null && dataManager.currentData != null)
            {
                var d = dataManager.currentData;
                GUILayout.Label($"Name: {d.playerName} | ID: {d.playerId}");
                GUILayout.Label($"Lives: {d.lives} | Emotion: {d.emotion}");
                GUILayout.Label($"Level: {d.currentLevel} | Segment: {d.currentSegment}");
                GUILayout.Label($"HasSeenOpening: {d.hasSeenOpening}");
            }
            else
            {
                GUILayout.Label("数据管理器未初始化");
            }

            GUILayout.Space(5);
            GUILayout.Label("=== 数据操作 ===", GUI.skin.box);

            if (GUILayout.Button("F7 - 应用当前段数据变化", GUILayout.Height(30)))
                ApplyDataChange();

            if (GUILayout.Button("F6 - 切换段 (day1-1 / day2-1)", GUILayout.Height(24)))
                SwitchSegment();

            GUILayout.Space(5);
            GUILayout.Label("=== 存档操作 ===", GUI.skin.box);

            if (GUILayout.Button("F5 - 存档 (含 PlayerData)", GUILayout.Height(30)))
                SaveCheckpoint();

            if (GUILayout.Button("F9 - 读档 (恢复 PlayerData)", GUILayout.Height(30)))
                LoadCheckpoint();

            if (GUILayout.Button("显示数据到控制台", GUILayout.Height(24)))
                DisplayDataToConsole();

            if (GUILayout.Button("清除所有存档", GUILayout.Height(24)))
                ClearAll();

            GUILayout.Space(3);
            GUILayout.Label("=== 变化日志 ===", GUI.skin.box);
            GUILayout.Label(dataChangeLog, GUI.skin.label);

            GUI.DragWindow();
        }

        /// <summary>应用当前段的数据变化</summary>
        [ContextMenu("应用数据变化")]
        public void ApplyDataChange()
        {
            EnsureComponents();
            if (dataManager == null) return;

            var before = JsonUtility.ToJson(dataManager.currentData);

            if (currentSegmentIndex == 0)
            {
                // day1-1: 生命-1, 情绪+10
                dataManager.currentData.lives += day1LivesChange;
                dataManager.currentData.emotion += day1EmotionChange;
                dataManager.currentData.currentLevel = 1;
                dataManager.currentData.currentSegment = "day1-1";
                dataManager.currentData.hasSeenOpening = true;
                dataChangeLog = $"[day1-1] Lives {day1LivesChange:+#;-#;0}, Emotion {day1EmotionChange:+#;-#;0}";
            }
            else
            {
                // day2-1: 生命-2, 情绪+20
                dataManager.currentData.lives += day2LivesChange;
                dataManager.currentData.emotion += day2EmotionChange;
                dataManager.currentData.currentLevel = 2;
                dataManager.currentData.currentSegment = "day2-1";
                dataChangeLog = $"[day2-1] Lives {day2LivesChange:+#;-#;0}, Emotion {day2EmotionChange:+#;-#;0}";
            }

            // 限制范围
            dataManager.currentData.lives = Mathf.Clamp(dataManager.currentData.lives, 0, 5);
            dataManager.currentData.emotion = Mathf.Clamp(dataManager.currentData.emotion, 0, 100);

            dataManager.SaveData();

            var after = JsonUtility.ToJson(dataManager.currentData);
            statusLog = $"已应用变化: {dataChangeLog}";
            Debug.Log($"[VNDataChangeTest] 数据变化:\nBEFORE: {before}\nAFTER: {after}");
        }

        /// <summary>切换段</summary>
        [ContextMenu("切换段")]
        public void SwitchSegment()
        {
            currentSegmentIndex = (currentSegmentIndex + 1) % segmentNames.Length;
            string segName = segmentNames[currentSegmentIndex];

            if (vnController != null)
            {
                var loaded = Resources.Load<TextAsset>($"Dialogue/{segName}");
                if (loaded != null)
                    vnController.dialogueText = loaded;
                ResetVNToStart();
            }

            statusLog = $"已切换到段 {currentSegmentIndex}: {segName}";
            Debug.Log($"[VNDataChangeTest] 切换段: {segName}");
        }

        /// <summary>存档：保存 VN 进度 + PlayerData</summary>
        [ContextMenu("存档")]
        public void SaveCheckpoint()
        {
            EnsureComponents();
            string segName = segmentNames[currentSegmentIndex];

            // 1. 保存 PlayerData JSON
            if (dataManager != null)
                dataManager.SaveData();

            // 2. Fungus 存档
            if (vnController != null && vnController.saveFlowchart != null)
                vnController.SaveProgress();

            // 3. CrossLevelSave 存档
            saveSystem.SaveStoryProgress(segmentLevels[currentSegmentIndex], segName, 0);

            statusLog = $"已存档: {segName} + PlayerData";
            Debug.Log($"[VNDataChangeTest] 存档完成: {segName}");
        }

        /// <summary>读档：恢复 VN 进度 + PlayerData</summary>
        [ContextMenu("读档")]
        public void LoadCheckpoint()
        {
            EnsureComponents();

            if (!saveSystem.HasSave())
            {
                statusLog = "无存档可读取";
                Debug.Log("[VNDataChangeTest] 无存档");
                return;
            }

            var before = dataManager != null ? JsonUtility.ToJson(dataManager.currentData) : "null";

            // 1. 恢复 PlayerData
            if (dataManager != null)
                dataManager.LoadData();

            // 2. 恢复 VN
            var cp = saveSystem.LoadCheckpoint();
            int targetIdx = 0;
            for (int i = 0; i < segmentNames.Length; i++)
            {
                if (segmentNames[i] == cp.storyFileName || segmentLevels[i] == cp.currentLevelId)
                {
                    targetIdx = i;
                    break;
                }
            }

            currentSegmentIndex = targetIdx;
            if (vnController != null)
            {
                var loaded = Resources.Load<TextAsset>($"Dialogue/{segmentNames[targetIdx]}");
                if (loaded != null)
                    vnController.dialogueText = loaded;
                ResetVNToStart();
            }

            var after = dataManager != null ? JsonUtility.ToJson(dataManager.currentData) : "null";
            statusLog = $"已读档: {cp.storyFileName} + PlayerData 已恢复";
            Debug.Log($"[VNDataChangeTest] 读档完成:\nPlayerData BEFORE: {before}\nPlayerData AFTER: {after}");
        }

        /// <summary>在控制台显示当前数据</summary>
        [ContextMenu("显示数据到控制台")]
        public void DisplayDataToConsole()
        {
            if (dataManager != null)
                dataManager.DisplayData();
            else
                Debug.LogWarning("[VNDataChangeTest] dataManager 未找到");
        }

        void ResetVNToStart()
        {
            if (vnController == null) return;
            if (vnController.saveFlowchart != null)
            {
                vnController.saveFlowchart.SetIntegerVariable("VN_LineIndex", 0);
                vnController.saveFlowchart.SetBooleanVariable("VN_IsOpeningDone", false);
            }
            vnController.RestartDialogue();
        }

        [ContextMenu("清除所有存档")]
        public void ClearAll()
        {
            saveSystem?.ClearAll();
            dataManager?.ClearData();
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "FungusSaves", "vn_save.json");
            if (System.IO.File.Exists(savePath))
                System.IO.File.Delete(savePath);
            statusLog = "已清除所有存档";
            Debug.Log("[VNDataChangeTest] 已清除所有存档");
        }
    }
}
