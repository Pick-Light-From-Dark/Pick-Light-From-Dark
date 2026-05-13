using UnityEngine;
using Game.Config;
using Game.Flow;

namespace Game.Test
{
    /// <summary>
    /// 剧情分支结局测试器
    /// 挂载到场景后，运行时左上角显示菜单栏，可手动选择分支条件并触发对应结局
    /// </summary>
    public class EndingBranchTestRunner : MonoBehaviour
    {
        [Header("VN 控制器（为空则自动查找）")]
        public FungusVNController vnController;

        [Header("结局数据配置")]
        public EndingDataSO endingData;

        [Header("自动初始化")]
        public bool autoInitOnStart = true;

        [Header("测试对话文本")]
        public TextAsset dialogueText;

        // 菜单窗口状态
        private bool showMenu = true;
        private Rect menuRect = new Rect(10, 10, 220, 320);
        private Vector2 scrollPos;

        // 分支结局映射
        private readonly string[] branchNames = { "迷茫结局 (6002)", "网吧结局 (6003)", "天台结局 (6004)", "邀友同行 (6005)" };
        private readonly int[] endingIds = { 6002, 6003, 6004, 6005 };

        void Start()
        {
            if (autoInitOnStart)
            {
                InitializeTest();
            }
        }

        void InitializeTest()
        {
            // 确保 EndingManager 存在
            if (EndingManager.Instance == null)
            {
                var go = new GameObject("EndingManager");
                go.AddComponent<EndingManager>();
            }

            // 确保结局数据已配置
            if (endingData == null)
            {
                endingData = CreateDefaultEndingData();
            }
            EndingManager.Instance.Initialize(endingData);

            // 查找或创建 VN 控制器
            if (vnController == null)
            {
                vnController = FindObjectOfType<FungusVNController>();
                if (vnController == null)
                {
                    var go = new GameObject("FungusVNController");
                    vnController = go.AddComponent<FungusVNController>();
                }
            }

            // 配置对话文本
            if (dialogueText == null)
            {
                dialogueText = Resources.Load<TextAsset>("Dialogue/Dialogue5");
            }
            if (dialogueText != null)
            {
                vnController.dialogueText = dialogueText;
            }

            Debug.Log("[EndingBranchTestRunner] 初始化完成。按 F2 切换菜单显隐。");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                showMenu = !showMenu;
            }
        }

        void OnGUI()
        {
            if (!showMenu) return;

            menuRect = GUILayout.Window(0, menuRect, DrawMenuWindow, "结局分支测试", GUILayout.Width(220));
        }

        void DrawMenuWindow(int windowID)
        {
            GUILayout.Label("分支选择", GUI.skin.box);

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(160));

            for (int i = 0; i < branchNames.Length; i++)
            {
                if (GUILayout.Button(branchNames[i], GUILayout.Height(30)))
                {
                    TriggerEndingByBranch(i);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Space(5);
            GUILayout.Label("快捷操作", GUI.skin.box);

            if (GUILayout.Button("从头播放 VN", GUILayout.Height(28)))
            {
                RestartVN();
            }

            if (GUILayout.Button("快进模式", GUILayout.Height(28)))
            {
                ToggleFastForward();
            }

            GUILayout.Space(5);
            GUILayout.Label("按键: F2=显隐菜单", GUI.skin.label);

            GUI.DragWindow();
        }

        void TriggerEndingByBranch(int index)
        {
            int id = endingIds[index];
            Debug.Log($"[EndingBranchTestRunner] 触发分支: {branchNames[index]}");
            EndingManager.Instance.TriggerEnding(id);
        }

        void RestartVN()
        {
            if (vnController != null)
            {
                // 通过反射调用私有方法 StartDialogue
                var method = typeof(FungusVNController).GetMethod("StartDialogue",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(vnController, null);
                    Debug.Log("[EndingBranchTestRunner] VN 已从头播放");
                }
                else
                {
                    Debug.LogWarning("[EndingBranchTestRunner] 未找到 StartDialogue 方法");
                }
            }
        }

        void ToggleFastForward()
        {
            if (vnController != null)
            {
                var field = typeof(FungusVNController).GetField("isFastForwarding",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    bool current = (bool)field.GetValue(vnController);
                    field.SetValue(vnController, !current);
                    Debug.Log($"[EndingBranchTestRunner] 快进模式: {!current}");
                }
            }
        }

        /// <summary>
        /// 创建默认结局数据（内置5个结局）
        /// </summary>
        EndingDataSO CreateDefaultEndingData()
        {
            var so = ScriptableObject.CreateInstance<EndingDataSO>();
            so.endings = new System.Collections.Generic.List<EndingEntry>
            {
                new EndingEntry { id = 6001, endingName = "【结局一：太阳照常升起】", description = "薯片改变不了任何事，你也是。" },
                new EndingEntry { id = 6002, endingName = "【结局二：莫比乌斯环】", description = "一条走廊，离开起点之时，你就明白你终会回来。" },
                new EndingEntry { id = 6003, endingName = "【结局三：人心不足蛇吞象】", description = "得失荣枯总在天，机关用尽也徒然。" },
                new EndingEntry { id = 6004, endingName = "【结局四：星垂之夜】", description = "俯仰天地之间——无愧于人，无愧于心，无愧于己。" },
                new EndingEntry { id = 6005, endingName = "【结局五：北极星】", description = "我对你透露一个大秘密，这是人类最古老的玩笑——无论往哪走，都是向前走。" }
            };
            return so;
        }

        [ContextMenu("触发结局二")]
        void TestEnding2() { EndingManager.Instance?.TriggerEnding(6002); }

        [ContextMenu("触发结局三")]
        void TestEnding3() { EndingManager.Instance?.TriggerEnding(6003); }

        [ContextMenu("触发结局四")]
        void TestEnding4() { EndingManager.Instance?.TriggerEnding(6004); }

        [ContextMenu("触发结局五")]
        void TestEnding5() { EndingManager.Instance?.TriggerEnding(6005); }
    }
}
