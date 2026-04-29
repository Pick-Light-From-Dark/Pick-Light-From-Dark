using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using Game.Data;
using Game.Config;

namespace Game.Editor
{
    /// <summary>
    /// 一键测试工具 - 自动准备所有测试环境
    /// </summary>
    public class OneClickTestSetup
    {
        [MenuItem("Tools/《灯下黑》/🚀 一键测试 (自动准备所有环境)")]
        public static void SetupAndRunTest()
        {
            Debug.Log("=== 🚀 开始一键测试环境准备 ===");

            // 步骤1: 创建测试数据
            Debug.Log("步骤1/4: 创建测试数据...");
            CreateTestData();

            // 步骤2: 创建测试场景
            Debug.Log("步骤2/4: 创建综合测试场景...");
            Scene testScene = CreateIntegrationTestScene();

            // 步骤3: 配置测试场景
            Debug.Log("步骤3/4: 配置测试场景...");
            ConfigureTestScene(testScene);

            // 步骤4: 保存并提示
            Debug.Log("步骤4/4: 保存场景...");
            EditorSceneManager.SaveScene(testScene, "Assets/Scenes/OneClickTest.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== ✅ 测试环境准备完成！===");
            Debug.Log("📝 测试说明:");
            Debug.Log("   1. 点击 Unity 的 Play 按钮开始测试");
            Debug.Log("   2. 观察 Console 的日志输出");
            Debug.Log("   3. 查看屏幕左上角的 GUI 状态显示");
            Debug.Log("   4. 等待 5 秒自动测试卡牌读条");
            Debug.Log("   5. 等待 10 秒自动测试打断功能");
            Debug.Log("   6. 观察老师 AI 的状态切换");
            Debug.Log("");
            Debug.Log("⏹️  测试结束后点击 Stop 停止");

            // 显示完成对话框
            EditorUtility.DisplayDialog(
                "测试环境准备完成 ✅",
                "所有测试环境和数据已自动创建完成！\n\n" +
                "现在可以直接点击 Play 按钮开始测试。\n\n" +
                "测试内容：\n" +
                "• 游戏流程控制\n" +
                "• 老师 AI 巡逻\n" +
                "• 卡牌读条系统\n" +
                "• 情绪值系统\n" +
                "• 事件系统",
                "好的，开始测试"
            );
        }

        static void CreateTestData()
        {
            string testDataPath = "Assets/Resources/TestData";

            // 创建文件夹
            if (!AssetDatabase.IsValidFolder(testDataPath))
            {
                string parent = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder(parent, "TestData");
            }

            // 创建关卡配置
            LevelConfigSO levelConfig = ScriptableObject.CreateInstance<LevelConfigSO>();
            levelConfig.levelId = 1;
            levelConfig.levelName = "一键测试关卡";
            levelConfig.timeLimit = 60; // 60秒测试关卡
            levelConfig.initialPanic = 35;
            levelConfig.initialExcite = 35;
            levelConfig.criticalValue = 100;
            levelConfig.patrolIntervals = new Vector2(3f, 5f); // 更频繁的巡逻
            levelConfig.patrolTime = new Vector2(2f, 3f);
            levelConfig.eyeCheckDuration = new Vector2(1.5f, 2.5f);
            levelConfig.flashCheckDuration = new Vector2(2f, 3f);
            levelConfig.flashPanicPerSec = 10;
            levelConfig.initialCards = new List<int> { 1, 2, 3 };
            levelConfig.maxLives = 2;

            AssetDatabase.CreateAsset(levelConfig, $"{testDataPath}/TestLevelConfig.asset");

            // 创建测试卡牌
            CardData cardData = new CardData
            {
                id = 1,
                cardName = "翻身",
                description = "在床上翻个身，降低少量兴奋值",
                iconPath = "Icons/card_turnover",
                segments = new List<Segment>
                {
                    new Segment(2f, true),   // 可打断
                    new Segment(2f, false)  // 不可打断
                },
                panicDelta = 0,
                exciteDelta = -5,
                interruptPanicAdd = 10
            };

            CardDataContainer cardContainer = ScriptableObject.CreateInstance<CardDataContainer>();
            cardContainer.cardData = cardData;

            AssetDatabase.CreateAsset(cardContainer, $"{testDataPath}/TestCardData.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"  ✓ 测试数据已创建到: {testDataPath}");
        }

        static Scene CreateIntegrationTestScene()
        {
            // 创建新场景
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 设置场景背景色
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // 深色背景

            // 创建灯光
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.8f);

            // 创建系统单例对象
            GameObject flowObj = new GameObject("GameFlowController");
            flowObj.AddComponent<Game.Flow.GameFlowController>();

            GameObject emotionObj = new GameObject("EmotionSystem");
            emotionObj.AddComponent<Game.Emotion.EmotionSystem>();

            // 创建综合测试脚本
            GameObject testObj = new GameObject("OneClickTest");
            Game.System.GameIntegrationTest test = testObj.AddComponent<Game.System.GameIntegrationTest>();
            test.autoStart = true;

            return newScene;
        }

        static void ConfigureTestScene(Scene scene)
        {
            // 查找测试对象并配置
            GameObject[] rootObjects = scene.GetRootGameObjects();
            Game.System.GameIntegrationTest test = null;

            foreach (var obj in rootObjects)
            {
                if (obj.name == "OneClickTest")
                {
                    test = obj.GetComponent<Game.System.GameIntegrationTest>();
                    break;
                }
            }

            if (test != null)
            {
                // 自动加载测试数据
                var testLevelConfig = AssetDatabase.LoadAssetAtPath<LevelConfigSO>("Assets/Resources/TestData/TestLevelConfig.asset");

                if (testLevelConfig != null)
                {
                    var serializedObj = new SerializedObject(test);
                    var prop = serializedObj.FindProperty("testLevelConfig");
                    prop.objectReferenceValue = testLevelConfig;
                    serializedObj.ApplyModifiedProperties();

                    Debug.Log($"  ✓ 测试配置已自动加载: {testLevelConfig.levelName}");
                    Debug.Log($"  ✓ 时间限制: {testLevelConfig.timeLimit}秒");
                    Debug.Log($"  ✓ 巡逻间隔: {testLevelConfig.patrolIntervals.x}-{testLevelConfig.patrolIntervals.y}秒");
                }
                else
                {
                    Debug.LogError("  ✗ 无法加载测试配置！");
                }
            }
        }

        [MenuItem("Tools/《灯下黑》/📊 查看测试指南")]
        public static void ShowTestGuide()
        {
            string guide =
                "╔════════════════════════════════════════════════════════════╗\n" +
                "║          《灯下黑》核心系统测试指南                         ║\n" +
                "╚════════════════════════════════════════════════════════════╝\n" +
                "\n" +
                "🚀 快速开始:\n" +
                "   1. 菜单: Tools → 《灯下黑》 → 🚀 一键测试\n" +
                "   2. 等待对话框提示完成\n" +
                "   3. 点击 Play 按钮\n" +
                "   4. 观察测试输出\n" +
                "\n" +
                "📋 测试内容:\n" +
                "   ✓ 游戏流程控制 (开始、暂停、胜利、失败)\n" +
                "   ✓ 老师 AI 状态机 (巡逻、检查)\n" +
                "   ✓ 卡牌读条系统 (读条、打断)\n" +
                "   ✓ 情绪值系统 (增减、临界判定)\n" +
                "   ✓ 事件系统 (所有事件触发)\n" +
                "\n" +
                "👁️ 观察要点:\n" +
                "   • Console 日志: 查看所有系统状态变化\n" +
                "   • 屏幕左上角 GUI: 实时显示游戏状态\n" +
                "   • 老师状态: 应该在 Idle → Approaching → Inspecting → Leaving 之间循环\n" +
                "   • 第 5 秒: 自动开始卡牌读条测试\n" +
                "   • 第 10 秒: 自动测试打断功能\n" +
                "\n" +
                "⏱️ 测试时间: 约 60 秒（测试关卡时间限制）\n" +
                "\n" +
                "✅ 成功标志:\n" +
                "   • 所有系统正常运行\n" +
                "   • 事件正常触发\n" +
                "   • 无报错和警告\n" +
                "   • GUI 实时更新\n" +
                "\n" +
                "📝 测试完成后:\n" +
                "   • 点击 Stop 停止测试\n" +
                "   • 查看 Console 确认所有功能正常\n" +
                "   • 记录任何问题或异常\n";

            Debug.Log(guide);
            EditorUtility.DisplayDialog("测试指南", guide, "知道了");
        }
    }
}
