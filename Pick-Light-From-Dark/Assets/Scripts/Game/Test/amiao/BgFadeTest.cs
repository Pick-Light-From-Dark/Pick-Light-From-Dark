using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// 独立的背景 Fade 测试脚本。
    /// 自动循环切换 light_room → CheckScores_2 → CheckScores_1，中间使用 fade。
    /// </summary>
    public class BgFadeTest : MonoBehaviour
    {
        [Header("要测试的背景图名称（按顺序）")]
        public string[] bgNames = new string[] { "light_room", "CheckScores_2", "CheckScores_1" };

        [Header("切换间隔（秒）")]
        public float switchInterval = 2f;

        [Header("Fade 时长（秒）")]
        public float fadeDuration = 0.5f;

        private Image bgImage;
        private Canvas testCanvas;
        private int currentIndex = 0;

        void Start()
        {
            // 创建测试 Canvas
            var canvasGo = new GameObject("TestFadeCanvas");
            testCanvas = canvasGo.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            testCanvas.sortingOrder = -10;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 创建背景 Image
            var imgGo = new GameObject("TestBgImage");
            imgGo.transform.SetParent(canvasGo.transform, false);
            var rect = imgGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // 保险：显式添加 CanvasRenderer
            var renderer = imgGo.GetComponent<CanvasRenderer>();
            if (renderer == null) renderer = imgGo.AddComponent<CanvasRenderer>();

            bgImage = imgGo.AddComponent<Image>();
            bgImage.color = Color.clear;
            bgImage.raycastTarget = false;

            Debug.Log($"[BgFadeTest] Canvas 和 Image 已创建。Image.active={bgImage.gameObject.activeSelf}, CanvasRenderer={(renderer != null)}");

            // 开始循环
            StartCoroutine(LoopTest());
        }

        IEnumerator LoopTest()
        {
            // 第一张图直接显示
            var first = LoadSprite(bgNames[0]);
            if (first != null)
            {
                bgImage.sprite = first;
                bgImage.color = Color.white;
                Debug.Log($"[BgFadeTest] 直接显示第一张: {bgNames[0]}");
            }
            else
            {
                Debug.LogError($"[BgFadeTest] 第一张图加载失败: {bgNames[0]}");
            }

            yield return new WaitForSeconds(switchInterval);

            while (true)
            {
                currentIndex = (currentIndex + 1) % bgNames.Length;
                string nextName = bgNames[currentIndex];
                Sprite nextSprite = LoadSprite(nextName);

                if (nextSprite != null)
                {
                    Debug.Log($"[BgFadeTest] 开始 Fade 到: {nextName}");
                    yield return StartCoroutine(DoFade(bgImage, nextSprite));
                    Debug.Log($"[BgFadeTest] Fade 完成: {nextName}");
                }
                else
                {
                    Debug.LogError($"[BgFadeTest] 加载失败，跳过: {nextName}");
                }

                yield return new WaitForSeconds(switchInterval);
            }
        }

        IEnumerator DoFade(Image img, Sprite newSprite)
        {
            Color startColor = img.color;
            float elapsed = 0f;

            // 淡出到黑色
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                img.color = Color.Lerp(startColor, Color.black, elapsed / fadeDuration);
                yield return null;
            }

            img.sprite = newSprite;
            img.color = Color.black;
            elapsed = 0f;

            // 从黑色淡入
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                img.color = Color.Lerp(Color.black, Color.white, elapsed / fadeDuration);
                yield return null;
            }

            img.color = Color.white;
        }

        Sprite LoadSprite(string name)
        {
            // 先尝试 Resources
            Sprite s = Resources.Load<Sprite>("CG/" + name);
            if (s != null) return s;
            s = Resources.Load<Sprite>("Backgrounds/" + name);
            if (s != null) return s;

#if UNITY_EDITOR
            // Editor 下从 Art 路径加载
            string[] paths = new string[]
            {
                $"Assets/Art/Characters/cg/{name}.png",
                $"Assets/Art/DialogueTestArt/{name}.png",
                $"Assets/Art/Scene/{name}.png",
                $"Assets/Art/{name}.png"
            };
            foreach (var p in paths)
            {
                s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (s != null) return s;
            }
#endif
            return null;
        }
    }
}
