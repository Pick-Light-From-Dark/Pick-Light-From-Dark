using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Fungus;

namespace Game.Editor
{
    public class ConvertSayDialogToTMP
    {
        [MenuItem("Tools/《灯下黑》/SayDialog 转 TextMeshPro")]
        public static void Convert()
        {
            var prefabPath = "Assets/ThirdParty/Fungus/Resources/Prefabs/SayDialog.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("找不到 SayDialog.prefab");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, PrefabStageUtility.GetCurrentPrefabStage()?.scene ?? EditorSceneManager.GetActiveScene());

            var sayDialog = instance.GetComponent<SayDialog>();
            if (sayDialog == null)
            {
                Debug.LogError("SayDialog 组件未找到");
                Object.DestroyImmediate(instance);
                return;
            }

            // 通过反射获取 storyText 和 nameText 字段
            var storyTextField = typeof(SayDialog).GetField("storyText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nameTextField = typeof(SayDialog).GetField("nameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var storyTextGOField = typeof(SayDialog).GetField("storyTextGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nameTextGOField = typeof(SayDialog).GetField("nameTextGO", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var oldStoryText = storyTextField?.GetValue(sayDialog) as UnityEngine.UI.Text;
            var oldNameText = nameTextField?.GetValue(sayDialog) as UnityEngine.UI.Text;

            int count = 0;

            // 转换 StoryText
            if (oldStoryText != null)
            {
                var go = oldStoryText.gameObject;
                var text = oldStoryText.text;
                var fontSize = oldStoryText.fontSize;
                var color = oldStoryText.color;
                var alignment = oldStoryText.alignment;

                Object.DestroyImmediate(oldStoryText, true);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = text;
                tmp.fontSize = fontSize;
                tmp.color = color;
                tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Font/wenkai.asset");
                tmp.alignment = TextAlignmentOptions.TopLeft;

                storyTextField.SetValue(sayDialog, null);
                storyTextGOField.SetValue(sayDialog, go);
                count++;
            }

            // 转换 NameText
            if (oldNameText != null)
            {
                var go = oldNameText.gameObject;
                var text = oldNameText.text;
                var fontSize = oldNameText.fontSize;
                var color = oldNameText.color;
                var alignment = oldNameText.alignment;

                Object.DestroyImmediate(oldNameText, true);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = text;
                tmp.fontSize = fontSize;
                tmp.color = color;
                tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Font/wenkai.asset");
                tmp.alignment = TextAlignmentOptions.Left;

                nameTextField.SetValue(sayDialog, null);
                nameTextGOField.SetValue(sayDialog, go);
                count++;
            }

            // 转换 Continue 按钮下的 Text
            var continueBtn = instance.transform.Find("Panel/StoryText/Continue/Text");
            if (continueBtn != null)
            {
                var oldText = continueBtn.GetComponent<UnityEngine.UI.Text>();
                if (oldText != null)
                {
                    var text = oldText.text;
                    var fontSize = oldText.fontSize;
                    var color = oldText.color;

                    Object.DestroyImmediate(oldText, true);
                    var tmp = continueBtn.gameObject.AddComponent<TextMeshProUGUI>();
                    tmp.text = text;
                    tmp.fontSize = fontSize;
                    tmp.color = color;
                    tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Font/wenkai.asset");
                    tmp.alignment = TextAlignmentOptions.Center;
                    count++;
                }
            }

            // 保存回 prefab
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            Debug.Log($"[ConvertSayDialogToTMP] 完成！共转换 {count} 个 Text → TMP");
            AssetDatabase.SaveAssets();
        }
    }
}
