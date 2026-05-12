using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Fungus;

namespace Game.Test
{
    [System.Serializable]
    public class CharacterMapping
    {
        public string name;
        public Character fungusCharacter;
    }

    [System.Serializable]
    public class SpriteMapping
    {
        public string name;
        public Sprite sprite;
    }

    [System.Serializable]
    public class AudioMapping
    {
        public string name;
        public AudioClip clip;
    }

    /// <summary>
    /// 独立 Fungus 视觉小说驱动器。
    /// 所有 CG 统一作为背景图处理，无立绘功能。
    /// 支持快进按钮、选项按钮点击选择。
    /// </summary>
    public class FungusVNController : MonoBehaviour
    {
        [Header("对话文本")]
        public TextAsset dialogueText;

        [Header("Fungus 组件（为空则运行时自动查找/创建）")]
        public SayDialog sayDialog;

        [Header("背景设置（为空则运行时自动创建）")]
        public Image backgroundImage;   // 后景（向后兼容）
        public Image midgroundImage;    // 中景
        public Image foregroundImage;   // 前景
        private Image backdropImage;    // 黑色兜底层（转场时提供黑色背景）

        [Header("音频源（为空则运行时自动添加）")]
        public AudioSource sfxSource;
        public AudioSource bgmSource;

        [Header("对话框遮罩（全屏渐变底图）")]
        public Sprite dialogMask;

        [Header("素材映射（Inspector 静态配置）")]
        public List<CharacterMapping> characters = new List<CharacterMapping>();
        public List<SpriteMapping> backgrounds = new List<SpriteMapping>();
        public List<AudioMapping> soundEffects = new List<AudioMapping>();
        public List<AudioMapping> bgmTracks = new List<AudioMapping>();

        [Header("快进配置")]
        public float fastForwardInterval = 0.1f;

        [Header("Fungus 存档")]
        public Flowchart saveFlowchart; // VN_SaveFlowchart

        private List<DialogueLine> lines;
        private int lineIndex;
        private bool isProcessing;
        private bool isChoosing;
        private DialogueLine currentChoiceLine;
        private bool isFastForwarding;
        private Canvas vnCanvas;

        // 段落跳转索引：段落名 → lines 中的起始索引
        private Dictionary<string, int> blockStartIndices = new Dictionary<string, int>();

        /// <summary>剧情结束后的分支类型（供流程控制器读取）</summary>
        public VNExitType exitType { get; private set; } = VNExitType.None;

        /// <summary>剧情全部结束后的回调（带分支类型）</summary>
        public System.Action<VNExitType> OnDialogueExit;

        // UI 按钮
        private Button ffButton;
        private Button skipButton;
        private GameObject choicePanel;
        private Button choiceBtn1;
        private Button choiceBtn2;
        private Text choiceText1;
        private Text choiceText2;

        // 素材缺失占位符
        private Text placeholderText;
        private HashSet<string> missingAssetLogs = new HashSet<string>();

        /// <summary>剧情全部结束后的回调（供流程控制器订阅，兼容旧代码）</summary>
        public System.Action OnDialogueComplete;

        void Awake()
        {
            EnsureComponents();
            CreateUI();
        }

        void Start()
        {
            RestartDialogue();
        }

        /// <summary>
        /// 重新启动对话（供 LevelFlowCoordinator 在切换 opening/ending 文本时调用）
        /// </summary>
        public void RestartDialogue()
        {
            if (dialogueText == null)
            {
                Debug.LogError("[FungusVNController] dialogueText 未赋值");
                return;
            }

            // 重置状态
            hasEnded = false;
            exitType = VNExitType.None;
            isProcessing = false;
            isChoosing = false;
            currentChoiceLine = null;

            lines = DialogueParser.Parse(dialogueText);
            BuildBlockIndex();

            // 保险：确保 SetupDialogMask 在 sayDialog 有效后再执行一次
            if (sayDialog != null)
                SetupDialogMask();

            // 检查是否有 Fungus 存档需要恢复
            if (saveFlowchart != null)
            {
                bool isOpeningDone = saveFlowchart.GetBooleanVariable("VN_IsOpeningDone");
                string savedFile = saveFlowchart.GetStringVariable("VN_StoryFile");

                // 若开场剧情已看完且匹配当前文件，直接结束（由 LevelFlowCoordinator 进入 gameplay）
                if (isOpeningDone && dialogueText.name == savedFile)
                {
                    Debug.Log("[FungusVNController] 开场剧情已看完，跳过剧情");
                    EndDialogue();
                    return;
                }

                int savedLine = saveFlowchart.GetIntegerVariable("VN_LineIndex");
                if (savedLine > 0 && dialogueText.name == savedFile)
                {
                    lineIndex = savedLine;
                    Debug.Log($"[FungusVNController] 从 Fungus 存档恢复，行号: {lineIndex}");
                }
                else
                {
                    lineIndex = 0;
                }
            }
            else
            {
                lineIndex = 0;
            }

            ShowNextLine();
        }

        /// <summary>扫描 lines 建立段落名到行号的索引</summary>
        void BuildBlockIndex()
        {
            blockStartIndices.Clear();
            if (lines == null) return;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].type == "段落" && !string.IsNullOrEmpty(lines[i].content))
                {
                    blockStartIndices[lines[i].content] = i + 1; // 跳到段落标记的下一行
                }
            }
        }

        /// <summary>切换快进模式（供按钮调用）</summary>
        public void ToggleFastForward()
        {
            isFastForwarding = !isFastForwarding;
            UpdateFFButtonVisual();

            // 开启快进时，如果当前正在等待输入，立即推进
            if (isFastForwarding && sayDialog != null)
            {
                var writer = sayDialog.GetComponent<Writer>();
                if (writer != null && writer.IsWaitingForInput)
                {
                    writer.OnNextLineEvent();
                }
            }

            Debug.Log($"[FungusVNController] 快进: {isFastForwarding}");
        }

        /// <summary>运行时自动查找/创建所有必需的组件</summary>
        void EnsureComponents()
        {
            // 1. 确保有 Canvas（用于背景）
            vnCanvas = FindObjectOfType<Canvas>();
            if (vnCanvas == null)
            {
                var canvasGo = new GameObject("VNCanvas");
                vnCanvas = canvasGo.AddComponent<Canvas>();
                vnCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                vnCanvas.sortingOrder = -1;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // 2. SayDialog：如果 Inspector 未赋值，则实例化新的（不复用场景中已有的，避免遮罩设置到错误的对话框）
            if (sayDialog == null)
            {
                var prefab = Resources.Load<GameObject>("Prefabs/SayDialog");
                if (prefab != null)
                {
                    var go = Instantiate(prefab);
                    go.name = $"SayDialog_{gameObject.name}";
                    sayDialog = go.GetComponent<SayDialog>();
                    Debug.Log($"[FungusVNController] 已自动实例化 SayDialog: {go.name}");
                }
                else
                {
                    Debug.LogError("[FungusVNController] 未找到 SayDialog prefab。请确保 Fungus 已正确导入，或在 Inspector 中手动赋值。");
                }
            }

            // 3. 分层背景：查找或创建后/中/前 三层 Image
            EnsureLayerImage(ref backgroundImage, "VNBackground", -2);
            EnsureLayerImage(ref midgroundImage, "VNMidground", -1);
            EnsureLayerImage(ref foregroundImage, "VNForeground", 0);

            // 3.5 黑色兜底层：放在所有背景层之下，转场时透出黑色而非透明
            EnsureLayerImage(ref backdropImage, "VNBackdrop", -3);
            if (backdropImage != null)
            {
                backdropImage.color = Color.black;
                backdropImage.raycastTarget = false;
            }

            // 4. AudioSource
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
            }

            // 5. 字体设置：自动加载项目中文字体并应用到 SayDialog
            SetupFont();

            // 6. 对话框遮罩：将 dialog_dark.png 作为全屏渐变层
            SetupDialogMask();

            // 7. 人名居中
            SetupNameTextAlignment();

            // 8. 对话文字位置下调
            SetupStoryTextPosition();
        }

        void SetupFont()
        {
            if (sayDialog == null) return;

            // 尝试加载项目中文字体
            var chineseFont = Resources.Load<Font>("Font/文软雅黑");
            if (chineseFont == null)
            {
                // 尝试其他可能路径
                chineseFont = Resources.Load<Font>("Fonts/文软雅黑");
            }

            if (chineseFont != null)
            {
                // 设置 SayDialog 子对象中的 Text 字体
                var texts = sayDialog.GetComponentsInChildren<Text>(true);
                foreach (var txt in texts)
                {
                    txt.font = chineseFont;
                }
                Debug.Log("[FungusVNController] 已设置中文字体");
            }
        }

        void SetupDialogMask()
        {
            if (sayDialog == null || dialogMask == null) return;

            var canvas = sayDialog.GetComponent<Canvas>();
            if (canvas == null) return;

            // 查找是否已存在遮罩层
            var existing = canvas.transform.Find("DialogMask");
            if (existing != null) return;

            var maskGo = new GameObject("DialogMask");
            maskGo.transform.SetParent(canvas.transform, false);
            maskGo.transform.SetAsFirstSibling();

            var maskRect = maskGo.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            var maskImg = maskGo.AddComponent<Image>();
            maskImg.sprite = dialogMask;
            maskImg.color = Color.white;
            maskImg.raycastTarget = false;

            // 将 Panel 原有背景设为透明，避免遮挡遮罩层
            var panel = canvas.transform.Find("Panel");
            if (panel != null)
            {
                var panelImg = panel.GetComponent<Image>();
                if (panelImg != null)
                {
                    panelImg.sprite = null;
                    panelImg.color = new Color(1, 1, 1, 0);
                }
            }
        }

        void SetupNameTextAlignment()
        {
            if (sayDialog == null) return;

            var nameTextObj = sayDialog.gameObject.transform.Find("Panel/NameText");
            if (nameTextObj == null)
            {
                // 尝试直接查找
                var allTexts = sayDialog.GetComponentsInChildren<Text>(true);
                foreach (var t in allTexts)
                {
                    if (t.gameObject.name == "NameText")
                    {
                        t.alignment = TextAnchor.MiddleCenter;
                        return;
                    }
                }
                return;
            }

            var nameText = nameTextObj.GetComponent<Text>();
            if (nameText != null)
                nameText.alignment = TextAnchor.MiddleCenter;

            // 调整 NameText 在 Panel 中顶部居中
            var nameRect = nameTextObj.GetComponent<RectTransform>();
            if (nameRect != null)
            {
                nameRect.anchorMin = new Vector2(0.5f, 1f);
                nameRect.anchorMax = new Vector2(0.5f, 1f);
                nameRect.pivot = new Vector2(0.5f, 1f);
                nameRect.anchoredPosition = new Vector2(0f, -5f);
            }
        }

        /// <summary>调整对话文字位置，使其更靠下</summary>
        void SetupStoryTextPosition()
        {
            if (sayDialog == null) return;

            var storyTextObj = sayDialog.gameObject.transform.Find("Panel/StoryText");
            if (storyTextObj == null)
            {
                // 兜底遍历查找
                var allTexts = sayDialog.GetComponentsInChildren<Text>(true);
                foreach (var t in allTexts)
                {
                    if (t.gameObject.name == "StoryText")
                    {
                        storyTextObj = t.gameObject.transform;
                        break;
                    }
                }
            }

            if (storyTextObj == null) return;

            var storyRect = storyTextObj.GetComponent<RectTransform>();
            if (storyRect == null) return;

            // 在当前位置基础上向下偏移（y 值减小 = 更靠下）
            storyRect.anchoredPosition = new Vector2(
                storyRect.anchoredPosition.x,
                storyRect.anchoredPosition.y - 60f
            );
        }

        /// <summary>查找或创建某一层背景 Image</summary>
        void EnsureLayerImage(ref Image layerImage, string goName, int sortingOrder)
        {
            if (layerImage != null) return;

            var go = GameObject.Find(goName);
            if (go != null)
            {
                layerImage = go.GetComponent<Image>();
            }
            else
            {
                go = new GameObject(goName);
                go.transform.SetParent(vnCanvas.transform, false);
                var rect = go.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                layerImage = go.AddComponent<Image>();
                layerImage.color = Color.clear;
                layerImage.raycastTarget = false;

                var canvas = go.GetComponent<Canvas>();
                if (canvas == null) canvas = go.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = sortingOrder;
            }
        }

        /// <summary>设置某一层背景图（空值=清空）</summary>
        void SetLayerBackground(Image layerImage, string spriteName, string transition = null, float transitionHold = 0f)
        {
            if (layerImage == null) return;

            if (string.IsNullOrEmpty(spriteName))
            {
                layerImage.sprite = null;
                layerImage.color = Color.clear;
                return;
            }

            var mapping = backgrounds.Find(b => b.name == spriteName);
            Sprite targetSprite = null;
            if (mapping != null && mapping.sprite != null)
            {
                targetSprite = mapping.sprite;
            }
            else
            {
                targetSprite = LoadBgSprite(spriteName);
            }

            if (targetSprite == null)
            {
                ShowPlaceholder("Image", spriteName);
                return;
            }

            switch (transition)
            {
                case "fade":
                    StartCoroutine(BackgroundFadeTransition(layerImage, targetSprite, transitionHold));
                    break;
                case "slide":
                    StartCoroutine(BackgroundSlideTransition(layerImage, targetSprite));
                    break;
                case "crossfade":
                    StartCoroutine(BackgroundCrossfadeTransition(layerImage, targetSprite));
                    break;
                default:
                    if (!string.IsNullOrEmpty(transition))
                    {
                        Debug.Log($"[VNTransitionTest] 转场 '{transition}' 尚未实现，使用直切。");
                    }
                    layerImage.sprite = targetSprite;
                    layerImage.color = Color.white;
                    break;
            }
        }

        IEnumerator BackgroundFadeTransition(Image layerImage, Sprite newSprite, float holdDuration = 0f)
        {
            float duration = 0.5f;
            Color startColor = layerImage.color;

            // 淡出到透明（底层黑色 Backdrop 透出）
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                layerImage.color = Color.Lerp(startColor, Color.clear, t / duration);
                yield return null;
            }

            layerImage.sprite = newSprite;
            layerImage.color = Color.clear;

            // 黑屏停留（fadehold 模式）
            if (holdDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(holdDuration);
            }

            // 淡入到白色
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                layerImage.color = Color.Lerp(Color.clear, Color.white, t / duration);
                yield return null;
            }

            layerImage.color = Color.white;
        }

        IEnumerator BackgroundSlideTransition(Image layerImage, Sprite newSprite)
        {
            float duration = 0.5f;
            RectTransform rt = layerImage.rectTransform;
            Vector2 centerPos = Vector2.zero;
            float screenW = Screen.width;

            // 划出旧背景（向左）
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                rt.anchoredPosition = Vector2.Lerp(centerPos, centerPos + Vector2.left * screenW, t / duration);
                yield return null;
            }

            // 切换图片并放到右侧外
            layerImage.sprite = newSprite;
            rt.anchoredPosition = centerPos + Vector2.right * screenW;

            // 划入新背景（从右到左回到中心）
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                rt.anchoredPosition = Vector2.Lerp(centerPos + Vector2.right * screenW, centerPos, t / duration);
                yield return null;
            }

            rt.anchoredPosition = centerPos;
        }

        IEnumerator BackgroundCrossfadeTransition(Image layerImage, Sprite newSprite)
        {
            float duration = 0.5f;

            // 创建临时全屏 Image 放新图，放在当前层之上
            GameObject tempGO = new GameObject("TempCrossfade");
            tempGO.transform.SetParent(layerImage.transform.parent, false);
            RectTransform tempRect = tempGO.AddComponent<RectTransform>();
            tempRect.anchorMin = Vector2.zero;
            tempRect.anchorMax = Vector2.one;
            tempRect.offsetMin = Vector2.zero;
            tempRect.offsetMax = Vector2.zero;
            Image tempImg = tempGO.AddComponent<Image>();
            tempImg.sprite = newSprite;
            tempImg.color = Color.clear;
            tempImg.raycastTarget = false;

            // 设置临时层 sorting order 略高于当前层
            Canvas tempCanvas = tempGO.GetComponent<Canvas>();
            if (tempCanvas == null) tempCanvas = tempGO.AddComponent<Canvas>();
            tempCanvas.overrideSorting = true;
            Canvas layerCanvas = layerImage.GetComponent<Canvas>();
            tempCanvas.sortingOrder = (layerCanvas != null) ? layerCanvas.sortingOrder + 1 : 1;

            // 交叉溶解：旧图淡出同时新图淡入
            Color oldStart = layerImage.color;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                float p = t / duration;
                layerImage.color = Color.Lerp(oldStart, Color.clear, p);
                tempImg.color = Color.Lerp(Color.clear, Color.white, p);
                yield return null;
            }

            // 结束：将新图切回正式层，清理临时对象
            layerImage.sprite = newSprite;
            layerImage.color = Color.white;
            Destroy(tempGO);
        }

        Sprite LoadBgSprite(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            Sprite s = Resources.Load<Sprite>("CG/" + name);
            if (s == null) s = Resources.Load<Sprite>("Backgrounds/" + name);
            if (s == null) s = Resources.Load<Sprite>("UI/Dialogue/Backgrounds/" + name);

            return s;
        }

        /// <summary>解析颜色字符串（#RRGGBB 或命名颜色）</summary>
        Color ParseSolidColor(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr)) return Color.clear;

            if (colorStr.StartsWith("#") && colorStr.Length == 7)
            {
                if (ColorUtility.TryParseHtmlString(colorStr, out Color c))
                    return c;
            }

            switch (colorStr.ToLower())
            {
                case "black": return Color.black;
                case "white": return Color.white;
                case "red": return Color.red;
                case "green": return Color.green;
                case "blue": return Color.blue;
                case "yellow": return Color.yellow;
                case "cyan": return Color.cyan;
                case "magenta": return Color.magenta;
                case "gray": return Color.gray;
                case "grey": return Color.grey;
                case "clear":
                case "transparent": return Color.clear;
                default:
                    Debug.LogWarning($"[FungusVNController] 未知颜色名称: {colorStr}");
                    return Color.clear;
            }
        }

        /// <summary>应用纯色背景：清空所有图片层，后景设为指定颜色</summary>
        void ApplySolidBackground(string colorStr, string transition, float transitionHold = 0f)
        {
            Color c = ParseSolidColor(colorStr);

            // 清空所有层的图片
            if (backgroundImage != null)  { backgroundImage.sprite = null; }
            if (midgroundImage != null)   { midgroundImage.sprite = null; }
            if (foregroundImage != null)  { foregroundImage.sprite = null; }

            if (backgroundImage != null)
            {
                if (transition == "fade")
                {
                    StartCoroutine(FadeToSolidBackground(c, transitionHold));
                }
                else
                {
                    backgroundImage.color = c;
                    if (midgroundImage != null) midgroundImage.color = Color.clear;
                    if (foregroundImage != null) foregroundImage.color = Color.clear;
                }
            }
        }

        IEnumerator FadeToSolidBackground(Color targetColor, float holdDuration = 0f)
        {
            float duration = 0.5f;
            float elapsed = 0f;

            Color startBg = backgroundImage != null ? backgroundImage.color : Color.clear;
            Color startMid = midgroundImage != null ? midgroundImage.color : Color.clear;
            Color startFg = foregroundImage != null ? foregroundImage.color : Color.clear;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (backgroundImage != null)
                {
                    backgroundImage.color = Color.Lerp(startBg, targetColor, t);
                }
                if (midgroundImage != null)
                {
                    midgroundImage.color = Color.Lerp(startMid, Color.clear, t);
                }
                if (foregroundImage != null)
                {
                    foregroundImage.color = Color.Lerp(startFg, Color.clear, t);
                }

                yield return null;
            }

            // 黑屏停留（fadehold 模式）
            if (holdDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(holdDuration);
            }

            if (backgroundImage != null) backgroundImage.color = targetColor;
            if (midgroundImage != null) midgroundImage.color = Color.clear;
            if (foregroundImage != null) foregroundImage.color = Color.clear;
        }

        /// <summary>创建快进按钮和选项面板</summary>
        void CreateUI()
        {
            if (vnCanvas == null) return;

            // === 存档按钮（右上角）===
            var saveBtn = CreateButton("SaveBtn", vnCanvas.transform,
                new Vector2(-30, -30), new Vector2(100, 45), "存档",
                () => SaveProgress());
            saveBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            saveBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            saveBtn.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
            saveBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-30, -30);

            // === 跳过按钮（存档按钮左侧，开场剧情时显示）===
            skipButton = CreateButton("SkipBtn", vnCanvas.transform,
                new Vector2(-140, -30), new Vector2(100, 45), "跳过",
                () => OnSkipStory());
            skipButton.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            skipButton.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            skipButton.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
            skipButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-140, -30);
            skipButton.gameObject.SetActive(false);

            // === 选项面板（屏幕中央）===
            choicePanel = new GameObject("ChoicePanel");
            choicePanel.transform.SetParent(vnCanvas.transform, false);
            var panelRect = choicePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(600, 320);
            choicePanel.SetActive(false);

            // 按钮1（上排大按钮）
            choiceBtn1 = CreateButton("ChoiceBtn1", choicePanel.transform,
                new Vector2(0, 90), new Vector2(520, 100), "选项1",
                () => OnChoose(1));

            // 按钮2（下排大按钮）
            choiceBtn2 = CreateButton("ChoiceBtn2", choicePanel.transform,
                new Vector2(0, -90), new Vector2(520, 100), "选项2",
                () => OnChoose(2));

            choiceText1 = choiceBtn1.GetComponentInChildren<Text>();
            choiceText2 = choiceBtn2.GetComponentInChildren<Text>();

            // === 素材缺失占位符（左上角）===
            var phGo = new GameObject("PlaceholderText");
            phGo.transform.SetParent(vnCanvas.transform, false);
            var phRect = phGo.AddComponent<RectTransform>();
            phRect.anchorMin = new Vector2(0, 1);
            phRect.anchorMax = new Vector2(0, 1);
            phRect.pivot = new Vector2(0, 1);
            phRect.anchoredPosition = new Vector2(10, -10);
            phRect.sizeDelta = new Vector2(600, 200);
            placeholderText = phGo.AddComponent<Text>();
            placeholderText.fontSize = 18;
            placeholderText.color = Color.yellow;
            placeholderText.alignment = TextAnchor.UpperLeft;
            placeholderText.raycastTarget = false;

            // 描边：黑色描边，避免与背景撞色
            var outline = phGo.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var phCanvas = phGo.AddComponent<Canvas>();
            phCanvas.overrideSorting = true;
            phCanvas.sortingOrder = 999;
            phGo.SetActive(false);
        }

        Button CreateButton(string name, Transform parent, Vector2 anchoredPos, Vector2 size, string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            // 文字子对象
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            var txt = textGo.AddComponent<Text>();
            txt.text = label;
            txt.fontSize = 26;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;

            return btn;
        }

        void UpdateFFButtonVisual()
        {
            if (ffButton == null) return;
            var img = ffButton.GetComponent<Image>();
            var txt = ffButton.GetComponentInChildren<Text>();
            if (isFastForwarding)
            {
                img.color = new Color(0.8f, 0.3f, 0.1f, 0.9f); // 橙红色表示快进中
                txt.text = "快进中";
            }
            else
            {
                img.color = new Color(0.15f, 0.15f, 0.15f, 0.85f); // 恢复默认
                txt.text = "快进";
            }
        }

        /// <summary>显示素材缺失占位符（左上角）</summary>
        void ShowPlaceholder(string assetType, string assetName)
        {
            string key = $"{assetType}:{assetName}";
            if (missingAssetLogs.Contains(key)) return;
            missingAssetLogs.Add(key);

            if (placeholderText != null)
            {
                placeholderText.gameObject.SetActive(true);
                placeholderText.text += $"{assetType} not found: {assetName}\n";
            }
            Debug.LogWarning($"[FungusVNController] {assetType} not found: {assetName}");
        }

        /// <summary>清除素材缺失占位符</summary>
        public void ClearPlaceholder()
        {
            missingAssetLogs.Clear();
            if (placeholderText != null)
            {
                placeholderText.text = "";
                placeholderText.gameObject.SetActive(false);
            }
        }

        /// <summary>手动保存当前剧情进度（使用 Fungus SaveManager）</summary>
        public void SaveProgress()
        {
            if (saveFlowchart == null)
            {
                Debug.LogWarning("[FungusVNController] saveFlowchart 未赋值，无法存档");
                return;
            }

            UpdateSaveVars();

            var saveManager = FungusManager.Instance.SaveManager;
            saveManager.AddSavePoint("ManualSave", $"手动存档 - {dialogueText.name} 行{lineIndex}");
            saveManager.Save("vn_save");
            Debug.Log($"[FungusVNController] Fungus 存档已保存: {dialogueText.name} 行{lineIndex}");
        }

        /// <summary>从 Fungus 存档恢复进度</summary>
        public void LoadProgress()
        {
            var saveManager = FungusManager.Instance.SaveManager;
            if (!saveManager.SaveDataExists("vn_save"))
            {
                Debug.Log("[FungusVNController] 无存档，从头开始");
                return;
            }

            // Fungus Load 会重新加载场景并恢复 Flowchart 变量
            // 场景加载完成后 Start() 中读取 Flowchart 变量恢复 lineIndex
            saveManager.Load("vn_save");
        }

        /// <summary>更新 Flowchart 存档变量</summary>
        void UpdateSaveVars()
        {
            if (saveFlowchart == null) return;
            saveFlowchart.SetIntegerVariable("VN_LineIndex", lineIndex);
            saveFlowchart.SetStringVariable("VN_StoryFile", dialogueText != null ? dialogueText.name : "");
        }

        /// <summary>跳过当前剧情（直接结束）</summary>
        void OnSkipStory()
        {
            Debug.Log("[FungusVNController] 用户跳过剧情");
            EndDialogue();
        }

        /// <summary>设置跳过按钮显隐（供 LevelFlowCoordinator 调用）</summary>
        public void SetSkipButtonVisible(bool visible)
        {
            if (skipButton != null)
                skipButton.gameObject.SetActive(visible);
        }

        /// <summary>推进到下一行对话</summary>
        void ShowNextLine()
        {
            if (lines == null || lineIndex >= lines.Count)
            {
                EndDialogue();
                return;
            }

            if (isProcessing) return;
            isProcessing = true;

            var line = lines[lineIndex++];

            // 段落标记行直接跳过
            if (line.type == "段落")
            {
                isProcessing = false;
                ShowNextLine();
                return;
            }

            UpdateSaveVars(); // 每行切换后更新 Flowchart 存档变量

            // ========== 分支动作指令（最高优先级）==========
            if (!string.IsNullOrEmpty(line.action))
            {
                ExecuteAction(line.action);
                return;
            }

            // ========== 分层背景处理 ==========
            if (line.isLayerCommand)
            {
                // [layer:...] 全换指令：显式指定的层才更新（null=未指定，保持原样；空字符串=清空）
                if (line.bg != null) SetLayerBackground(backgroundImage, line.bg, line.transition, line.transitionHold);
                if (line.mg != null) SetLayerBackground(midgroundImage, line.mg, line.transition, line.transitionHold);
                if (line.fg != null) SetLayerBackground(foregroundImage, line.fg, line.transition, line.transitionHold);
            }
            else
            {
                // 单换指令：有值才更新
                if (!string.IsNullOrEmpty(line.bg)) SetLayerBackground(backgroundImage, line.bg, line.transition, line.transitionHold);
                if (!string.IsNullOrEmpty(line.mg)) SetLayerBackground(midgroundImage, line.mg, line.transition, line.transitionHold);
                if (!string.IsNullOrEmpty(line.fg)) SetLayerBackground(foregroundImage, line.fg, line.transition, line.transitionHold);
            }

            // ========== 纯色背景处理 ==========
            if (!string.IsNullOrEmpty(line.solid))
            {
                ApplySolidBackground(line.solid, line.transition, line.transitionHold);
            }

            if (!string.IsNullOrEmpty(line.se))
            {
                var se = soundEffects.Find(s => s.name == line.se);
                if (se != null && se.clip != null && sfxSource != null)
                    sfxSource.PlayOneShot(se.clip);
                else
                    ShowPlaceholder("SFX", line.se);
            }

            if (!string.IsNullOrEmpty(line.bgm))
            {
                var bgm = bgmTracks.Find(b => b.name == line.bgm);
                if (bgm != null && bgm.clip != null && bgmSource != null)
                {
                    bgmSource.clip = bgm.clip;
                    bgmSource.Play();
                }
                else
                {
                    ShowPlaceholder("BGM", line.bgm);
                }
            }

            // ========== 等待指令 ==========
            if (line.wait > 0)
            {
                StartCoroutine(WaitAndContinue(line.wait));
                return;
            }

            // ========== 黑屏检测 ==========
            if (!string.IsNullOrEmpty(line.content) && line.content.Contains("黑屏"))
            {
                var blackSprite = Resources.Load<Sprite>("UI/Background/Bg_Black");
                if (blackSprite != null)
                {
                    if (backgroundImage != null)  { backgroundImage.sprite = blackSprite;  backgroundImage.color = Color.white; }
                    if (midgroundImage != null)   { midgroundImage.sprite = blackSprite;   midgroundImage.color = Color.white; }
                    if (foregroundImage != null)  { foregroundImage.sprite = blackSprite;  foregroundImage.color = Color.white; }
                }
            }

            // ========== 选项分支 ==========
            if (line.type == "选项")
            {
                ShowChoice(line);
                return;
            }

            // ========== 文本显示（通过 Fungus SayDialog）==========
            if (line.type != "指令")
            {
                string speaker = line.speaker;
                if (line.type == "旁白" || line.type == "场景")
                    speaker = "陆萤";

                var charMapping = characters.Find(c => c.name == speaker);
                if (charMapping != null && charMapping.fungusCharacter != null)
                    sayDialog.SetCharacter(charMapping.fungusCharacter);
                else
                {
                    sayDialog.SetCharacterName(speaker, Color.white);
                    if (line.type == "对话")
                        ShowPlaceholder("Character", speaker);
                }

                sayDialog.gameObject.SetActive(true);

                if (isFastForwarding)
                {
                    // 快进模式：瞬间显示，自动推进
                    sayDialog.Say(line.content, true, false, false, true, false, null, null);
                    StartCoroutine(FastForwardSkipRoutine());
                }
                else
                {
                    // 正常模式：打字机 + 等待输入
                    sayDialog.Say(line.content, true, true, false, true, false, null, () =>
                    {
                        isProcessing = false;
                        ShowNextLine();
                    });
                }
            }
            else
            {
                // 纯指令行，直接推进
                isProcessing = false;
                ShowNextLine();
            }
        }

        void ShowChoice(DialogueLine line)
        {
            isChoosing = true;
            currentChoiceLine = line;
            isProcessing = false;

            sayDialog.SetCharacter(null);
            sayDialog.NameText = "";
            sayDialog.gameObject.SetActive(true);
            sayDialog.StoryText = "";

            // 设置按钮文本并显示
            if (choiceText1 != null)
                choiceText1.text = string.IsNullOrEmpty(line.choice1) ? "..." : line.choice1;
            if (choiceText2 != null)
                choiceText2.text = string.IsNullOrEmpty(line.choice2) ? "..." : line.choice2;

            if (choicePanel != null)
                choicePanel.SetActive(true);
        }

        void OnChoose(int choice)
        {
            isChoosing = false;
            if (choicePanel != null)
                choicePanel.SetActive(false);

            if (currentChoiceLine == null) return;

            // 发送剧情选择事件，记录结局分支
            string choiceId = choice == 1 ? currentChoiceLine.choice1Id : currentChoiceLine.choice2Id;
            if (!string.IsNullOrEmpty(choiceId))
            {
                EventCenter.Instance.EventTrigger(E_EventType.StoryChoiceMade, choiceId);
            }

            string jumpTarget = choice == 1 ? currentChoiceLine.choice1JumpTarget : currentChoiceLine.choice2JumpTarget;
            string action = choice == 1 ? currentChoiceLine.choice1Action : currentChoiceLine.choice2Action;

            // 优先处理跳转目标
            if (!string.IsNullOrEmpty(jumpTarget))
            {
                if (blockStartIndices.TryGetValue(jumpTarget, out int targetIndex))
                {
                    lineIndex = targetIndex;
                    isProcessing = false;
                    ShowNextLine();
                    return;
                }
                else
                {
                    Debug.LogWarning($"[FungusVNController] 未找到跳转段落: {jumpTarget}");
                }
            }

            string result = choice == 1 ? currentChoiceLine.choice1Result : currentChoiceLine.choice2Result;

            if (!string.IsNullOrEmpty(result))
            {
                sayDialog.SetCharacter(null);
                sayDialog.NameText = "";
                sayDialog.gameObject.SetActive(true);

                System.Action onResultDone = () =>
                {
                    if (!string.IsNullOrEmpty(action))
                        ExecuteAction(action);
                    else
                        EndDialogue();
                };

                if (isFastForwarding)
                {
                    sayDialog.Say(result, true, false, false, true, false, null, null);
                    StartCoroutine(FastForwardChoiceRoutine(action));
                }
                else
                {
                    sayDialog.Say(result, true, true, false, true, false, null, () =>
                    {
                        onResultDone?.Invoke();
                    });
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(action))
                    ExecuteAction(action);
                else
                    EndDialogue();
            }
        }

        IEnumerator FastForwardChoiceRoutine(string action)
        {
            yield return new WaitForSeconds(0.02f);
            var writer = sayDialog.GetComponent<Writer>();
            if (writer != null && writer.IsWriting)
                writer.Stop();
            yield return new WaitForSeconds(fastForwardInterval);
            isProcessing = false;
            if (!string.IsNullOrEmpty(action))
                ExecuteAction(action);
            else
                ShowNextLine();
        }

        IEnumerator FastForwardSkipRoutine()
        {
            yield return new WaitForSeconds(0.02f);

            var writer = sayDialog.GetComponent<Writer>();
            if (writer != null && writer.IsWriting)
            {
                writer.Stop();
            }

            yield return new WaitForSeconds(fastForwardInterval);

            isProcessing = false;
            ShowNextLine();
        }

        IEnumerator WaitAndContinue(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            isProcessing = false;
            ShowNextLine();
        }

        /// <summary>执行分支动作指令</summary>
        void ExecuteAction(string action)
        {
            switch (action)
            {
                case "ending":
                    exitType = VNExitType.Ending;
                    EndDialogue();
                    break;
                case "gameplay":
                    exitType = VNExitType.Gameplay;
                    EndDialogue();
                    break;
                case "nextlevel":
                    exitType = VNExitType.NextLevel;
                    EndDialogue();
                    break;
                default:
                    Debug.LogWarning($"[FungusVNController] 未知动作指令: {action}");
                    isProcessing = false;
                    ShowNextLine();
                    break;
            }
        }

        private bool hasEnded = false;

        void EndDialogue()
        {
            if (hasEnded) return;
            hasEnded = true;

            if (choicePanel != null)
                choicePanel.SetActive(false);
            if (sayDialog != null)
                sayDialog.gameObject.SetActive(false);
            if (skipButton != null)
                skipButton.gameObject.SetActive(false);
            Debug.Log($"[FungusVNController] 对话结束，分支类型: {exitType}");
            OnDialogueExit?.Invoke(exitType);
            OnDialogueComplete?.Invoke();
        }
    }
}
