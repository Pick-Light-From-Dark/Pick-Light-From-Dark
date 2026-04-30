using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.Editor
{
    /// <summary>
    /// 情绪值系统测试场景创建工具
    /// </summary>
    public class EmotionTestSceneCreator
    {
        [MenuItem("Tools/《灯下黑》/创建情绪系统测试场景")]
        public static void CreateEmotionTestScene()
        {
            // 创建新场景
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 创建主相机
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0, 0, -10);

            // 创建灯光
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;

            // 创建EmotionSystem单例
            GameObject emotionSystemObj = new GameObject("EmotionSystem");
            emotionSystemObj.AddComponent<Game.Emotion.EmotionSystem>();

            // 创建测试脚本
            GameObject testObj = new GameObject("EmotionTest");
            var emotionTest = testObj.AddComponent<Game.System.EmotionTest>();

            // 尝试自动加载测试数据
            var testLevelConfig = AssetDatabase.LoadAssetAtPath<Game.Config.LevelConfigSO>("Assets/Resources/TestData/TestLevelConfig.asset");

            if (testLevelConfig != null)
            {
                var serializedObj = new SerializedObject(emotionTest);
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
            string scenePath = "Assets/Scenes/EmotionTest.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"情绪系统测试场景已创建: {scenePath}");
            Debug.Log("可以直接运行测试了！");
        }
    }
}
