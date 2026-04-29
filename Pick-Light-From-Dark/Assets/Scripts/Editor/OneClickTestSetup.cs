using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using Game.Data;
using Game.Config;
using Game.Testing;

namespace Game.Editor
{
    /// <summary>
    /// 一键测试工具 - 自动准备所有测试环境
    /// </summary>
    public class OneClickTestSetup
    {
        [MenuItem("Tools/《灯下黑》/🚀 一键自动化测试 (自动准备所有环境)")]
        public static void SetupAndRunTest()
        {
            Debug.Log("=== 🚀 开始一键自动化测试环境准备 ===");

            // 步骤1: 创建测试数据
            Debug.Log("步骤1/4: 创建测试数据...");
            CreateTestData();

            // 步骤2: 创建测试场景
            Debug.Log("步骤2/4: 创建自动化测试场景...");
            Scene testScene = CreateAutomatedTestScene();

            // 步骤3: 配置测试场景
            Debug.Log("步骤3/4: 配置测试场景...");
            ConfigureAutomatedTestScene(testScene);

            // 步骤4: 保存并提示
            Debug.Log("步骤4/4: 保存场景...");
            EditorSceneManager.SaveScene(testScene, "Assets/Scenes/OneClickTest.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== ✅ 自动化测试环境准备完成！===");
            Debug.Log("📝 自动化测试说明:");
            Debug.Log("   1. 点击 Unity 的 Play 按钮开始自动化测试");
            Debug.Log("   2. 测试将自动运行8个测试阶段");
            Debug.Log("   3. 观察Console的测试日志和断言结果");
            Debug.Log("   4. 查看屏幕上的GUI显示测试结果");
            Debug.Log("   5. 测试完成后查看通过率");
            Debug.Log("");
            Debug.Log("⏹️  测试结束后点击 Stop 停止");

            // 显示完成对话框
            EditorUtility.DisplayDialog(
                "自动化测试环境准备完成 ✅",
                "所有自动化测试环境和数据已自动创建完成！\n\n" +
                "现在可以直接点击 Play 按钮开始测试。\n\n" +
                "测试内容（8个阶段）：\n" +
                "• 1. 系统初始化测试\n" +
                "• 2. 游戏流程测试\n" +
                "• 3. 情绪系统测试\n" +
                "• 4. 玩家状态测试\n" +
                "• 5. 老师AI测试\n" +
                "• 6. 卡牌系统测试\n" +
                "• 7. 闭眼系统测试\n" +
                "• 8. 游戏结束测试\n\n" +
                "测试将自动运行并显示结果。",
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

        static Scene CreateAutomatedTestScene()
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

            // 创建自动化测试脚本
            GameObject testObj = new GameObject("AutomatedTestRunner");
            AutomatedGameTest test = testObj.AddComponent<AutomatedGameTest>();

            return newScene;
        }

        static void ConfigureAutomatedTestScene(Scene scene)
        {
            // 查找测试对象并配置
            GameObject[] rootObjects = scene.GetRootGameObjects();
            AutomatedGameTest test = null;

            foreach (var obj in rootObjects)
            {
                if (obj.name == "AutomatedTestRunner")
                {
                    test = obj.GetComponent<AutomatedGameTest>();
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

        [MenuItem("Tools/《灯下黑》/📊 查看自动化测试指南")]
        public static void ShowTestGuide()
        {
            string guide =
                "╔════════════════════════════════════════════════════════════╗\n" +
                "║          《灯下黑》自动化测试指南                           ║\n" +
                "╚════════════════════════════════════════════════════════════╝\n" +
                "\n" +
                "🚀 快速开始:\n" +
                "   1. 菜单: Tools → 《灯下黑》 → 🚀 一键自动化测试\n" +
                "   2. 等待对话框提示完成\n" +
                "   3. 点击 Play 按钮\n" +
                "   4. 测试自动运行并输出结果\n" +
                "\n" +
                "📋 8个自动化测试阶段:\n" +
                "   1. ✓ 系统初始化测试\n" +
                "   2. ✓ 游戏流程测试 (暂停/恢复/倒计时)\n" +
                "   3. ✓ 情绪系统测试 (慌乱值/兴奋值)\n" +
                "   4. ✓ 玩家状态测试 (卧床/闭眼)\n" +
                "   5. ✓ 老师AI测试 (状态转换)\n" +
                "   6. ✓ 卡牌系统测试 (读条/打断)\n" +
                "   7. ✓ 闭眼系统测试 (时长/加速)\n" +
                "   8. ✓ 游戏结束测试 (胜利/失败)\n" +
                "\n" +
                "👁️ 观察要点:\n" +
                "   • Console 日志: 查看测试进度和断言结果\n" +
                "   • 屏幕GUI: 显示测试通过数/失败数/成功率\n" +
                "   • 绿色=全部通过，红色=有失败\n" +
                "   • deltaTime异常已修复 (限制最大0.1秒)\n" +
                "   • 多实例检测已添加\n" +
                "\n" +
                "⏱️ 测试时间: 约 10-15秒 (自动化测试)\n" +
                "\n" +
                "✅ 成功标志:\n" +
                "   • 所有测试断言通过\n" +
                "   • 成功率 100%\n" +
                "   • GUI显示绿色 ✓\n" +
                "\n" +
                "📝 测试完成后:\n" +
                "   • 点击 Stop 停止测试\n" +
                "   • 查看 Console 确认所有测试通过\n" +
                "   • 如有失败，查看具体失败信息";

            Debug.Log(guide);
            EditorUtility.DisplayDialog("自动化测试指南", guide, "知道了");
        }
    }
}
