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

        [Header("背景 Canvas（为空则运行时自动创建）")]
        public Canvas vnCanvas;

        [Header("背景设置（为空则运行时自动创建）")]
        public Image backgroundImage;   // 后景
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

        // 段落跳转索引：段落名 → lines 中的起始索引
        private Dictionary<string, int> blockStartIndices = new Dictionary<string, int>();

        /// <summary>剧情结束后的分支类型（供流程控制器读取）</summary>
        public VNExitType exitType { get; private set; } = VNExitType.None;

        /// <summary>第一关跳过按钮是否跳到选项（否则直接结束剧情）</summary>
        public bool skipToChoiceIfAvailable = false;

        /// <summary>剧情开始时自动显示跳过按钮（供测试 Prefab 使用）</summary>
        public bool showSkipButtonOnStart = false;

        /// <summary>剧情全部结束后的回调（带分支类型）</summary>
        public System.Action<VNExitType> OnDialogueExit;

        // UI 按钮
        private Button ffButton;
        private Button skipButton;
        private Button advanceTapButton;
        private GameObject choicePanel;
        private Button choiceBtn1;
        private Button choiceBtn2;
        private Text choiceText1;
        private Text choiceText2;

        // 素材缺失占位符
        private Text placeholderText;
        private HashSet<string> missingAssetLogs = new HashSet<string>();

        // 居中大字显示
        private Text centerTextDisplay;

        // 文本位置调整标志（避免重复偏移）
        private bool namePositionAdjusted = false;
        private bool storyPositionAdjusted = false;

        // 各层正在运行的背景转场协程（避免多重协程冲突）
        private Coroutine bgTransitionRoutine;
        private Coroutine mgTransitionRoutine;
        private Coroutine fgTransitionRoutine;
        private Coroutine sayWatchdogRoutine;

        /// <summary>剧情全部结束后的回调（供流程控制器订阅，兼容旧代码）</summary>
        public System.Action OnDialogueComplete;

        void Awake()
        {
            EnsureComponents();
            CreateUI();
        }

        void Start()
        {
            // 若场景中有 LevelFlowCoordinator，由它统一管理 VN 启动，避免重复初始化
            if (Game.Flow.LevelFlowCoordinator.Instance == null)
            {
                RestartDialogue();
            }
            if (showSkipButtonOnStart)
                SetSkipButtonVisible(true);
        }

        /// <summary>
        /// 重新启动对话（供 LevelFlowCoordinator 在切换 opening/ending 文本时调用）
        /// </summary>
        /// <param name="forceFromStart">true=忽略 Flowchart 行号存档，从第 0 行开始（结尾剧情必须用）</param>
        public void RestartDialogue(bool forceFromStart = false)
        {
            if (dialogueText == null)
            {
                Debug.LogError("[FungusVNController] dialogueText 未赋值");
                return;
            }

            EnsureSaveFlowchart();

            // 停止所有可能正在运行的协程，防止重入时状态污染
            StopAllCoroutines();

            ClearWriterAndDialogueUI();

            // 重置状态
            hasEnded = false;
            exitType = VNExitType.None;
            isProcessing = false;
            isChoosing = false;
            isFastForwarding = false;
            currentChoiceLine = null;
            HideCenterText();

            if (sayDialog != null)
                sayDialog.gameObject.SetActive(true);

            if (vnCanvas != null)
                vnCanvas.gameObject.SetActive(true);

            // 保险：清理未使用的中景/前景层，避免旧图残留透显
            if (midgroundImage != null)  { midgroundImage.sprite = null;  midgroundImage.color = Color.clear; }
            if (foregroundImage != null) { foregroundImage.sprite = null; foregroundImage.color = Color.clear; }
            // 停罢所有转场协程
            if (bgTransitionRoutine != null) { StopCoroutine(bgTransitionRoutine); bgTransitionRoutine = null; }
            if (mgTransitionRoutine != null) { StopCoroutine(mgTransitionRoutine); mgTransitionRoutine = null; }
            if (fgTransitionRoutine != null) { StopCoroutine(fgTransitionRoutine); fgTransitionRoutine = null; }

            lines = DialogueParser.Parse(dialogueText);
            BuildBlockIndex();

            // 保险：确保 SetupDialogMask / DialogInput 在 sayDialog 有效后再执行一次（结尾 Restart 时必需）
            if (sayDialog != null)
            {
                SetupDialogMask();
                EnsureDialogInput();
            }

            // 检查是否有 Fungus 存档需要恢复
            if (saveFlowchart != null)
            {
                bool isOpeningDone = saveFlowchart.GetBooleanVariable("VN_IsOpeningDone");
                string savedFile = saveFlowchart.GetStringVariable("VN_StoryFile");

                // 若开场剧情已看完且匹配当前文件，直接结束（由 LevelFlowCoordinator 进入 gameplay）
                if (!forceFromStart && isOpeningDone && dialogueText.name == savedFile)
                {
                    Debug.Log("[FungusVNController] 开场剧情已看完，跳过剧情");
                    EndDialogue();
                    return;
                }

                int savedLine = saveFlowchart.GetIntegerVariable("VN_LineIndex");
                if (!forceFromStart && savedLine > 0 && dialogueText.name == savedFile)
                {
                    lineIndex = savedLine;
                    Debug.Log($"[FungusVNController] 从 Fungus 存档恢复，行号: {lineIndex}");
                }
                else
                {
                    lineIndex = 0;
                    if (forceFromStart)
                        Debug.Log($"[FungusVNController] 强制从头播放: {dialogueText.name}");
                }
            }
            else
            {
                lineIndex = 0;
            }

            // 新剧本第 0 行：清掉上一段 solid:black 残留，避免黑屏只显示名牌
            if (lineIndex == 0 && backgroundImage != null)
                backgroundImage.color = Color.white;

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

        void Update()
        {
            if (lines == null || hasEnded || isChoosing) return;
            if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return))
                return;

            // 交给 Fungus DialogInput / Writer 处理，不绕过 Say 协程
            ForwardDialogueInput();
        }

        /// <summary>把点击/空格转发给 SayDialog 上的 Fungus DialogInput（与预制体 clickMode 一致）</summary>
        void ForwardDialogueInput()
        {
            if (sayDialog == null) return;

            var dialogInput = sayDialog.GetComponent<DialogInput>();
            if (dialogInput != null)
            {
                if (Input.GetMouseButtonDown(0))
                    dialogInput.SetClickAnywhereClickedFlag();
                else
                    dialogInput.SetNextLineFlag();
                return;
            }

            var writer = sayDialog.GetComponent<Writer>();
            writer?.OnNextLineEvent();
        }

        /// <summary>运行时自动查找/创建所有必需的组件</summary>
        void EnsureComponents()
        {
            // 1. 确保有 Canvas（用于背景）
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
            else if (vnCanvas.transform.localScale.x == 0f || vnCanvas.transform.localScale.y == 0f)
            {
                vnCanvas.transform.localScale = Vector3.one;
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

            // 9. 确保 Fungus DialogInput 存在（负责把点击转给 Writer）
            EnsureDialogInput();
        }

        void EnsureDialogInput()
        {
            if (sayDialog == null) return;
            if (sayDialog.GetComponent<DialogInput>() == null)
            {
                sayDialog.gameObject.AddComponent<DialogInput>();
                Debug.Log("[FungusVNController] 已为 SayDialog 添加 DialogInput");
            }
            if (sayDialog.GetComponent<Writer>() == null)
                Debug.LogError("[FungusVNController] SayDialog 缺少 Writer 组件");
        }

        void SetupFont()
        {
            if (sayDialog == null) return;

            // 尝试加载项目中文字体
            var chineseFont = Resources.Load<Font>("Font/LXGWWenKaiScreen");
            if (chineseFont == null)
            {
                chineseFont = Resources.Load<Font>("Font/文软雅黑");
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
            if (sayDialog == null)
            {
                Debug.LogWarning("[SetupDialogMask] sayDialog 为 null，跳过");
                return;
            }
            if (dialogMask == null)
            {
                Debug.LogWarning("[SetupDialogMask] dialogMask 未赋值，跳过");
                return;
            }

            var canvas = sayDialog.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[SetupDialogMask] sayDialog 上没有 Canvas，跳过");
                return;
            }

            // 查找是否已存在遮罩层
            var existing = canvas.transform.Find("DialogMask");
            if (existing != null)
            {
                Debug.Log("[SetupDialogMask] DialogMask 已存在，跳过创建");
                return;
            }

            // 创建全屏遮罩层
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
            // 确保使用 Simple 模式，避免 Sliced 模式下 sprite 为 null 时的显示异常
            maskImg.type = Image.Type.Simple;

            Debug.Log("[SetupDialogMask] DialogMask 已创建并设置 sprite");

            // 将 Panel 及其子对象中的所有 Image 背景设为透明
            bool panelCleared = ClearPanelBackground(canvas.transform);
            if (!panelCleared)
            {
                Debug.LogWarning("[SetupDialogMask] 未找到 Panel，尝试查找其他背景对象...");
                // 兜底：尝试常见背景对象名
                string[] fallbackNames = { "Background", "Bg", "BackGround", "Panel" };
                foreach (var name in fallbackNames)
                {
                    var bg = canvas.transform.Find(name);
                    if (bg != null)
                    {
                        ClearImageRecursive(bg);
                        Debug.Log($"[SetupDialogMask] 已清除背景对象 '{name}'");
                        break;
                    }
                }
            }
        }

        /// <summary>清除 Panel 及其子对象中的所有 Image 背景</summary>
        bool ClearPanelBackground(Transform canvasTransform)
        {
            var panel = canvasTransform.Find("Panel");
            if (panel == null) return false;

            ClearImageRecursive(panel);
            Debug.Log("[SetupDialogMask] Panel 及其子对象的 Image 已设为透明");
            return true;
        }

        /// <summary>递归清除指定 Transform 及其子对象中的 Image 颜色</summary>
        void ClearImageRecursive(Transform target)
        {
            if (target == null) return;

            var img = target.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = null;
                img.color = new Color(1, 1, 1, 0);
                img.type = Image.Type.Simple;
            }

            foreach (Transform child in target)
            {
                ClearImageRecursive(child);
            }
        }

        void SetupNameTextAlignment()
        {
            if (sayDialog == null || namePositionAdjusted) return;

            var nameTextObj = sayDialog.gameObject.transform.Find("Panel/NameText");
            if (nameTextObj == null) return;

            var nameRect = nameTextObj.GetComponent<RectTransform>();
            if (nameRect != null)
            {
                nameRect.anchoredPosition += new Vector2(0f, 10f);
                namePositionAdjusted = true;
            }

            var tmp = nameTextObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmp != null)
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
        }

        void SetupStoryTextPosition()
        {
            if (sayDialog == null || storyPositionAdjusted) return;

            var storyTextObj = sayDialog.gameObject.transform.Find("Panel/StoryText");
            if (storyTextObj == null) return;

            var storyRect = storyTextObj.GetComponent<RectTransform>();
            if (storyRect != null)
            {
                storyRect.anchoredPosition += new Vector2(0f, -20f);
                storyPositionAdjusted = true;
            }
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

                // 保险：显式添加 CanvasRenderer，否则 Image 可能无法渲染
                var canvasRenderer = go.GetComponent<CanvasRenderer>();
                if (canvasRenderer == null) canvasRenderer = go.AddComponent<CanvasRenderer>();

                layerImage = go.AddComponent<Image>();
                layerImage.color = Color.clear;
                layerImage.raycastTarget = false;

                var canvas = go.GetComponent<Canvas>();
                if (canvas == null) canvas = go.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = sortingOrder;
            }
        }

        /// <summary>停罢指定层的转场协程</summary>
        void StopLayerTransition(Image layerImage)
        {
            if (layerImage == midgroundImage && mgTransitionRoutine != null) { StopCoroutine(mgTransitionRoutine); mgTransitionRoutine = null; }
            else if (layerImage == foregroundImage && fgTransitionRoutine != null) { StopCoroutine(fgTransitionRoutine); fgTransitionRoutine = null; }
            else if (layerImage == backgroundImage && bgTransitionRoutine != null) { StopCoroutine(bgTransitionRoutine); bgTransitionRoutine = null; }
        }

        /// <summary>启动指定层的转场协程</summary>
        void StartLayerTransition(Image layerImage, Coroutine cr)
        {
            if (layerImage == midgroundImage) mgTransitionRoutine = cr;
            else if (layerImage == foregroundImage) fgTransitionRoutine = cr;
            else bgTransitionRoutine = cr;
        }

        /// <summary>获取指定层是否有正在运行的协程</summary>
        bool IsLayerTransitioning(Image layerImage)
        {
            if (layerImage == midgroundImage) return mgTransitionRoutine != null;
            if (layerImage == foregroundImage) return fgTransitionRoutine != null;
            return bgTransitionRoutine != null;
        }

        /// <summary>设置某一层背景图（空值=清空）</summary>
        void SetLayerBackground(Image layerImage, string spriteName, string transition = null, float transitionHold = 0f)
        {
            if (layerImage == null) return;

            if (string.IsNullOrEmpty(spriteName))
            {
                StopLayerTransition(layerImage);
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

            Debug.Log($"[SetLayerBackground] 准备设置背景。层={(layerImage == backgroundImage ? "bg" : layerImage == midgroundImage ? "mg" : "fg")}, spriteName={spriteName}, transition={transition}, 当前sprite={(layerImage.sprite != null ? layerImage.sprite.name : "null")}, 当前color={layerImage.color}");

            // 若目标图与当前图相同，且非转场中，则跳过
            if (layerImage.sprite == targetSprite && !IsLayerTransitioning(layerImage))
            {
                Debug.Log($"[SetLayerBackground] 目标图与当前图相同且非转场中，跳过: {spriteName}");
                return;
            }

            // 停罢该层旧协程，避免多协程并改同一 Image
            StopLayerTransition(layerImage);

            switch (transition)
            {
                case "fade":
                    Debug.Log($"[SetLayerBackground] 启动 Fade 协程: {spriteName}");
                    StartLayerTransition(layerImage, StartCoroutine(BackgroundFadeTransition(layerImage, targetSprite, transitionHold)));
                    break;
                case "slide":
                    Debug.Log($"[SetLayerBackground] 启动 Slide 协程: {spriteName}");
                    StartLayerTransition(layerImage, StartCoroutine(BackgroundSlideTransition(layerImage, targetSprite)));
                    break;
                case "crossfade":
                    Debug.Log($"[SetLayerBackground] 启动 Crossfade 协程: {spriteName}");
                    StartLayerTransition(layerImage, StartCoroutine(BackgroundCrossfadeTransition(layerImage, targetSprite)));
                    break;
                default:
                    if (!string.IsNullOrEmpty(transition))
                    {
                        Debug.Log($"[FungusVNController] 转场 '{transition}' 尚未实现，使用直切。");
                    }
                    Debug.Log($"[SetLayerBackground] 直切设置 sprite: {spriteName}");
                    layerImage.sprite = targetSprite;
                    layerImage.color = Color.white;
                    break;
            }
        }

        IEnumerator BackgroundFadeTransition(Image layerImage, Sprite newSprite, float holdDuration = 0f)
        {
            float duration = 0.5f;
            Color startColor = layerImage.color;

            Debug.Log($"[BackgroundFadeTransition] 开始 fade。GameObject.active={layerImage.gameObject.activeSelf}, Image.enabled={layerImage.enabled}, startColor={startColor}, oldSprite={(layerImage.sprite != null ? layerImage.sprite.name : "null")}, newSprite={newSprite.name}");

            // 保险：确保目标对象处于活跃状态
            if (!layerImage.gameObject.activeSelf)
            {
                Debug.LogWarning("[BackgroundFadeTransition] layerImage GameObject 被禁用，强制启用");
                layerImage.gameObject.SetActive(true);
            }
            if (!layerImage.enabled)
            {
                Debug.LogWarning("[BackgroundFadeTransition] Image 组件被禁用，强制启用");
                layerImage.enabled = true;
            }

            // 淡出到黑色（而非透明）
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                layerImage.color = Color.Lerp(startColor, Color.black, t / duration);
                yield return null;
            }

            Debug.Log($"[BackgroundFadeTransition] 淡出到黑完成，切换 sprite → {newSprite.name}");
            layerImage.sprite = newSprite;
            layerImage.color = Color.black;

            // 黑屏停留（fadehold 模式）
            if (holdDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(holdDuration);
            }

            // 从黑色淡入到白色
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                layerImage.color = Color.Lerp(Color.black, Color.white, t / duration);
                yield return null;
            }

            layerImage.color = Color.white;
            Debug.Log($"[BackgroundFadeTransition] fade 完成。最终 color={layerImage.color}");

            // 协程结束，清理引用
            if (layerImage == backgroundImage) bgTransitionRoutine = null;
            else if (layerImage == midgroundImage) mgTransitionRoutine = null;
            else if (layerImage == foregroundImage) fgTransitionRoutine = null;
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

            // 协程结束，清理引用
            if (layerImage == backgroundImage) bgTransitionRoutine = null;
            else if (layerImage == midgroundImage) mgTransitionRoutine = null;
            else if (layerImage == foregroundImage) fgTransitionRoutine = null;
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

            // 协程结束，清理引用
            if (layerImage == backgroundImage) bgTransitionRoutine = null;
            else if (layerImage == midgroundImage) mgTransitionRoutine = null;
            else if (layerImage == foregroundImage) fgTransitionRoutine = null;
        }

        Sprite LoadBgSprite(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.Log("[LoadBgSprite] name 为空，返回 null");
                return null;
            }

            Sprite s = Resources.Load<Sprite>("CG/" + name);
            if (s != null) { Debug.Log($"[LoadBgSprite] 从 Resources/CG 找到: {name}"); return s; }

            s = Resources.Load<Sprite>("Backgrounds/" + name);
            if (s != null) { Debug.Log($"[LoadBgSprite] 从 Resources/Backgrounds 找到: {name}"); return s; }

            s = Resources.Load<Sprite>("UI/Dialogue/Backgrounds/" + name);
            if (s != null) { Debug.Log($"[LoadBgSprite] 从 Resources/UI/Dialogue/Backgrounds 找到: {name}"); return s; }

            s = Resources.Load<Sprite>("UI/Background/" + name);
            if (s != null) { Debug.Log($"[LoadBgSprite] 从 Resources/UI/Background 找到: {name}"); return s; }

#if UNITY_EDITOR
            if (s == null)
            {
                // 1. 先查常见硬编码路径（快路径）
                string[] artPaths = new string[]
                {
                    $"Assets/Art/Characters/cg/{name}.png",
                    $"Assets/Art/DialogueTestArt/{name}.png",
                    $"Assets/Art/Scene/{name}.png",
                    $"Assets/Art/{name}.png"
                };
                foreach (var path in artPaths)
                {
                    s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (s != null)
                    {
                        Debug.Log($"[LoadBgSprite] 从 Art 路径找到 Sprite: {path}");
                        return s;
                    }
                    Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (tex != null)
                    {
                        Debug.LogWarning($"[LoadBgSprite] {path} 是 Texture2D，非 Sprite。尝试动态创建 Sprite。");
                        s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        return s;
                    }
                }

                // 2. 递归扫描 Assets/Art 所有子目录（兜底）
                if (System.IO.Directory.Exists("Assets/Art"))
                {
                    var files = System.IO.Directory.GetFiles("Assets/Art", $"{name}.png", System.IO.SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        string path = file.Replace('\\', '/');
                        s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (s != null)
                        {
                            Debug.Log($"[LoadBgSprite] 从 Art 子目录递归找到 Sprite: {path}");
                            return s;
                        }
                        Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                        if (tex != null)
                        {
                            Debug.LogWarning($"[LoadBgSprite] {path} 是 Texture2D，非 Sprite。尝试动态创建 Sprite。");
                            s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                            return s;
                        }
                    }
                }
            }
#endif

            Debug.LogError($"[LoadBgSprite] 最终未找到任何匹配资源: {name}");
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

            // === 跳过按钮（右上角，开场剧情时显示）===
            skipButton = CreateButton("SkipBtn", vnCanvas.transform,
                new Vector2(-190, -30), new Vector2(100, 45), "跳过",
                () => OnSkipStory());
            skipButton.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            skipButton.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            skipButton.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
            skipButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-190, -30);
            skipButton.gameObject.SetActive(false);
            EnsureButtonOnTop(skipButton);

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
            var choiceCanvas = choicePanel.AddComponent<Canvas>();
            choiceCanvas.overrideSorting = true;
            choiceCanvas.sortingOrder = 200;
            choicePanel.AddComponent<GraphicRaycaster>();

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
            go.AddComponent<Game.UI.ChoiceButtonEffect>();

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
            txt.font = Resources.Load<Font>("Font/LXGWWenKaiScreen") ?? Resources.Load<Font>("Font/文软雅黑");

            var chineseFont = Resources.Load<Font>("Font/文软雅黑");
            if (chineseFont == null) chineseFont = Resources.Load<Font>("Fonts/文软雅黑");
            if (chineseFont == null && sayDialog != null)
            {
                var existingText = sayDialog.GetComponentInChildren<Text>(true);
                if (existingText != null) chineseFont = existingText.font;
            }
            if (chineseFont != null) txt.font = chineseFont;

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

        /// <summary>显示素材缺失占位符（左上角独立 Canvas，确保最前显示）</summary>
        void ShowPlaceholder(string assetType, string assetName)
        {
            string key = $"{assetType}:{assetName}";
            if (missingAssetLogs.Contains(key)) return;
            missingAssetLogs.Add(key);

            EnsurePlaceholderCanvas();

            if (placeholderText != null)
            {
                placeholderText.gameObject.SetActive(true);
                placeholderText.text += $"{assetType} not found: {assetName}\n";
            }
            Debug.LogWarning($"[FungusVNController] {assetType} not found: {assetName}");
        }

        /// <summary>确保占位文字有独立的 ScreenSpaceOverlay Canvas</summary>
        void EnsurePlaceholderCanvas()
        {
            if (placeholderText != null && placeholderText.gameObject != null) return;

            // 查找已有的独立占位 Canvas
            var existingCanvas = GameObject.Find("PlaceholderCanvas");
            Canvas canvas;
            if (existingCanvas != null)
            {
                canvas = existingCanvas.GetComponent<Canvas>();
            }
            else
            {
                var go = new GameObject("PlaceholderCanvas");
                canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;
                go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                go.AddComponent<GraphicRaycaster>();
            }

            var textGo = new GameObject("PlaceholderText");
            textGo.transform.SetParent(canvas.transform, false);
            var rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
            rect.sizeDelta = new Vector2(600, 200);

            textGo.AddComponent<CanvasRenderer>();
            placeholderText = textGo.AddComponent<Text>();
            placeholderText.fontSize = 18;
            placeholderText.color = Color.yellow;
            placeholderText.alignment = TextAnchor.UpperLeft;
            placeholderText.raycastTarget = false;

            var outline = textGo.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            textGo.SetActive(false);
        }

        /// <summary>清除素材缺失占位符</summary>
        public void ClearPlaceholder()
        {
            missingAssetLogs.Clear();
            if (placeholderText != null && placeholderText.gameObject != null)
            {
                placeholderText.text = "";
                placeholderText.gameObject.SetActive(false);
            }
        }

        /// <summary>动态加载音频剪辑（优先 Resources，Editor 下回退 AssetDatabase）</summary>
        AudioClip LoadAudioClip(string name, bool isBgm)
        {
            if (string.IsNullOrEmpty(name)) return null;

            string[] resPaths = isBgm
                ? new string[] { "Sound/BkMusic/" + name, "Audio/Music/" + name }
                : new string[] { "Sound/sound/" + name, "Sound/sound/DXH_SOUND/" + name, "Audio/SFX/" + name, "Audio/SFX/DXH_SOUND/" + name };

            foreach (var path in resPaths)
            {
                var clip = Resources.Load<AudioClip>(path);
                if (clip != null) return clip;
            }

#if UNITY_EDITOR
            string[] editorPaths = isBgm
                ? new string[] { $"Assets/Audio/Music/{name}.wav", $"Assets/Audio/Music/{name}.mp3", $"Assets/Resources/Sound/BkMusic/{name}.wav", $"Assets/Resources/Sound/BkMusic/{name}.mp3" }
                : new string[] { $"Assets/Audio/SFX/{name}.wav", $"Assets/Audio/SFX/{name}.mp3", $"Assets/Audio/SFX/DXH_SOUND/{name}.wav", $"Assets/Audio/SFX/DXH_SOUND/{name}.mp3", $"Assets/Resources/Sound/sound/{name}.wav", $"Assets/Resources/Sound/sound/{name}.mp3" };

            foreach (var path in editorPaths)
            {
                var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) return clip;
            }
#endif
            return null;
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

        /// <summary>当前是否正在播放 VN 剧本（未 EndDialogue）</summary>
        public bool IsStoryPlaying => lines != null && !hasEnded;

        /// <summary>跳过当前剧情（供测试快捷键等外部调用）</summary>
        public void SkipCurrentStory() => OnSkipStory();

        /// <summary>跳过当前剧情：优先跳到后续选项，无选项则结束并触发下一个 prefab</summary>
        void OnSkipStory()
        {
            Debug.Log("[FungusVNController] 用户跳过剧情");

            int choiceIndex = -1;
            for (int i = lineIndex; i < lines.Count; i++)
            {
                if (lines[i].type == "选项")
                {
                    choiceIndex = i;
                    break;
                }
            }

            if (choiceIndex != -1)
            {
                StopAllCoroutines();
                isProcessing = false;
                isFastForwarding = false;
                lineIndex = choiceIndex;
                ShowNextLine();
                return;
            }

            // 无选项：标记本段已结束并清零行号，避免之后 RestartDialogue 续播跳过前位置
            FinalizeSkipProgress();
            ClearWriterAndDialogueUI();
            EndDialogue();
        }

        /// <summary>跳过时写入 Flowchart：开场视为已看完，行号归零</summary>
        void FinalizeSkipProgress()
        {
            if (saveFlowchart == null) return;
            saveFlowchart.SetIntegerVariable("VN_LineIndex", 0);
            if (!saveFlowchart.GetBooleanVariable("VN_IsOpeningDone"))
                saveFlowchart.SetBooleanVariable("VN_IsOpeningDone", true);
            if (dialogueText != null)
                saveFlowchart.SetStringVariable("VN_StoryFile", dialogueText.name);
        }

        /// <summary>开场正常播完时由 LevelFlowCoordinator 调用</summary>
        public void MarkOpeningStoryComplete()
        {
            EnsureSaveFlowchart();
            if (saveFlowchart == null) return;
            saveFlowchart.SetBooleanVariable("VN_IsOpeningDone", true);
            saveFlowchart.SetIntegerVariable("VN_LineIndex", 0);
            if (dialogueText != null)
                saveFlowchart.SetStringVariable("VN_StoryFile", dialogueText.name);
        }

        void EnsureSaveFlowchart()
        {
            if (saveFlowchart != null) return;
            var go = GameObject.Find("VN_SaveFlowchart");
            if (go != null)
                saveFlowchart = go.GetComponent<Flowchart>();
        }

        /// <summary>结束上一句 Say 协程并复位 Writer（不绕过 Fungus，避免 DoSay 在 while 里死等）</summary>
        void StopActiveSayDialog()
        {
            if (sayDialog == null) return;
            sayDialog.Stop();
            sayDialog.StopAllCoroutines();
        }

        /// <summary>重开剧本前清空对白（仅用于 RestartDialogue / EndDialogue）</summary>
        void ClearWriterAndDialogueUI()
        {
            if (sayDialog == null) return;
            StopActiveSayDialog();
            sayDialog.Clear();
        }

        /// <summary>为按钮添加独立 Canvas，确保在最上层显示</summary>
        void EnsureButtonOnTop(Button btn)
        {
            if (btn == null) return;
            var canvas = btn.gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = btn.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            var raycaster = btn.gameObject.GetComponent<GraphicRaycaster>();
            if (raycaster == null) btn.gameObject.AddComponent<GraphicRaycaster>();
        }

        /// <summary>设置跳过按钮显隐（供 LevelFlowCoordinator 调用）</summary>
        public void SetSkipButtonVisible(bool visible)
        {
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(visible);
                Debug.Log($"[FungusVNController] 跳过按钮显隐设置为: {visible}");
            }
            else
            {
                Debug.LogWarning("[FungusVNController] skipButton 为 null，无法设置显隐");
            }
        }

        /// <summary>替换剧本占位符等当前字体无法渲染的字符，避免 Writer 卡住</summary>
        static string SanitizeDisplayText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("▉", "*");
        }

        /// <summary>推进到下一行对话</summary>
        void ShowNextLine()
        {
            if (lines == null || lineIndex >= lines.Count)
            {
                EndDialogue();
                return;
            }

            // 选项面板打开时禁止推进，否则会顺序执行「选项」后面的 [block]/对白/[action]，出现分支错乱或直接进游戏
            if (isChoosing) return;

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
                AudioClip clip = se?.clip;
                if (clip == null)
                    clip = LoadAudioClip(line.se, false);

                if (clip != null && sfxSource != null)
                    sfxSource.PlayOneShot(clip);
                else
                    ShowPlaceholder("SFX", line.se);
            }

            if (!string.IsNullOrEmpty(line.bgm))
            {
                var bgm = bgmTracks.Find(b => b.name == line.bgm);
                AudioClip clip = bgm?.clip;
                if (clip == null)
                    clip = LoadAudioClip(line.bgm, true);

                if (clip != null && bgmSource != null)
                {
                    bgmSource.clip = clip;
                    bgmSource.Play();
                }
                else
                {
                    ShowPlaceholder("BGM", line.bgm);
                }
            }

            // ========== 对话框显隐控制 ==========
            if (line.dialogAction == "hide" && sayDialog != null)
            {
                sayDialog.gameObject.SetActive(false);
            }
            else if (line.dialogAction == "show" && sayDialog != null)
            {
                sayDialog.gameObject.SetActive(true);
            }

            // ========== 居中大字显示 ==========
            if (!string.IsNullOrEmpty(line.centerText))
            {
                ShowCenterText(line.centerText);
                StartCoroutine(WaitAndContinue(isFastForwarding ? 0.05f : 3f));
                return;
            }

            // ========== 等待指令 ==========
            if (line.wait > 0)
            {
                StartCoroutine(WaitAndContinue(isFastForwarding ? 0.05f : line.wait));
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

                var charMapping = characters.Find(c => c.name == speaker);
                if (charMapping != null && charMapping.fungusCharacter != null)
                    sayDialog.SetCharacter(charMapping.fungusCharacter);
                else
                {
                    sayDialog.SetCharacter(null);
                    sayDialog.SetCharacterName(speaker, Color.white);
                    if (line.type == "对话")
                        ShowPlaceholder("Character", speaker);
                }

                sayDialog.gameObject.SetActive(true);
                string displayText = SanitizeDisplayText(line.content);

                // 仅在上一句仍在输出/等待时停止，避免 Clear 后首句 Say 被 Stop 掉导致黑屏无字
                var writer = sayDialog.GetComponent<Writer>();
                if (writer != null && (writer.IsWriting || writer.IsWaitingForInput))
                    StopActiveSayDialog();

                if (isFastForwarding)
                {
                    sayDialog.Say(displayText, true, false, false, true, false, null, null);
                    StartCoroutine(FastForwardSkipRoutine());
                }
                else
                {
                    sayDialog.Say(displayText, true, true, false, true, false, null, () =>
                    {
                        CancelSayWatchdog();
                        isProcessing = false;
                        ShowNextLine();
                    });
                    CancelSayWatchdog();
                    sayWatchdogRoutine = StartCoroutine(SayWatchdog());
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

            if (!string.IsNullOrEmpty(line.speaker))
            {
                sayDialog.SetCharacterName(line.speaker, Color.white);
            }
            else
            {
                sayDialog.SetCharacter(null);
                sayDialog.NameText = "";
            }
            sayDialog.gameObject.SetActive(true);
            // 保留之前的文本，不清空 StoryText，使选项与旁白同时显示

            // 向前查找最近一句对话/旁白/场景文本，确保跳过到达选项时对话框显示正确内容
            int choiceIdx = lines.IndexOf(line);
            if (choiceIdx >= 0 && sayDialog != null)
            {
                for (int i = choiceIdx - 1; i >= 0; i--)
                {
                    var prev = lines[i];
                    if ((prev.type == "对话" || prev.type == "旁白" || prev.type == "场景") && !string.IsNullOrEmpty(prev.content))
                    {
                        sayDialog.StoryText = prev.content;
                        break;
                    }
                }
            }

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
                EventCenter.Instance?.EventTrigger(E_EventType.StoryChoiceMade, choiceId);
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
            yield return new WaitForSecondsRealtime(0.02f);

            var writer = sayDialog != null ? sayDialog.GetComponent<Writer>() : null;
            if (writer != null && writer.IsWriting)
                writer.Stop();

            yield return new WaitForSecondsRealtime(fastForwardInterval);

            isProcessing = false;
            ShowNextLine();
        }

        void CancelSayWatchdog()
        {
            if (sayWatchdogRoutine != null)
            {
                StopCoroutine(sayWatchdogRoutine);
                sayWatchdogRoutine = null;
            }
        }

        /// <summary>Say 回调丢失时超时自动推进（用 unscaled 避免 timeScale=0 卡死）</summary>
        IEnumerator SayWatchdog()
        {
            yield return new WaitForSecondsRealtime(20f);
            sayWatchdogRoutine = null;
            if (!isProcessing || hasEnded) yield break;
            Debug.LogWarning("[FungusVNController] Say 回调超时，强制推进下一行");
            isProcessing = false;
            ShowNextLine();
        }

        IEnumerator WaitAndContinue(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            HideCenterText();
            isProcessing = false;
            ShowNextLine();
        }

        /// <summary>显示居中大字（隐藏对话框，独立 Canvas 最前显示）</summary>
        void ShowCenterText(string text)
        {
            if (sayDialog != null)
                sayDialog.gameObject.SetActive(false);

            if (centerTextDisplay == null || centerTextDisplay.gameObject == null)
            {
                var go = new GameObject("CenterTextCanvas");
                var canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000;
                go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                go.AddComponent<GraphicRaycaster>();

                var textGo = new GameObject("CenterText");
                textGo.transform.SetParent(go.transform, false);
                var rect = textGo.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(1200, 300);

                textGo.AddComponent<CanvasRenderer>();
                centerTextDisplay = textGo.AddComponent<Text>();
                centerTextDisplay.fontSize = 48;
                centerTextDisplay.color = Color.white;
                centerTextDisplay.alignment = TextAnchor.MiddleCenter;
                centerTextDisplay.raycastTarget = false;

                var outline = textGo.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(2f, -2f);

                // 加载中文字体
                var chineseFont = Resources.Load<Font>("Font/LXGWWenKaiScreen");
                if (chineseFont == null) chineseFont = Resources.Load<Font>("Font/文软雅黑");
                if (chineseFont != null) centerTextDisplay.font = chineseFont;
            }

            centerTextDisplay.text = text;
            centerTextDisplay.gameObject.SetActive(true);
            centerTextDisplay.transform.parent.gameObject.SetActive(true);
        }

        /// <summary>隐藏居中大字</summary>
        void HideCenterText()
        {
            if (centerTextDisplay != null && centerTextDisplay.gameObject != null)
            {
                var canvasRoot = centerTextDisplay.transform.parent;
                centerTextDisplay.gameObject.SetActive(false);
                if (canvasRoot != null)
                    canvasRoot.gameObject.SetActive(false);
            }
        }

        /// <summary>执行分支动作指令</summary>
        void ExecuteAction(string action)
        {
            isProcessing = false;
            isChoosing = false;

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
            isProcessing = false;
            isChoosing = false;
            ClearWriterAndDialogueUI();
            HideCenterText();

            if (choicePanel != null)
                choicePanel.SetActive(false);
            if (sayDialog != null)
                sayDialog.gameObject.SetActive(false);
            if (skipButton != null)
                skipButton.gameObject.SetActive(false);
            if (vnCanvas != null)
                vnCanvas.gameObject.SetActive(false);
            Debug.Log($"[FungusVNController] 对话结束，分支类型: {exitType}");
            OnDialogueExit?.Invoke(exitType);
            // 不再调用 OnDialogueComplete：LevelFlowCoordinator 等对开场/结尾同时订阅 Exit 与 Complete 到同一收尾时，
            // 会在「结局」分支后再跑一次（例如又执行 OnOpeningStoryEnd → 进游玩）。段切换已由 OnDialogueExit 处理。
        }
    }
}
