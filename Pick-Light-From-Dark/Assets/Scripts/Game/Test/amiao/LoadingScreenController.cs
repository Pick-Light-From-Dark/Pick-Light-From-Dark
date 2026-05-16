using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Game.Test
{
    /// <summary>
    /// 全屏 Loading 画面控制器 — 用于剧情↔游玩切换时遮挡透明帧
    /// 使用方式：LoadingScreenController.Show() / Hide()
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("UI 引用")]
        public CanvasGroup canvasGroup;
        public Text loadingText;
        public Image backgroundImage;

        [Header("默认配置")]
        public float defaultFadeDuration = 0.3f;
        public string defaultLoadingText = "Loading...";

        public static LoadingScreenController Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            if (canvasGroup != null)
                canvasGroup.alpha = 0;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>显示 Loading 画面（带淡入）</summary>
        public static void Show(float fadeDuration = -1f)
        {
            EnsureInstance();
            if (Instance == null) return;
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.FadeIn(fadeDuration < 0 ? Instance.defaultFadeDuration : fadeDuration));
        }

        /// <summary>隐藏 Loading 画面（带淡出）</summary>
        public static void Hide(float fadeDuration = -1f)
        {
            if (Instance == null) return;
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.FadeOut(fadeDuration < 0 ? Instance.defaultFadeDuration : fadeDuration));
        }

        /// <summary>立即显示（无动画）</summary>
        public static void ShowImmediate(string text = null)
        {
            EnsureInstance();
            if (Instance == null) return;
            Instance.StopAllCoroutines();
            if (Instance.canvasGroup != null)
                Instance.canvasGroup.alpha = 1;
            if (text != null && Instance.loadingText != null)
                Instance.loadingText.text = text;
        }

        /// <summary>立即隐藏（无动画）</summary>
        public static void HideImmediate()
        {
            if (Instance == null) return;
            Instance.StopAllCoroutines();
            if (Instance.canvasGroup != null)
                Instance.canvasGroup.alpha = 0;
        }

        /// <summary>设置 Loading 文字</summary>
        public static void SetText(string text)
        {
            if (Instance == null) return;
            if (Instance.loadingText != null)
                Instance.loadingText.text = text;
        }

        static void EnsureInstance()
        {
            if (Instance != null) return;

            var existing = FindObjectOfType<LoadingScreenController>();
            if (existing != null)
            {
                Instance = existing;
                return;
            }

            // 自动创建
            var go = new GameObject("LoadingScreenController");
            var ctrl = go.AddComponent<LoadingScreenController>();
            ctrl.CreateUI(go);
            Instance = ctrl;
            DontDestroyOnLoad(go);
        }

        void CreateUI(GameObject root)
        {
            // Canvas
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvas.overrideSorting = true;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0;

            root.AddComponent<GraphicRaycaster>();

            // CanvasGroup
            canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;

            // Background Image
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(root.transform, false);
            backgroundImage = bgGo.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 1);
            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Loading Text
            var textGo = new GameObject("LoadingText");
            textGo.transform.SetParent(root.transform, false);
            loadingText = textGo.AddComponent<Text>();
            loadingText.text = defaultLoadingText;
            loadingText.color = Color.white;
            loadingText.fontSize = 36;
            loadingText.alignment = TextAnchor.MiddleCenter;
            loadingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(600, 80);
        }

        IEnumerator FadeIn(float duration)
        {
            if (canvasGroup == null) yield break;
            canvasGroup.blocksRaycasts = true;
            float t = 0;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / duration);
                yield return null;
            }
            canvasGroup.alpha = 1;
        }

        IEnumerator FadeOut(float duration)
        {
            if (canvasGroup == null) yield break;
            float t = 0;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - t / duration);
                yield return null;
            }
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
