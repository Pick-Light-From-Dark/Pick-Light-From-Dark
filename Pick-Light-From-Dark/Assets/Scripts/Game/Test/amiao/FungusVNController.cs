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

        [Header("音频源（为空则运行时自动添加）")]
        public AudioSource sfxSource;
        public AudioSource bgmSource;

        [Header("素材映射（Inspector 静态配置）")]
        public List<CharacterMapping> characters = new List<CharacterMapping>();
        public List<SpriteMapping> backgrounds = new List<SpriteMapping>();
        public List<AudioMapping> soundEffects = new List<AudioMapping>();
        public List<AudioMapping> bgmTracks = new List<AudioMapping>();

        [Header("快进配置")]
        public float fastForwardInterval = 0.1f;

        private List<DialogueLine> lines;
        private int lineIndex;
        private bool isProcessing;
        private bool isChoosing;
        private DialogueLine currentChoiceLine;
        private bool isFastForwarding;
        private Canvas vnCanvas;

        // UI 按钮
        private Button ffButton;
        private GameObject choicePanel;
        private Button choiceBtn1;
        private Button choiceBtn2;
        private Text choiceText1;
        private Text choiceText2;

        void Awake()
        {
            EnsureComponents();
            CreateUI();
        }

        void Start()
        {
            if (dialogueText == null)
            {
                Debug.LogError("[FungusVNController] dialogueText 未赋值");
                return;
            }
            lines = DialogueParser.Parse(dialogueText);
            lineIndex = 0;
            ShowNextLine();
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

            // 2. SayDialog：优先查找场景中的，没有则实例化 Fungus 预制体
            if (sayDialog == null)
            {
                sayDialog = FindObjectOfType<SayDialog>();
                if (sayDialog == null)
                {
                    var prefab = Resources.Load<GameObject>("Prefabs/SayDialog");
                    if (prefab != null)
                    {
                        var go = Instantiate(prefab);
                        go.name = "SayDialog";
                        sayDialog = go.GetComponent<SayDialog>();
                        Debug.Log("[FungusVNController] 已自动实例化 SayDialog");
                    }
                    else
                    {
                        Debug.LogError("[FungusVNController] 未找到 SayDialog prefab。请确保 Fungus 已正确导入，或在 Inspector 中手动赋值。");
                    }
                }
            }

            // 3. 分层背景：查找或创建后/中/前 三层 Image
            EnsureLayerImage(ref backgroundImage, "VNBackground", -2);
            EnsureLayerImage(ref midgroundImage, "VNMidground", -1);
            EnsureLayerImage(ref foregroundImage, "VNForeground", 0);

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
        void SetLayerBackground(Image layerImage, string spriteName)
        {
            if (layerImage == null) return;

            if (string.IsNullOrEmpty(spriteName))
            {
                layerImage.sprite = null;
                layerImage.color = Color.clear;
                return;
            }

            var mapping = backgrounds.Find(b => b.name == spriteName);
            if (mapping != null && mapping.sprite != null)
            {
                layerImage.sprite = mapping.sprite;
                layerImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"[FungusVNController] 背景图未找到: {spriteName}");
            }
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
        void ApplySolidBackground(string colorStr, string transition)
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
                    StartCoroutine(FadeToSolidBackground(c));
                }
                else
                {
                    backgroundImage.color = c;
                    if (midgroundImage != null) midgroundImage.color = Color.clear;
                    if (foregroundImage != null) foregroundImage.color = Color.clear;
                }
            }
        }

        IEnumerator FadeToSolidBackground(Color targetColor)
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

            if (backgroundImage != null) backgroundImage.color = targetColor;
            if (midgroundImage != null) midgroundImage.color = Color.clear;
            if (foregroundImage != null) foregroundImage.color = Color.clear;
        }

        /// <summary>创建快进按钮和选项面板</summary>
        void CreateUI()
        {
            if (vnCanvas == null) return;

            // === 快进按钮（右上角）===
            ffButton = CreateButton("FFButton", vnCanvas.transform,
                new Vector2(-30, -30), new Vector2(100, 45), "快进",
                () => ToggleFastForward());
            ffButton.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            ffButton.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            ffButton.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
            ffButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-30, -30);

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

            // ========== 分层背景处理 ==========
            if (line.isLayerCommand)
            {
                // [layer:...] 全换指令：显式指定的层才更新（null=未指定，保持原样；空字符串=清空）
                if (line.bg != null) SetLayerBackground(backgroundImage, line.bg);
                if (line.mg != null) SetLayerBackground(midgroundImage, line.mg);
                if (line.fg != null) SetLayerBackground(foregroundImage, line.fg);
            }
            else
            {
                // 单换指令：有值才更新
                if (!string.IsNullOrEmpty(line.bg)) SetLayerBackground(backgroundImage, line.bg);
                if (!string.IsNullOrEmpty(line.mg)) SetLayerBackground(midgroundImage, line.mg);
                if (!string.IsNullOrEmpty(line.fg)) SetLayerBackground(foregroundImage, line.fg);
            }

            // ========== 纯色背景处理 ==========
            if (!string.IsNullOrEmpty(line.solid))
            {
                ApplySolidBackground(line.solid, line.transition);
            }

            if (!string.IsNullOrEmpty(line.se))
            {
                var se = soundEffects.Find(s => s.name == line.se);
                if (se != null && se.clip != null && sfxSource != null)
                    sfxSource.PlayOneShot(se.clip);
            }

            if (!string.IsNullOrEmpty(line.bgm))
            {
                var bgm = bgmTracks.Find(b => b.name == line.bgm);
                if (bgm != null && bgm.clip != null && bgmSource != null)
                {
                    bgmSource.clip = bgm.clip;
                    bgmSource.Play();
                }
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
                    sayDialog.SetCharacterName(speaker, Color.white);

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

            string result = choice == 1 ? currentChoiceLine.choice1Result : currentChoiceLine.choice2Result;

            if (!string.IsNullOrEmpty(result))
            {
                sayDialog.SetCharacter(null);
                sayDialog.NameText = "";
                sayDialog.gameObject.SetActive(true);

                if (isFastForwarding)
                {
                    sayDialog.Say(result, true, false, false, true, false, null, null);
                    StartCoroutine(FastForwardSkipRoutine());
                }
                else
                {
                    sayDialog.Say(result, true, true, false, true, false, null, () =>
                    {
                        EndDialogue();
                    });
                }
            }
            else
            {
                EndDialogue();
            }
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

        void EndDialogue()
        {
            if (choicePanel != null)
                choicePanel.SetActive(false);
            if (sayDialog != null)
                sayDialog.gameObject.SetActive(false);
            Debug.Log("[FungusVNController] 对话结束");
        }
    }
}
