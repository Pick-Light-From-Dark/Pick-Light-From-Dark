using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.UI;

namespace Game.UI
{
    public class LuYingAnimTestPanel : MonoBehaviour
    {
        private LuYingCharacterManager manager;

        void Start()
        {
            manager = FindObjectOfType<LuYingCharacterManager>();
            if (manager == null) { Debug.LogError("LuYingCharacterManager not found"); return; }

            var canvas = GetComponentInParent<Canvas>().transform;
            var font = Resources.Load<TMP_FontAsset>("Font/wenkai");

            var buttons = new[]
            {
                ("BtnNormal", "Normal", 240, (System.Action)(() => manager.SetMood(0))),
                ("BtnLittleExcited", "LittleExcited", 190, (System.Action)(() => manager.SetMood(1))),
                ("BtnExcited", "Excited", 140, (System.Action)(() => manager.SetMood(2))),
                ("BtnChew", "Chew", 90, (System.Action)(() => manager.PlayChew())),
                ("BtnSwitch", "Switch Bed/Stand", 20, (System.Action)(() => manager.SetInBed(!manager.IsInBed))),
            };

            var panel = new GameObject("TestPanel");
            panel.transform.SetParent(canvas, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0.5f);
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.pivot = new Vector2(1, 0.5f);
            panelRect.anchoredPosition = new Vector2(-10, 0);
            panelRect.sizeDelta = new Vector2(180, 400);
            panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            foreach (var (name, label, yPos, action) in buttons)
            {
                CreateButton(name, panel.transform, new Vector2(0, yPos), new Vector2(160, 40), label, action, font);
            }
        }

        Button CreateButton(string name, Transform parent, Vector2 pos, Vector2 size, string label, System.Action onClick, TMP_FontAsset font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            go.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.8f, 1f);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick());

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var txt = textGo.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.font = font;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.fontSize = 16;

            return btn;
        }
    }
}
