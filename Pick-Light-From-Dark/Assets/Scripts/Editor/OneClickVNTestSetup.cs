using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.Editor
{
    /// <summary>
    /// 一键创建视觉小说对话测试场景
    /// </summary>
    public class OneClickVNTestSetup
    {
        [MenuItem("Tools/《灯下黑》/🎬 一键创建 VN 对话测试场景")]
        public static void CreateVNTestScene()
        {
            // 创建新场景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 创建 Camera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5;
            camGo.tag = "MainCamera";

            // 创建 Canvas
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 创建 EventSystem
            var esGo = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

            // 创建测试对象
            var testGo = new GameObject("VNTestRunner");
            var vnTest = testGo.AddComponent<Game.Test.DialogueVNTest>();

            // 尝试自动加载 Dialogue1
            var dialogueText = Resources.Load<TextAsset>("Dialogue1");
            if (dialogueText != null)
            {
                var so = new UnityEditor.SerializedObject(vnTest);
                var prop = so.FindProperty("dialogueText");
                prop.objectReferenceValue = dialogueText;
                so.ApplyModifiedProperties();
            }

            // 保存场景
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/AmiaoTestVNScene.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== ✅ VN 对话测试场景创建完成 ===");
            Debug.Log("场景路径: Assets/Scenes/AmiaoTestVNScene.unity");
            Debug.Log("包含组件: Camera / Canvas / EventSystem / VNTestRunner(DialogueVNTest)");
            Debug.Log("点击 Play 即可体验视觉小说对话效果");

            EditorUtility.DisplayDialog(
                "VN 测试场景创建完成",
                "场景已保存到 Assets/Scenes/AmiaoTestVNScene.unity\n\n" +
                "点击 Play 即可体验：\n" +
                "• 打字机逐字显示\n" +
                "• 点击跳过打字\n" +
                "• 立绘淡入淡出\n" +
                "• 背景划入划出\n" +
                "• 选项分支\n\n" +
                "使用 Dialogue1.txt 作为测试文本。",
                "好的"
            );
        }
    }
}
