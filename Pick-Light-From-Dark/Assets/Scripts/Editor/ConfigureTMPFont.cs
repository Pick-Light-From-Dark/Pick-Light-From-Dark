using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace Game.Editor
{
    /// <summary>
    /// 配置TextMeshPro中文字体
    /// </summary>
    public class ConfigureTMPFont
    {
        [MenuItem("Tools/《灯下黑》/配置TextMeshPro中文字体")]
        public static void ConfigureChineseFont()
        {
            // 中文字体资源路径
            string chineseFontPath = "Assets/Resources/Font/卓特自由体-TTF SDF.asset";
            TMP_FontAsset chineseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(chineseFontPath);

            if (chineseFont == null)
            {
                Debug.LogError($"找不到中文字体资源: {chineseFontPath}");
                return;
            }

            Debug.Log($"开始配置中文字体: {chineseFont.name}");

            // 查找场景中所有TextMeshProUGUI组件
            TextMeshProUGUI[] tmpTexts = GameObject.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            int count = 0;

            foreach (var tmp in tmpTexts)
            {
                // 使用SerializedObject来修改字体
                SerializedObject so = new SerializedObject(tmp);
                SerializedProperty fontProp = so.FindProperty("m_fontAsset");

                if (fontProp != null && fontProp.objectReferenceValue != null)
                {
                    string currentFontName = fontProp.objectReferenceValue.name;

                    // 只修改使用默认字体的组件
                    if (currentFontName.Contains("LiberationSans"))
                    {
                        fontProp.objectReferenceValue = chineseFont;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(tmp);
                        count++;
                        Debug.Log($"已配置: {tmp.gameObject.name} -> {tmp.text}");
                    }
                }
            }

            Debug.Log($"字体配置完成！共修改了 {count} 个TextMeshPro组件");

            // 保存场景
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
