using UnityEngine;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// 简易跳过按钮 — 不依赖 FungusVNController，仅用于验证按钮在场景中可见
    /// 挂载到任意 GameObject，运行时自动创建 Canvas + Button
    /// </summary>
    public class SimpleSkipButton : MonoBehaviour
    {
        [Header("按钮配置")]
        public string buttonText = "跳过";
        public Vector2 buttonSize = new Vector2(100, 45);
        public Vector2 anchoredPosition = new Vector2(-140, -30);

        [Header("运行时自动创建")]
        public bool createOnStart = true;

        [Header("初始可见性")]
        public bool visibleOnStart = true;

        private Button skipBtn;

        void Start()
        {
            if (createOnStart)
            {
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
                canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
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

            var img = btnGo.AddComponent<Image>();
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
            img.type = Image.Type.Sliced;
            img.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            skipBtn = btnGo.AddComponent<Button>();
            skipBtn.targetGraphic = img;
            skipBtn.onClick.AddListener(() =>
            {
                Debug.Log("[SimpleSkipButton] 跳过按钮被点击（仅测试，无实际功能）");
            });

            // 创建文本
            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            txtGo.AddComponent<CanvasRenderer>();
            var tmp = txtGo.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = buttonText;
            tmp.fontSize = 24;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color(0.2f, 0.2f, 0.2f, 1f);

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
