using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.System;

namespace Game.Editor
{
    /// <summary>
    /// 完整游戏测试场景创建工具
    /// </summary>
    public class FullGameTestSceneCreator
    {
        [MenuItem("Tools/《灯下黑》/🎮 创建完整游戏测试")]
        public static void CreateFullGameTestScene()
        {
            // 创建新场景
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 设置场景
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;

            // 创建系统单例
            GameObject flowObj = new GameObject("GameFlowController");
            flowObj.AddComponent<Game.Flow.GameFlowController>();

            GameObject emotionObj = new GameObject("EmotionSystem");
            emotionObj.AddComponent<Game.Emotion.EmotionSystem>();

            GameObject playerStateObj = new GameObject("PlayerState");
            playerStateObj.AddComponent<Game.Data.PlayerState>();

            GameObject eyeCloseObj = new GameObject("EyeCloseSystem");
            eyeCloseObj.AddComponent<Game.EyeClose.EyeCloseSystem>();

            GameObject cardManagerObj = new GameObject("CardManager");
            cardManagerObj.AddComponent<Game.Card.CardManager>();

            // 创建测试脚本
            GameObject testObj = new GameObject("FullGameTest");
            Game.System.FullGameTest test = testObj.AddComponent<Game.System.FullGameTest>();
            test.autoStart = true;

            // 加载测试数据
            var testLevelConfig = AssetDatabase.LoadAssetAtPath<Game.Config.LevelConfigSO>("Assets/Resources/TestData/TestLevelConfig.asset");

            if (testLevelConfig != null)
            {
                var serializedObj = new SerializedObject(test);
                var prop = serializedObj.FindProperty("testLevelConfig");
                prop.objectReferenceValue = testLevelConfig;
                serializedObj.ApplyModifiedProperties();

                Debug.Log($"✓ 测试配置已加载: {testLevelConfig.levelName}");
            }

            // 保存场景
            string scenePath = "Assets/Scenes/FullGameTest.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"✓ 完整游戏测试场景已创建: {scenePath}");
            Debug.Log("✓ 可以直接运行测试所有系统协同工作");

            EditorUtility.DisplayDialog(
                "测试场景创建完成",
                "完整游戏测试场景已创建！\n\n" +
                "测试内容：\n" +
                "• 老师AI查寝判定\n" +
                "• 闭眼系统（C键）\n" +
                "• 被抓判定\n" +
                "• 时间加速\n" +
                "• 卡牌系统\n\n" +
                "点击Play开始测试",
                "好的"
            );
        }
    }
}
