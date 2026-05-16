using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// 章节开场图 — 每关开始前全屏展示 Resources/Backgrounds/night1~4
    /// </summary>
    public class ChapterSplashController : MonoBehaviour
    {
        const string ResourceFolder = "Backgrounds";

        [Header("显示时长")]
        public float displayDuration = 2.5f;
        public float fadeInDuration = 0.4f;
        public float fadeOutDuration = 0.5f;

        [Header("交互")]
        public bool allowClickToSkip = true;

        CanvasGroup canvasGroup;
        Image chapterImage;

        /// <summary>
        /// 显示对应章节的插图，结束后执行 onComplete。
        /// levelId: 1→night1, 2→night2, 3→night3, 4→night4, 5→night4
        /// </summary>
        public static void ShowForChapter(int levelId, Action onComplete)
        {
            var go = new GameObject("ChapterSplash");
            var ctrl = go.AddComponent<ChapterSplashController>();
            ctrl.StartCoroutine(ctrl.Run(levelId, onComplete));
        }

        IEnumerator Run(int levelId, Action onComplete)
        {
            BuildUI();

            var sprite = LoadChapterSprite(levelId);
            if (sprite != null)
            {
                chapterImage.sprite = sprite;
                chapterImage.preserveAspect = true;
                chapterImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"[ChapterSplash] 未找到章节图 levelId={levelId}");
                chapterImage.color = Color.black;
            }

            yield return Fade(0f, 1f, fadeInDuration);

            float elapsed = 0f;
            bool skipped = false;
            while (elapsed < displayDuration && !skipped)
            {
                if (allowClickToSkip && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)
                    || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.F8)))
                    skipped = true;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return Fade(1f, 0f, fadeOutDuration);

            onComplete?.Invoke();
            Destroy(gameObject);
        }

        static Sprite LoadChapterSprite(int levelId)
        {
            int imageIndex = Mathf.Clamp(levelId, 1, 4);
            string path = $"{ResourceFolder}/night{imageIndex}";
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
            {
                var tex = Resources.Load<Texture2D>(path);
                if (tex != null)
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            return sprite;
        }

        void BuildUI()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 980;
            canvas.overrideSorting = true;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;

            var bgGo = new GameObject("ChapterImage");
            bgGo.transform.SetParent(transform, false);
            chapterImage = bgGo.AddComponent<Image>();
            chapterImage.raycastTarget = true;
            var imgRect = bgGo.GetComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one;
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            if (canvasGroup == null || duration <= 0f)
            {
                if (canvasGroup != null) canvasGroup.alpha = to;
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}
