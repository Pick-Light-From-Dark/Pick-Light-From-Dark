using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 结局分支系统测试器 — 运行时 IMGUI 界面，可手动配置游玩记录并测试结局判定
    /// </summary>
    public class EndingBranchSystemTester : MonoBehaviour
    {
        [Header("目标结局分支系统")]
        public EndingBranchSystem branchSystem;

        [Header("测试数据")]
        public int testLevelId = 5;
        public string testBranchChoice = "";
        public int testLives = 3;
        public int testEmotion = 50;
        public string testItem = "";

        private GameplayRecord testRecord;
        private int lastResult = -1;
        private Rect windowRect = new Rect(10, 10, 320, 400);
        private Vector2 scrollPos;

        void Start()
        {
            EnsureBranchSystem();
        }

        void EnsureBranchSystem()
        {
            if (branchSystem == null)
            {
                branchSystem = FindObjectOfType<EndingBranchSystem>();
                if (branchSystem == null)
                {
                    var go = new GameObject("EndingBranchSystem");
                    branchSystem = go.AddComponent<EndingBranchSystem>();
                }
            }
        }

        void OnGUI()
        {
            windowRect = GUILayout.Window(0, windowRect, DrawWindow, "结局分支系统测试", GUILayout.Width(320));
        }

        void DrawWindow(int id)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(340));

            GUILayout.Label("游玩记录配置", GUI.skin.box);
            GUILayout.Label($"关卡: {testLevelId}");
            testLevelId = Mathf.RoundToInt(GUILayout.HorizontalSlider(testLevelId, 1, 5));

            GUILayout.Label($"分支: {testBranchChoice}");
            testBranchChoice = GUILayout.TextField(testBranchChoice);

            GUILayout.Label($"生命: {testLives}");
            testLives = Mathf.RoundToInt(GUILayout.HorizontalSlider(testLives, 0, 5));

            GUILayout.Label($"情绪: {testEmotion}");
            testEmotion = Mathf.RoundToInt(GUILayout.HorizontalSlider(testEmotion, 0, 100));

            GUILayout.Label("道具:");
            testItem = GUILayout.TextField(testItem);

            GUILayout.Space(10);
            if (GUILayout.Button("执行判定", GUILayout.Height(40)))
            {
                RunEvaluation();
            }

            if (lastResult > 0)
            {
                GUILayout.Space(5);
                GUILayout.Label($"判定结果: 结局 {lastResult}", GUI.skin.box);
            }

            GUILayout.Space(10);
            GUILayout.Label("快捷结局测试", GUI.skin.box);
            if (GUILayout.Button("结局一 (不吃分支)")) { testBranchChoice = "not_eat"; testLevelId = 1; RunEvaluation(); }
            if (GUILayout.Button("结局二 (全1血)")) { testBranchChoice = ""; testLevelId = 5; testLives = 1; RunEvaluation(); }
            if (GUILayout.Button("结局三 (仅2017卡)")) { testBranchChoice = ""; testLevelId = 5; testLives = 3; RunEvaluation(); }
            if (GUILayout.Button("结局四 (2017+2026两卡)")) { testBranchChoice = ""; testLevelId = 5; testLives = 3; RunEvaluation(); }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        void RunEvaluation()
        {
            EnsureBranchSystem();
            testRecord = new GameplayRecord
            {
                levelId = testLevelId,
                finalLives = testLives,
                finalEmotion = testEmotion
            };
            testRecord.AddBranchChoice(testBranchChoice);
            if (!string.IsNullOrEmpty(testItem))
                testRecord.AddCollectedItem(testItem);

            lastResult = branchSystem.EvaluateEnding(testRecord);
            Debug.Log($"[EndingBranchSystemTester] 判定结果: {lastResult} (分支={testBranchChoice} 生命={testLives} 情绪={testEmotion})");
        }

        [ContextMenu("测试：结局四")]
        void TestEnding4()
        {
            testBranchChoice = "";
            testLevelId = 5;
            testLives = 3;
            testItem = "";
            RunEvaluation();
        }
    }
}
