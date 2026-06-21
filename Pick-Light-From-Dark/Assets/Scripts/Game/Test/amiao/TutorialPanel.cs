using UnityEngine;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// 新手引导遮罩面板 — 全屏半透明遮罩 + 镂空区域 + 高亮边框 + 引导文案
    /// 使用4块矩形面板拼出镂空效果，不依赖自定义Shader
    /// </summary>
    public class TutorialPanel : MonoBehaviour
    {
        [Header("外观配置")]
        public Color maskColor = new Color(0, 0, 0, 0.7f);
        public Color borderColor = new Color(1, 0.85f, 0.2f, 1f);
        public float borderThickness = 3f;
        public float borderCornerRadius = 8f;

        [Header("文案配置")]
        public Font textFont;
        public int textFontSize = 28;
        public Color textColor = Color.white;

        // 内部组件
        private Canvas _canvas;
        private RectTransform _topMask, _bottomMask, _leftMask, _rightMask;
        private RectTransform _borderTop, _borderBottom, _borderLeft, _borderRight;
        private Text _instructionText;
        private RectTransform _fingerIndicator;
        private Image _clickCatcher;
        private Button _clickCatcherBtn;

        // 当前镂空区域（屏幕空间像素坐标，以屏幕中心为原点）
        private Rect _currentCutout;

        void Awake()
        {
            try
            {
                BuildUI();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TutorialPanel] Awake异常: {e}");
            }
        }

        /// <summary>构建遮罩UI结构</summary>
        void BuildUI()
        {
            // Canvas
            var canvasGo = gameObject;
            canvasGo.name = "TutorialCanvas";
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 999;
            _canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 根RectTransform
            var rootRect = GetComponent<RectTransform>();
            if (rootRect == null) rootRect = canvasGo.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // 4块遮罩
            _topMask    = CreateMaskPanel("TopMask");
            _bottomMask = CreateMaskPanel("BottomMask");
            _leftMask   = CreateMaskPanel("LeftMask");
            _rightMask  = CreateMaskPanel("RightMask");

            // 4条边框
            _borderTop    = CreateBorderLine("BorderTop");
            _borderBottom = CreateBorderLine("BorderBottom");
            _borderLeft   = CreateBorderLine("BorderLeft");
            _borderRight  = CreateBorderLine("BorderRight");

            // 引导文案
            _instructionText = CreateInstructionText();

            // 手指指示器
            _fingerIndicator = CreateFingerIndicator();

            // 全屏透明click catcher（AnyClick模式用，在遮罩之上）
            var catcherGo = new GameObject("ClickCatcher", typeof(RectTransform));
            catcherGo.transform.SetParent(transform, false);
            catcherGo.transform.SetAsLastSibling(); // 确保在最上层
            var catcherRt = catcherGo.GetComponent<RectTransform>();
            catcherRt.anchorMin = Vector2.zero;
            catcherRt.anchorMax = Vector2.one;
            catcherRt.offsetMin = Vector2.zero;
            catcherRt.offsetMax = Vector2.zero;
            _clickCatcher = catcherGo.AddComponent<Image>();
            _clickCatcher.color = new Color(0, 0, 0, 0);
            _clickCatcher.raycastTarget = false;
            _clickCatcherBtn = catcherGo.AddComponent<Button>();
            _clickCatcherBtn.transition = Selectable.Transition.None;
            _clickCatcherBtn.interactable = false;

            // 初始全隐藏
            HideAll();
        }

        RectTransform CreateMaskPanel(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = maskColor;
            img.raycastTarget = true; // 阻挡非镂空区域的点击
            return rt;
        }

        RectTransform CreateBorderLine(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = borderColor;
            img.raycastTarget = false;
            return rt;
        }

        Text CreateInstructionText()
        {
            var go = new GameObject("InstructionText", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(800, 100);

            var text = go.AddComponent<Text>();
            text.fontSize = textFontSize;
            text.color = textColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            // 描边
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(2, -2);

            if (textFont != null) text.font = textFont;
            else text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return text;
        }

        RectTransform CreateFingerIndicator()
        {
            var go = new GameObject("FingerIndicator", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(48, 48);

            var img = go.AddComponent<Image>();
            img.raycastTarget = false;

            // 尝试加载手指图标
            var fingerSprite = Resources.Load<Sprite>("UI/Tutorial/finger");
            if (fingerSprite != null)
            {
                img.sprite = fingerSprite;
                img.color = Color.white;
            }
            else
            {
                // 无手指图标资源时隐藏该指示器
                img.color = Color.clear;
            }

            return rt;
        }

        /// <summary>显示指定步骤的引导UI</summary>
        public void ShowStep(Rect targetPixelRect, string instruction, IndicatorType indicator, bool showMask = true)
        {
            // 防御：UI未初始化完成则跳过
            if (_topMask == null || _instructionText == null)
            {
                Debug.LogError("[TutorialPanel] UI未初始化，请检查Awake是否完整执行");
                return;
            }

            // 默认ClickTarget模式（遮罩阻挡非镂空区域），Manager会按需调用SetInteractionMode
            SetInteractionMode(true);

            // 遮罩可见性
            foreach (var mask in new[] { _topMask, _bottomMask, _leftMask, _rightMask,
                                         _borderTop, _borderBottom, _borderLeft, _borderRight })
            {
                if (mask != null) mask.gameObject.SetActive(showMask);
            }

            // 扩展镂空区域少许
            float pad = 24f;
            bool hasCutout = targetPixelRect.width > 1 && targetPixelRect.height > 1;
            var cutoutPixel = hasCutout
                ? new Rect(targetPixelRect.x - pad, targetPixelRect.y - pad,
                           targetPixelRect.width + pad * 2, targetPixelRect.height + pad * 2)
                : new Rect(0, 0, 0, 0);

            // 转为归一化坐标 (0-1) 用于 anchor 定位
            float sw = Screen.width;
            float sh = Screen.height;
            float leftN, rightN, bottomN, topN;

            if (hasCutout)
            {
                leftN   = Mathf.Clamp01(cutoutPixel.x / sw);
                rightN  = Mathf.Clamp01((cutoutPixel.x + cutoutPixel.width) / sw);
                bottomN = Mathf.Clamp01(cutoutPixel.y / sh);
                topN    = Mathf.Clamp01((cutoutPixel.y + cutoutPixel.height) / sh);
            }
            else
            {
                // 无镂空：全屏遮罩
                leftN = rightN = bottomN = topN = 0f;
            }

            _currentCutout = cutoutPixel;
            Debug.Log($"[TutorialPanel] 镂空像素: ({cutoutPixel.x:F0},{cutoutPixel.y:F0}) size({cutoutPixel.width:F0}x{cutoutPixel.height:F0}), " +
                      $"归一化: L={leftN:F2} R={rightN:F2} B={bottomN:F2} T={topN:F2}, 屏幕({sw}x{sh}), hasCutout={hasCutout}");

            // 4块遮罩用anchor拼出镂空
            _topMask.anchorMin = new Vector2(0, topN);
            _topMask.anchorMax = Vector2.one;
            _topMask.offsetMin = Vector2.zero;
            _topMask.offsetMax = Vector2.zero;

            _bottomMask.anchorMin = Vector2.zero;
            _bottomMask.anchorMax = new Vector2(1, bottomN);
            _bottomMask.offsetMin = Vector2.zero;
            _bottomMask.offsetMax = Vector2.zero;

            _leftMask.anchorMin = new Vector2(0, bottomN);
            _leftMask.anchorMax = new Vector2(leftN, topN);
            _leftMask.offsetMin = Vector2.zero;
            _leftMask.offsetMax = Vector2.zero;

            _rightMask.anchorMin = new Vector2(rightN, bottomN);
            _rightMask.anchorMax = new Vector2(1, topN);
            _rightMask.offsetMin = Vector2.zero;
            _rightMask.offsetMax = Vector2.zero;

            // 4条边框
            if (hasCutout)
            {
                PositionBorderLine(_borderTop,    leftN, rightN, topN,    topN,    borderThickness, false);
                PositionBorderLine(_borderBottom, leftN, rightN, bottomN, bottomN, borderThickness, false);
                PositionBorderLine(_borderLeft,   leftN, leftN,  bottomN, topN,    borderThickness, true);
                PositionBorderLine(_borderRight,  rightN, rightN, bottomN, topN,   borderThickness, true);
            }
            foreach (var b in new[] { _borderTop, _borderBottom, _borderLeft, _borderRight })
                if (b != null) b.gameObject.SetActive(hasCutout);

            // 文案始终居中
            _instructionText.text = instruction;
            _instructionText.gameObject.SetActive(!string.IsNullOrEmpty(instruction));
            _instructionText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _instructionText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _instructionText.rectTransform.anchoredPosition = Vector2.zero;

            // 指示器（无镂空时隐藏）
            if (hasCutout)
                UpdateIndicator(indicator, cutoutPixel);
            else if (_fingerIndicator != null)
                _fingerIndicator.gameObject.SetActive(false);

            gameObject.SetActive(true);
        }

        void PositionBorderLine(RectTransform rt, float anchorMinX, float anchorMaxX,
            float anchorMinY, float anchorMaxY, float thickness, bool isVertical)
        {
            rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
            rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            rt.offsetMin = isVertical ? new Vector2(-thickness / 2f, 0) : new Vector2(0, -thickness / 2f);
            rt.offsetMax = isVertical ? new Vector2( thickness / 2f, 0) : new Vector2(0,  thickness / 2f);
        }

        void UpdateIndicator(IndicatorType indicator, Rect cutoutPixel)
        {
            bool showFinger = indicator == IndicatorType.Finger || indicator == IndicatorType.Arrow;
            _fingerIndicator.gameObject.SetActive(showFinger);

            if (showFinger)
            {
                float sw = Screen.width;
                float sh = Screen.height;
                float fx = (cutoutPixel.x + cutoutPixel.width + 30) / sw;
                float fy = (cutoutPixel.y + cutoutPixel.height * 0.5f) / sh;
                _fingerIndicator.anchorMin = new Vector2(fx, fy);
                _fingerIndicator.anchorMax = new Vector2(fx, fy);
                _fingerIndicator.anchoredPosition = Vector2.zero;
                _fingerIndicator.localRotation = indicator == IndicatorType.Arrow
                    ? Quaternion.Euler(0, 0, -45)
                    : Quaternion.identity;
                StartCoroutine(BounceAnimation(_fingerIndicator));
            }
        }

        System.Collections.IEnumerator BounceAnimation(RectTransform target)
        {
            float elapsed = 0f;
            float baseY = target.anchoredPosition.y;
            while (target != null && target.gameObject.activeSelf)
            {
                elapsed += Time.unscaledDeltaTime * 2.5f;
                float offset = Mathf.Sin(elapsed) * 10f;
                target.anchoredPosition = new Vector2(target.anchoredPosition.x, baseY + offset);
                yield return null;
            }
        }

        /// <summary>设置交互模式</summary>
        /// <param name="blockOutsideCutout">true=遮罩阻挡镂空外的点击(ClickTarget); false=全屏可点击(AnyClick)</param>
        public void SetInteractionMode(bool blockOutsideCutout)
        {
            if (_clickCatcher == null || _clickCatcherBtn == null) return;

            if (blockOutsideCutout)
            {
                SetMaskRaycast(true);
                _clickCatcher.raycastTarget = false;
                _clickCatcherBtn.interactable = false;
            }
            else
            {
                SetMaskRaycast(false);
                _clickCatcher.raycastTarget = true;
                _clickCatcherBtn.interactable = true;
            }
        }

        void SetMaskRaycast(bool block)
        {
            foreach (var mask in new[] { _topMask, _bottomMask, _leftMask, _rightMask })
            {
                if (mask == null) continue;
                var img = mask.GetComponent<Image>();
                if (img != null) img.raycastTarget = block;
            }
        }

        /// <summary>获取click catcher的Button（供Manager绑定AnyClick回调）</summary>
        public Button ClickCatcherButton => _clickCatcherBtn;

        /// <summary>隐藏所有引导UI</summary>
        public void HideAll()
        {
            StopAllCoroutines();
            // 只隐藏遮罩和边框的GameObject（跳过click catcher）
            foreach (var mask in new[] { _topMask, _bottomMask, _leftMask, _rightMask,
                                         _borderTop, _borderBottom, _borderLeft, _borderRight })
            {
                if (mask != null) mask.gameObject.SetActive(false);
            }
            if (_instructionText != null)
                _instructionText.gameObject.SetActive(false);
            if (_fingerIndicator != null)
                _fingerIndicator.gameObject.SetActive(false);
        }

        /// <summary>获取当前镂空区域（屏幕坐标）</summary>
        public Rect GetCutoutRect() => _currentCutout;

        void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
