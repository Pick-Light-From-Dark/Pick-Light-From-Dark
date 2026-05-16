using UnityEngine;
using Fungus;
using Game.Backend;

namespace Game.Test
{
    /// <summary>
    /// VN 单段存档读档测试器 — Phase 1
    /// 挂载到场景空物体，自动查找 VN 控制器。
    /// 快捷键：F5=存档 | F9=读档回到开头 | F3=显隐面板
    /// </summary>
    public class VNCheckpointTest : MonoBehaviour
    {
        [Header("VN 控制器（为空则自动查找）")]
        public FungusVNController vnController;

        [Header("存档系统（为空则自动查找/创建）")]
        public CrossLevelSaveSystem saveSystem;
        public VN_SaveFlowchartSetup saveSetup;

        [Header("测试配置")]
        public int testLevelId = 1;
        public string testStoryFile = "day1-1";

        [Header("调试显示")]
        public bool showGUI = true;

        Rect uiRect = new Rect(10, 10, 420, 360);
        Vector2 scrollPos;
        string statusLog = "就绪";

        void Start()
        {
            EnsureComponents();
            Debug.Log("[VNCheckpointTest] Phase 1 测试器启动 | F5=存档 | F9=读档 | F3=显隐面板");
            statusLog = "就绪 - 等待存档操作";
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                showGUI = !showGUI;

            if (Input.GetKeyDown(KeyCode.F5))
                SaveCheckpoint();

            if (Input.GetKeyDown(KeyCode.F9))
                LoadCheckpoint();
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

            if (saveSetup == null)
            {
                saveSetup = FindObjectOfType<VN_SaveFlowchartSetup>();
                if (saveSetup == null)
                {
                    var go = new GameObject("VN_SaveFlowchartSetup");
                    saveSetup = go.AddComponent<VN_SaveFlowchartSetup>();
                }
            }

            // 确保 PlayerDataStore 存在（用于关卡记录）
            if (PlayerDataStore.Instance == null)
            {
                var go = new GameObject("PlayerDataStore");
                go.AddComponent<PlayerDataStore>();
            }
        }

        void OnGUI()
        {
            if (!showGUI) return;
            uiRect = GUILayout.Window(0, uiRect, DrawWindow, "Phase 1: 单段存档测试", GUILayout.Width(420));
        }

        void DrawWindow(int id)
        {
            GUILayout.Label($"=== 状态 ===", GUI.skin.box);
            GUILayout.Label(statusLog, GUI.skin.label);

            GUILayout.Space(5);
            GUILayout.Label("=== 操作 ===", GUI.skin.box);

            if (GUILayout.Button("F5 - 存档当前进度", GUILayout.Height(32)))
                SaveCheckpoint();

            if (GUILayout.Button("F9 - 读档回到开头", GUILayout.Height(32)))
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
                GUILayout.Label($"模式: {(string.IsNullOrEmpty(cp.storyFileName) ? "游玩" : "剧情")}");
            }
            else
            {
                GUILayout.Label("暂无存档");
            }

            GUILayout.Space(3);
            if (vnController != null && vnController.dialogueText != null)
            {
                GUILayout.Label($"当前 VN: {vnController.dialogueText.name}");
            }
            else
            {
                GUILayout.Label("VN 控制器未找到");
            }

            GUI.DragWindow();
        }

        /// <summary>存档当前 VN 进度</summary>
        [ContextMenu("测试：存档当前进度")]
        public void SaveCheckpoint()
        {
            EnsureComponents();

            if (vnController == null)
            {
                statusLog = "错误：vnController 未找到";
                Debug.LogWarning("[VNCheckpointTest] vnController 未找到");
                return;
            }

            // 1. 使用 Fungus SaveManager 存档
            if (vnController.saveFlowchart != null)
            {
                vnController.SaveProgress();
            }

            // 2. 使用 CrossLevelSaveSystem 存档检查点
            string storyFile = vnController.dialogueText != null ? vnController.dialogueText.name : testStoryFile;
            saveSystem.SaveStoryProgress(testLevelId, storyFile);

            statusLog = $"已存档: Level={testLevelId}, Story={storyFile}";
            Debug.Log($"[VNCheckpointTest] 存档完成: {storyFile}");
        }

        /// <summary>读档回到当前 VN 开头</summary>
        [ContextMenu("测试：读档回到开头")]
        public void LoadCheckpoint()
        {
            EnsureComponents();

            if (!saveSystem.HasSave())
            {
                statusLog = "无存档可读取";
                Debug.Log("[VNCheckpointTest] 无存档可读取");
                return;
            }

            var cp = saveSystem.LoadCheckpoint();
            Debug.Log($"[VNCheckpointTest] 读档: Level={cp.currentLevelId}, Story={cp.storyFileName}");

            // 验证 VN 控制器存在
            if (vnController == null)
            {
                statusLog = "错误：vnController 未找到";
                return;
            }

            // 方式1：尝试使用 Fungus SaveManager 读档
            var saveManager = FungusManager.Instance.SaveManager;
            if (saveManager.SaveDataExists("vn_save"))
            {
                // Fungus Load 会重新加载场景，对于单段测试我们改用轻量方式
                // 重置 VN 到开头
                ResetVNToStart();
                statusLog = $"已读档回到 {cp.storyFileName} 开头 (Fungus 存档有效)";
            }
            else
            {
                // 无 Fungus 存档，直接用 CrossLevelSave 的检查点信息重置
                ResetVNToStart();
                statusLog = $"已读档回到 {cp.storyFileName} 开头 (CrossLevel 检查点)";
            }

            Debug.Log($"[VNCheckpointTest] 读档完成，回到 {cp.storyFileName} 开头");
        }

        /// <summary>重置 VN 到开头（轻量级，不重新加载场景）</summary>
        void ResetVNToStart()
        {
            if (vnController == null) return;

            // 重置 Flowchart 变量
            if (vnController.saveFlowchart != null)
            {
                vnController.saveFlowchart.SetIntegerVariable("VN_LineIndex", 0);
                vnController.saveFlowchart.SetBooleanVariable("VN_IsOpeningDone", false);
            }

            // 重新启动对话
            vnController.RestartDialogue();
        }

        /// <summary>清除所有存档</summary>
        [ContextMenu("清除所有存档")]
        public void ClearAllSaves()
        {
            saveSystem?.ClearAll();

            // 同时清除 Fungus 存档
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "FungusSaves", "vn_save.json");
            if (System.IO.File.Exists(savePath))
            {
                System.IO.File.Delete(savePath);
            }

            statusLog = "已清除所有存档";
            Debug.Log("[VNCheckpointTest] 已清除所有存档");
        }
    }
}
