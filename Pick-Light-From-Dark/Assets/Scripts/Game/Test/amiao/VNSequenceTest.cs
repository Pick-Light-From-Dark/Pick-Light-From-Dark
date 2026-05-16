using UnityEngine;
using Fungus;
using System.Collections.Generic;

namespace Game.Test
{
    /// <summary>
    /// VN 两段串联存档读档测试器 — Phase 2
    /// 串联 day1-1 和 day2-1，每段开头是检查点。
    /// 快捷键：F5=存档 | F9=读档 | F6=切换下一段 | F3=显隐面板
    /// </summary>
    public class VNSequenceTest : MonoBehaviour
    {
        [System.Serializable]
        public class VNSegment
        {
            public string segmentName;
            public TextAsset dialogueText;
            public int levelId;
        }

        [Header("VN 段落配置")]
        public List<VNSegment> segments = new List<VNSegment>
        {
            new VNSegment { segmentName = "day1-1", levelId = 1 },
            new VNSegment { segmentName = "day2-1", levelId = 2 }
        };

        [Header("VN 控制器（为空则自动查找）")]
        public FungusVNController vnController;

        [Header("存档系统")]
        public CrossLevelSaveSystem saveSystem;
        public VN_SaveFlowchartSetup saveSetup;

        [Header("调试显示")]
        public bool showGUI = true;

        int currentSegmentIndex = 0;
        string statusLog = "就绪";
        Rect uiRect = new Rect(10, 10, 460, 420);

        void Start()
        {
            EnsureComponents();
            LoadSegment(0);
            Debug.Log("[VNSequenceTest] Phase 2 测试器启动 | F5=存档 | F9=读档 | F6=下一段 | F3=显隐面板");
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
                NextSegment();
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
        }

        void OnGUI()
        {
            if (!showGUI) return;
            uiRect = GUILayout.Window(0, uiRect, DrawWindow, "Phase 2: 两段串联存档测试", GUILayout.Width(460));
        }

        void DrawWindow(int id)
        {
            GUILayout.Label("=== 状态 ===", GUI.skin.box);
            GUILayout.Label($"当前段: {GetCurrentSegmentName()} (索引 {currentSegmentIndex})");
            GUILayout.Label(statusLog);

            GUILayout.Space(5);
            GUILayout.Label("=== 段落切换 ===", GUI.skin.box);

            for (int i = 0; i < segments.Count; i++)
            {
                string btnLabel = $"加载段 {i}: {segments[i].segmentName}";
                if (i == currentSegmentIndex)
                    btnLabel += " (当前)";

                if (GUILayout.Button(btnLabel, GUILayout.Height(28)))
                {
                    LoadSegment(i);
                }
            }

            GUILayout.Space(5);
            GUILayout.Label("=== 存档操作 ===", GUI.skin.box);

            if (GUILayout.Button("F5 - 存档当前进度", GUILayout.Height(30)))
                SaveCheckpoint();

            if (GUILayout.Button("F9 - 读档回到段开头", GUILayout.Height(30)))
                LoadCheckpoint();

            if (GUILayout.Button("F6 - 切换到下一段", GUILayout.Height(24)))
                NextSegment();

            if (GUILayout.Button("清除所有存档", GUILayout.Height(24)))
                ClearAllSaves();

            GUILayout.Space(5);
            GUILayout.Label("=== 存档信息 ===", GUI.skin.box);

            if (saveSystem != null && saveSystem.HasSave())
            {
                var cp = saveSystem.currentSave.checkpoint;
                GUILayout.Label($"存档段: {cp.storyFileName}");
                GUILayout.Label($"关卡ID: {cp.currentLevelId}");
            }
            else
            {
                GUILayout.Label("暂无存档");
            }

            GUI.DragWindow();
        }

        /// <summary>加载指定段落</summary>
        void LoadSegment(int index)
        {
            if (index < 0 || index >= segments.Count)
            {
                Debug.LogWarning($"[VNSequenceTest] 无效段索引: {index}");
                return;
            }

            currentSegmentIndex = index;
            var segment = segments[index];

            if (vnController == null)
            {
                statusLog = "错误: vnController 未找到";
                return;
            }

            // 动态更换对话文本
            if (segment.dialogueText != null)
            {
                vnController.dialogueText = segment.dialogueText;
            }
            else
            {
                // 尝试从 Resources/Dialogue/ 加载
                var loaded = Resources.Load<TextAsset>($"Dialogue/{segment.segmentName}");
                if (loaded != null)
                    vnController.dialogueText = loaded;
            }

            // 重置并启动
            ResetVNToStart();
            statusLog = $"已加载段 {index}: {segment.segmentName}";
            Debug.Log($"[VNSequenceTest] 加载段 {index}: {segment.segmentName}");
        }

        /// <summary>切换到下一段</summary>
        [ContextMenu("切换到下一段")]
        public void NextSegment()
        {
            int next = currentSegmentIndex + 1;
            if (next >= segments.Count) next = 0;
            LoadSegment(next);
        }

        /// <summary>存档当前段进度</summary>
        [ContextMenu("存档当前进度")]
        public void SaveCheckpoint()
        {
            EnsureComponents();
            var segment = segments[currentSegmentIndex];

            // 1. Fungus 存档
            if (vnController != null && vnController.saveFlowchart != null)
            {
                vnController.SaveProgress();
            }

            // 2. CrossLevelSave 存档（保存段名作为 storyFileName）
            saveSystem.SaveStoryProgress(segment.levelId, segment.segmentName);

            statusLog = $"已存档: 段 {currentSegmentIndex} ({segment.segmentName})";
            Debug.Log($"[VNSequenceTest] 存档完成: {segment.segmentName}");
        }

        /// <summary>读档回到存档段开头</summary>
        [ContextMenu("读档回到段开头")]
        public void LoadCheckpoint()
        {
            EnsureComponents();

            if (!saveSystem.HasSave())
            {
                statusLog = "无存档可读取";
                Debug.Log("[VNSequenceTest] 无存档");
                return;
            }

            var cp = saveSystem.LoadCheckpoint();
            Debug.Log($"[VNSequenceTest] 读档: {cp.storyFileName}");

            // 根据存档的 storyFileName 找到对应段
            int targetIndex = -1;
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i].segmentName == cp.storyFileName ||
                    segments[i].levelId == cp.currentLevelId)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex >= 0)
            {
                LoadSegment(targetIndex);
                statusLog = $"已读档回到段 {targetIndex}: {cp.storyFileName}";
            }
            else
            {
                // 未找到匹配段，默认回到第一段
                LoadSegment(0);
                statusLog = $"存档段 {cp.storyFileName} 未配置，默认回到段 0";
            }
        }

        /// <summary>重置 VN 到当前段开头</summary>
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

        string GetCurrentSegmentName()
        {
            if (currentSegmentIndex >= 0 && currentSegmentIndex < segments.Count)
                return segments[currentSegmentIndex].segmentName;
            return "无";
        }

        [ContextMenu("清除所有存档")]
        public void ClearAllSaves()
        {
            saveSystem?.ClearAll();
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "FungusSaves", "vn_save.json");
            if (System.IO.File.Exists(savePath))
                System.IO.File.Delete(savePath);
            statusLog = "已清除所有存档";
            Debug.Log("[VNSequenceTest] 已清除所有存档");
        }
    }
}
