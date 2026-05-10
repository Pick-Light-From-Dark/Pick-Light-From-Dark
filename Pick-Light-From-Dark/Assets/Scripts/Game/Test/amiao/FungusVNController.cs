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
        public Image backgroundImage;

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
        private Font cachedFont;

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
                vnCanvas.sortingOrder = 10;
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

            // 3. BackgroundImage：查找或创建全屏背景
            if (backgroundImage == null)
            {
                var bgGo = GameObject.Find("VNBackground");
                if (bgGo != null)
                {
                    backgroundImage = bgGo.GetComponent<Image>();
                }
                else
                {
                    bgGo = new GameObject("VNBackground");
                    bgGo.transform.SetParent(vnCanvas.transform, false);
                    var rect = bgGo.AddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    backgroundImage = bgGo.AddComponent<Image>();
                    backgroundImage.color = Color.clear; // 默认透明，避免白色方块
                    backgroundImage.raycastTarget = false;
                }
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

            // 5. 缓存中文字体
            CacheChineseFont();

            // 6. 字体设置：自动应用到 SayDialog
            SetupFont();
        }

        void CacheChineseFont()
        {
            if (cachedFont != null) return;

            cachedFont = Resources.Load<Font>("Font/文软雅黑");
            if (cachedFont == null)
                cachedFont = Resources.Load<Font>("Fonts/文软雅黑");

            if (cachedFont == null)
                Debug.LogWarning("[FungusVNController] 未找到中文字体，按钮文字可能无法显示");
        }

        void SetupFont()
        {
            if (sayDialog == null || cachedFont == null) return;

            var texts = sayDialog.GetComponentsInChildren<Text>(true);
            foreach (var txt in texts)
            {
                txt.font = cachedFont;
            }
            Debug.Log("[FungusVNController] 已设置中文字体");
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
            btn.targetGraphic = img; // 关键：设置点击目标图形
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
            if (cachedFont != null)
                txt.font = cachedFont; // 使用项目中文字体
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

            // ========== 指令处理（背景 / 音效 / BGM）==========
            if (!string.IsNullOrEmpty(line.bg))
            {
                var bg = backgrounds.Find(b => b.name == line.bg);
                if (bg != null && bg.sprite != null && backgroundImage != null)
                {
                    backgroundImage.sprite = bg.sprite;
                    backgroundImage.color = Color.white; // 有图时显示
                }
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
                if (blackSprite != null && backgroundImage != null)
                {
                    backgroundImage.sprite = blackSprite;
                    backgroundImage.color = Color.white;
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
