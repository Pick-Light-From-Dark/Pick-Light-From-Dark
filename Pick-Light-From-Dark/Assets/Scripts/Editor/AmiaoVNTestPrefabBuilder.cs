using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Editor
{
    /// <summary>
    /// 在 Amiao_Test 场景中搭建 VN 对话测试 Prefab
    /// </summary>
    public class AmiaoVNTestPrefabBuilder
    {
        [MenuItem("Tools/《灯下黑》/🔧 在 Amiao_Test 搭建 VN 测试 Prefab")]
        public static void BuildPrefabAndScene()
        {
            string prefabPath = "Assets/Scenes/Amiao_Test/TestPrefabs/DialogueVNTest.prefab";
            string scenePath = "Assets/Scenes/AmiaoTestScene.unity";

            // 1. 创建 VNTestRoot 层级
            GameObject vnRoot = new GameObject("VNTestRoot");

            // 1.1 DialogueSystem
            GameObject dsGo = new GameObject("DialogueSystem", typeof(DialogueSystem));
            dsGo.transform.SetParent(vnRoot.transform, false);

            // 1.2 VNTestRunner (DialogueVNTest)
            GameObject runnerGo = new GameObject("VNTestRunner", typeof(Game.Test.DialogueVNTest));
            runnerGo.transform.SetParent(vnRoot.transform, false);

            var vnTest = runnerGo.GetComponent<Game.Test.DialogueVNTest>();
            var so = new SerializedObject(vnTest);

            // 自动赋值 dialogueText
            var dialogueText = Resources.Load<TextAsset>("Dialogue1");
            if (dialogueText != null)
            {
                so.FindProperty("dialogueText").objectReferenceValue = dialogueText;
            }
            so.FindProperty("runOnStart").boolValue = true;
            so.FindProperty("mode").intValue = (int)DialogueSystem.DialogueMode.Gal;
            so.ApplyModifiedProperties();

            // 2. 保存为 Prefab
            string prefabDir = System.IO.Path.GetDirectoryName(prefabPath);
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/Scenes/Amiao_Test", "TestPrefabs");
            }

            // 如果已存在则替换
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                PrefabUtility.SaveAsPrefabAsset(vnRoot, prefabPath);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(vnRoot, prefabPath);
            }

            // 保存后销毁临时对象
            Object.DestroyImmediate(vnRoot);

            // 3. 配置场景
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);

            // 查找或创建 Camera
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                var cam = camGo.AddComponent<Camera>();
                cam.backgroundColor = Color.black;
                cam.orthographic = true;
                cam.orthographicSize = 5;
                camGo.tag = "MainCamera";
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            }

            // 查找或创建 Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;

                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            }

            // 查找或创建 EventSystem
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            }

            // 查找或实例化 VNTestRoot Prefab
            GameObject existingRoot = GameObject.Find("VNTestRoot");
            if (existingRoot == null)
            {
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabAsset != null)
                {
                    PrefabUtility.InstantiatePrefab(prefabAsset);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            // 保存场景
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== ✅ VN 测试 Prefab & 场景搭建完成 ===");
            Debug.Log($"Prefab: {prefabPath}");
            Debug.Log($"Scene: {scenePath}");
            Debug.Log("操作：打开 AmiaoTestScene.unity，点击 Play 即可测试");

            EditorUtility.DisplayDialog(
                "VN 测试搭建完成",
                "Prefab 已保存到:\n" + prefabPath + "\n\n" +
                "场景已配置:\n" + scenePath + "\n\n" +
                "打开场景点击 Play 即可测试视觉小说对话。",
                "好的"
            );
        }
    }
}
