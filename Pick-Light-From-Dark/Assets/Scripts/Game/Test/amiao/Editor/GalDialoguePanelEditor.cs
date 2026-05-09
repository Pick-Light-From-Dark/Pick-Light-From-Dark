using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class GalDialoguePanelEditor
{
    [MenuItem("Tools/Amiao/Setup GalDialoguePanel Buttons")]
    static void SetupButtons()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            GalDialoguePanel panel = Object.FindObjectOfType<GalDialoguePanel>();
            if (panel != null)
                selected = panel.gameObject;
        }

        if (selected == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选中带有 GalDialoguePanel 的 GameObject", "确定");
            return;
        }

        GalDialoguePanel galPanel = selected.GetComponent<GalDialoguePanel>();
        if (galPanel == null)
        {
            EditorUtility.DisplayDialog("错误", "选中的对象没有 GalDialoguePanel 组件", "确定");
            return;
        }

        Undo.RecordObject(galPanel, "Setup Dialogue Buttons");
        Undo.RecordObject(selected, "Setup Dialogue Buttons");

        if (galPanel.fastForwardBtn == null)
        {
            galPanel.fastForwardBtn = CreateButton(selected.transform, "FastForwardBtn", "快进", new Vector2(-80, -40));
            Undo.RegisterCreatedObjectUndo(galPanel.fastForwardBtn.gameObject, "Create FastForwardBtn");
        }

        if (galPanel.rewindBtn == null)
        {
            galPanel.rewindBtn = CreateButton(selected.transform, "RewindBtn", "快退", new Vector2(-210, -40));
            Undo.RegisterCreatedObjectUndo(galPanel.rewindBtn.gameObject, "Create RewindBtn");
        }

        EditorUtility.SetDirty(galPanel);
        EditorUtility.SetDirty(selected);

        EditorUtility.DisplayDialog("完成", "已为 GalDialoguePanel 添加快进/快退按钮\\n请记得 Save / Apply Prefab Overrides", "确定");
    }

    static Button CreateButton(Transform parent, string name, string text, Vector2 anchoredPos)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing.GetComponent<Button>();

        GameObject btnGO = new GameObject(name, typeof(RectTransform));
        btnGO.transform.SetParent(parent, false);

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(120, 50);

        Image img = btnGO.AddComponent<Image>();
        Sprite defaultSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (defaultSprite != null)
            img.sprite = defaultSprite;
        img.type = Image.Type.Sliced;

        Button btn = btnGO.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        colors.disabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f);
        btn.colors = colors;

        GameObject textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.196f, 0.196f, 0.196f, 1f);

        return btn;
    }
}
