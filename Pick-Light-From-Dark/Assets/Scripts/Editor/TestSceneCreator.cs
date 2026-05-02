using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.Editor
{
    /// <summary>
    /// 测试场景创建工具
    /// </summary>
    public class TestSceneCreator
    {
        [MenuItem("Tools/《灯下黑》/创建测试场景")]
        public static void CreateTestScene()
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

            // 创建DataManager并添加测试脚本
            GameObject dataManagerObj = new GameObject("DataManager");
            var dataTest = dataManagerObj.AddComponent<Game.Test.DataTest>();

            // 尝试自动加载测试数据
            var testLevelConfig = AssetDatabase.LoadAssetAtPath<Game.Config.LevelConfigSO>("Assets/Resources/TestData/TestLevelConfig.asset");
            var testCardContainer = AssetDatabase.LoadAssetAtPath<Game.Data.CardDataContainer>("Assets/Resources/TestData/TestCardData.asset");

            if (testLevelConfig != null)
            {
                dataTest.testLevelConfig = testLevelConfig;
                Debug.Log("已自动加载 TestLevelConfig");
            }
            else
            {
                Debug.LogWarning("未找到 TestLevelConfig，请手动创建测试数据");
            }

            if (testCardContainer != null)
            {
                var serializedObj = new SerializedObject(dataTest);
                var prop = serializedObj.FindProperty("testCardContainer");
                prop.objectReferenceValue = testCardContainer;
                serializedObj.ApplyModifiedProperties();
                Debug.Log("已自动加载 TestCardData");
            }
            else
            {
                Debug.LogWarning("未找到 TestCardData，请手动创建测试数据");
            }

            // 保存场景
            string scenePath = "Assets/Scenes/DataTest.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"测试场景已创建: {scenePath}");
            Debug.Log("可以直接运行测试了！");
        }
    }
}
