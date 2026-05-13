using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Test
{
    /// <summary>
    /// 素材缺失占位文字显示器 — 独立组件，不依赖 FungusVNController
    /// 在画面最前左上角显示缺失的素材名称
    /// </summary>
    public class PlaceholderDisplay : MonoBehaviour
    {
        [Header("文本样式")]
        public int fontSize = 18;
        public Color textColor = Color.yellow;
        public Color outlineColor = Color.black;
        public Vector2 outlineDistance = new Vector2(1.5f, -1.5f);

        [Header("布局")]
        public Vector2 panelSize = new Vector2(600, 200);
        public Vector2 anchoredPosition = new Vector2(10, -10);

        [Header("排序")]
        public int sortingOrder = 999;

        private Text displayText;
        private HashSet<string> loggedAssets = new HashSet<string>();
        private Canvas canvas;

        void Awake()
        {
            EnsureCanvas();
            EnsureText();
        }

        /// <summary>显示素材缺失占位文字（自动去重）</summary>
        public void Show(string assetType, string assetName)
        {
            string key = $"{assetType}:{assetName}";
            if (loggedAssets.Contains(key)) return;
            loggedAssets.Add(key);

            if (displayText != null)
            {
                displayText.gameObject.SetActive(true);
                displayText.text += $"{assetType} missing: {assetName}\n";
            }
            Debug.LogWarning($"[PlaceholderDisplay] {assetType} missing: {assetName}");
        }

        /// <summary>清除所有占位文字</summary>
        public void Clear()
        {
            loggedAssets.Clear();
            if (displayText != null)
            {
                displayText.text = "";
                displayText.gameObject.SetActive(false);
            }
        }

        void EnsureCanvas()
        {
            if (canvas != null) return;

            // 查找已有的 PlaceholderCanvas
            var existing = GameObject.Find("PlaceholderCanvas");
            if (existing != null)
            {
                canvas = existing.GetComponent<Canvas>();
                if (canvas != null) return;
            }

            var go = new GameObject("PlaceholderCanvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.AddComponent<GraphicRaycaster>();
        }

        void EnsureText()
        {
            if (displayText != null) return;

            var existing = canvas.transform.Find("PlaceholderText");
            GameObject textGo;
            if (existing != null)
            {
                textGo = existing.gameObject;
                displayText = textGo.GetComponent<Text>();
                if (displayText != null) return;
            }
            else
            {
                textGo = new GameObject("PlaceholderText");
                textGo.transform.SetParent(canvas.transform, false);
            }

            var rect = textGo.GetComponent<RectTransform>();
            if (rect == null) rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = panelSize;

            textGo.AddComponent<CanvasRenderer>();
            displayText = textGo.GetComponent<Text>();
            if (displayText == null) displayText = textGo.AddComponent<Text>();
            displayText.fontSize = fontSize;
            displayText.color = textColor;
            displayText.alignment = TextAnchor.UpperLeft;
            displayText.raycastTarget = false;

            // 黑色描边
            var outline = textGo.GetComponent<Outline>();
            if (outline == null) outline = textGo.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = outlineDistance;

            textGo.SetActive(false);
        }
    }
}
