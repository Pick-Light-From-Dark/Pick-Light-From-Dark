using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.Editor
{
    /// <summary>
    /// 综合测试场景创建工具
    /// </summary>
    public class IntegrationTestSceneCreator
    {
        [MenuItem("Tools/《灯下黑》/创建综合测试场景")]
        public static void CreateIntegrationTestScene()
        {
            // 创建新场景
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 创建主相机
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f);

            // 创建灯光
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;

            // 创建系统单例
            GameObject flowObj = new GameObject("GameFlowController");
            flowObj.AddComponent<Game.Flow.GameFlowController>();

            GameObject emotionObj = new GameObject("EmotionSystem");
            emotionObj.AddComponent<Game.Emotion.EmotionSystem>();

            // 创建测试脚本
            GameObject testObj = new GameObject("GameIntegrationTest");
            var test = testObj.AddComponent<Game.System.GameIntegrationTest>();

            // 尝试自动加载测试数据
            var testLevelConfig = AssetDatabase.LoadAssetAtPath<Game.Config.LevelConfigSO>("Assets/Resources/TestData/TestLevelConfig.asset");

            if (testLevelConfig != null)
            {
                var serializedObj = new SerializedObject(test);
                var prop = serializedObj.FindProperty("testLevelConfig");
                prop.objectReferenceValue = testLevelConfig;
                serializedObj.ApplyModifiedProperties();
                Debug.Log("已自动加载 TestLevelConfig");
            }
            else
            {
                Debug.LogWarning("未找到 TestLevelConfig，请先创建测试数据");
            }

            // 保存场景
            string scenePath = "Assets/Scenes/IntegrationTest.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"综合测试场景已创建: {scenePath}");
            Debug.Log("可以直接运行测试了！所有核心系统都会协同工作。");
        }
    }
}
