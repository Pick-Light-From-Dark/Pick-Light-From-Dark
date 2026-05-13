using UnityEngine;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// 简易跳过按钮 — 不依赖 FungusVNController，仅用于验证按钮在场景中可见
    /// 挂载到任意 GameObject，运行时自动创建 Canvas + Button
    /// 使用 Legacy Text + 项目字体，避免 TMPro 材质缺失导致的方块字问题
    /// </summary>
    public class SimpleSkipButton : MonoBehaviour
    {
        [Header("按钮配置")]
        public string buttonText = "跳过";
        public Vector2 buttonSize = new Vector2(100, 45);
        public Vector2 anchoredPosition = new Vector2(-140, -30);

        [Header("运行时自动创建")]
        public bool createOnStart = true;

        [Header("延迟创建（秒）")]
        public float delayCreate = 0.5f;

        [Header("初始可见性")]
        public bool visibleOnStart = true;

        private Button skipBtn;

        void Start()
        {
            if (createOnStart)
            {
                if (delayCreate > 0f)
                    Invoke(nameof(CreateButton), delayCreate);
                else
                    CreateButton();
            }
        }

        [ContextMenu("创建跳过按钮")]
        public void CreateButton()
        {
            // 查找或创建 Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("SkipButtonCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // 查找或销毁旧按钮
            var oldBtn = GameObject.Find("SimpleSkipBtn");
            if (oldBtn != null) Destroy(oldBtn);

            // 创建按钮 GameObject
            var btnGo = new GameObject("SimpleSkipBtn");
            btnGo.transform.SetParent(canvas.transform, false);

            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = buttonSize;

            btnGo.AddComponent<CanvasRenderer>();

            // 纯色背景，避免内置 sprite 加载失败导致纯白巨块
            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

            skipBtn = btnGo.AddComponent<Button>();
            skipBtn.targetGraphic = img;
            skipBtn.onClick.AddListener(() =>
            {
                Debug.Log("[SimpleSkipButton] 跳过按钮被点击（仅测试，无实际功能）");
            });

            // 创建文本（Legacy Text，与 FungusVNController.CreateButton 一致）
            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(10, 5);
            txtRt.offsetMax = new Vector2(-10, -5);

            txtGo.AddComponent<CanvasRenderer>();
            var txt = txtGo.AddComponent<Text>();
            txt.text = buttonText;
            txt.fontSize = 26;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            // 加载项目中文字体（与 SkipButtonTest / FungusVNController 一致）
            Font chineseFont = Resources.Load<Font>("Font/LXGWWenKaiScreen");
            if (chineseFont == null) chineseFont = Resources.Load<Font>("Font/文软雅黑");
            if (chineseFont == null) chineseFont = Resources.Load<Font>("Fonts/文软雅黑");
            if (chineseFont != null)
                txt.font = chineseFont;
            else
                Debug.LogWarning("[SimpleSkipButton] 未找到中文字体资源，将使用默认字体");

            // 确保按钮在最上层
            var btnCanvas = btnGo.GetComponent<Canvas>();
            if (btnCanvas == null) btnCanvas = btnGo.AddComponent<Canvas>();
            btnCanvas.overrideSorting = true;
            btnCanvas.sortingOrder = 100;
            if (btnGo.GetComponent<GraphicRaycaster>() == null)
                btnGo.AddComponent<GraphicRaycaster>();

            btnGo.SetActive(visibleOnStart);

            Debug.Log($"[SimpleSkipButton] 跳过按钮已创建，visible={visibleOnStart}");
        }

        [ContextMenu("显示按钮")]
        public void ShowButton()
        {
            if (skipBtn != null) skipBtn.gameObject.SetActive(true);
        }

        [ContextMenu("隐藏按钮")]
        public void HideButton()
        {
            if (skipBtn != null) skipBtn.gameObject.SetActive(false);
        }
    }
}
