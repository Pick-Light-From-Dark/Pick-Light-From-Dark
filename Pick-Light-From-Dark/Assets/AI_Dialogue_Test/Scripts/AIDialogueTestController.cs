using UnityEngine;
using UnityEngine.UI;

namespace Game.Test.AI
{
    /// <summary>
    /// AI 对话生成测试控制器。
    /// 将 AIDialogueGenerator + AIStyleTrainer + AIPlayerHistory 串联，
    /// 运行时创建测试 UI，支持输入情境一键生成对话。
    /// </summary>
    public class AIDialogueTestController : MonoBehaviour
    {
        [Header("风格样本（拖入 Dialogue1-1.txt）")]
        public TextAsset styleSampleText;

        [Header("测试配置")]
        [TextArea(3, 5)]
        public string testContext = "陆萤躺在床上，焦虑不安。";

        [Header("组件引用（为空则自动创建）")]
        public AIDialogueGenerator generator;
        public AIStyleTrainer styleTrainer;
        public AIPlayerHistory playerHistory;

        // UI 引用
        private InputField contextInput;
        private Text resultText;
        private Button generateBtn;
        private Text statusText;
        private Canvas testCanvas;

        void Start()
        {
            EnsureComponents();
            CreateTestUI();
        }

        void EnsureComponents()
        {
            // 1. AIDialogueGenerator
            if (generator == null)
            {
                generator = FindObjectOfType<AIDialogueGenerator>();
                if (generator == null)
                {
                    var go = new GameObject("AIDialogueGenerator");
                    generator = go.AddComponent<AIDialogueGenerator>();
                }
            }

            // 2. AIStyleTrainer
            if (styleTrainer == null)
            {
                styleTrainer = FindObjectOfType<AIStyleTrainer>();
                if (styleTrainer == null)
                {
                    var go = new GameObject("AIStyleTrainer");
                    styleTrainer = go.AddComponent<AIStyleTrainer>();
                }
            }

            // 绑定风格样本
            if (styleSampleText != null)
                styleTrainer.styleSampleText = styleSampleText;

            // 3. AIPlayerHistory
            if (playerHistory == null)
            {
                playerHistory = FindObjectOfType<AIPlayerHistory>();
                if (playerHistory == null)
                {
                    var go = new GameObject("AIPlayerHistory");
                    playerHistory = go.AddComponent<AIPlayerHistory>();
                }
            }
        }

        void CreateTestUI()
        {
            // 创建 Canvas
            var canvasGo = new GameObject("AI_Test_Canvas");
            testCanvas = canvasGo.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            testCanvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // 背景面板
            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(100, 100);
            panelRect.offsetMax = new Vector2(-100, -100);
            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // 标题
            CreateLabel("Title", panelGo.transform, new Vector2(0, 350), "AI 对话生成测试", 36);

            // 状态文本
            statusText = CreateLabel("Status", panelGo.transform, new Vector2(0, 280), "等待启动...", 18);
            statusText.color = Color.gray;

            // 情境输入标签
            CreateLabel("InputLabel", panelGo.transform, new Vector2(-250, 200), "情境描述：", 22);

            // 情境输入框
            var inputGo = new GameObject("ContextInput");
            inputGo.transform.SetParent(panelGo.transform, false);
            var inputRect = inputGo.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.pivot = new Vector2(0.5f, 0.5f);
            inputRect.anchoredPosition = new Vector2(0, 120);
            inputRect.sizeDelta = new Vector2(800, 120);

            var inputBg = inputGo.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            var inputTextGo = new GameObject("Text");
            inputTextGo.transform.SetParent(inputGo.transform, false);
            var inputTextRect = inputTextGo.AddComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = new Vector2(10, 10);
            inputTextRect.offsetMax = new Vector2(-10, -10);
            var inputText = inputTextGo.AddComponent<Text>();
            inputText.fontSize = 22;
            inputText.color = Color.white;
            inputText.alignment = TextAnchor.UpperLeft;

            var placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(inputGo.transform, false);
            var placeholderRect = placeholderGo.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10, 10);
            placeholderRect.offsetMax = new Vector2(-10, -10);
            var placeholder = placeholderGo.AddComponent<Text>();
            placeholder.fontSize = 22;
            placeholder.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholder.alignment = TextAnchor.UpperLeft;
            placeholder.text = "输入当前情境描述...";

            contextInput = inputGo.AddComponent<InputField>();
            contextInput.textComponent = inputText;
            contextInput.placeholder = placeholder;
            contextInput.text = testContext;
            contextInput.lineType = InputField.LineType.MultiLineSubmit;

            // 生成按钮
            var btnGo = new GameObject("GenerateBtn");
            btnGo.transform.SetParent(panelGo.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0, -20);
            btnRect.sizeDelta = new Vector2(200, 50);

            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.5f, 0.3f, 1f);

            generateBtn = btnGo.AddComponent<Button>();
            generateBtn.onClick.AddListener(OnGenerateClicked);

            var btnTextGo = new GameObject("Text");
            btnTextGo.transform.SetParent(btnGo.transform, false);
            var btnTextRect = btnTextGo.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = new Vector2(10, 5);
            btnTextRect.offsetMax = new Vector2(-10, -5);
            var btnText = btnTextGo.AddComponent<Text>();
            btnText.fontSize = 22;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.text = "生成对话";

            // 快捷测试按钮：记录选择
            var recordBtnGo = new GameObject("RecordChoiceBtn");
            recordBtnGo.transform.SetParent(panelGo.transform, false);
            var recordBtnRect = recordBtnGo.AddComponent<RectTransform>();
            recordBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
            recordBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
            recordBtnRect.pivot = new Vector2(0.5f, 0.5f);
            recordBtnRect.anchoredPosition = new Vector2(250, -20);
            recordBtnRect.sizeDelta = new Vector2(180, 50);

            var recordBtnImg = recordBtnGo.AddComponent<Image>();
            recordBtnImg.color = new Color(0.3f, 0.3f, 0.5f, 1f);

            var recordBtn = recordBtnGo.AddComponent<Button>();
            recordBtn.onClick.AddListener(() =>
            {
                playerHistory.RecordChoice("吃了宋明月的薯片");
                UpdateStatus($"已记录选择。历史：{playerHistory.GetRecentHistory(3)}");
            });

            var recordBtnTextGo = new GameObject("Text");
            recordBtnTextGo.transform.SetParent(recordBtnGo.transform, false);
            var recordBtnTextRect = recordBtnTextGo.AddComponent<RectTransform>();
            recordBtnTextRect.anchorMin = Vector2.zero;
            recordBtnTextRect.anchorMax = Vector2.one;
            recordBtnTextRect.offsetMin = new Vector2(10, 5);
            recordBtnTextRect.offsetMax = new Vector2(-10, -5);
            var recordBtnText = recordBtnTextGo.AddComponent<Text>();
            recordBtnText.fontSize = 20;
            recordBtnText.color = Color.white;
            recordBtnText.alignment = TextAnchor.MiddleCenter;
            recordBtnText.text = "记录测试选择";

            // 清空历史按钮
            var clearBtnGo = new GameObject("ClearHistoryBtn");
            clearBtnGo.transform.SetParent(panelGo.transform, false);
            var clearBtnRect = clearBtnGo.AddComponent<RectTransform>();
            clearBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
            clearBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
            clearBtnRect.pivot = new Vector2(0.5f, 0.5f);
            clearBtnRect.anchoredPosition = new Vector2(-250, -20);
            clearBtnRect.sizeDelta = new Vector2(180, 50);

            var clearBtnImg = clearBtnGo.AddComponent<Image>();
            clearBtnImg.color = new Color(0.5f, 0.2f, 0.2f, 1f);

            var clearBtn = clearBtnGo.AddComponent<Button>();
            clearBtn.onClick.AddListener(() =>
            {
                playerHistory.ClearHistory();
                UpdateStatus("历史记录已清空");
            });

            var clearBtnTextGo = new GameObject("Text");
            clearBtnTextGo.transform.SetParent(clearBtnGo.transform, false);
            var clearBtnTextRect = clearBtnTextGo.AddComponent<RectTransform>();
            clearBtnTextRect.anchorMin = Vector2.zero;
            clearBtnTextRect.anchorMax = Vector2.one;
            clearBtnTextRect.offsetMin = new Vector2(10, 5);
            clearBtnTextRect.offsetMax = new Vector2(-10, -5);
            var clearBtnText = clearBtnTextGo.AddComponent<Text>();
            clearBtnText.fontSize = 20;
            clearBtnText.color = Color.white;
            clearBtnText.alignment = TextAnchor.MiddleCenter;
            clearBtnText.text = "清空历史";

            // 结果标签
            CreateLabel("ResultLabel", panelGo.transform, new Vector2(-250, -100), "生成结果：", 22);

            // 结果文本
            var resultGo = new GameObject("ResultText");
            resultGo.transform.SetParent(panelGo.transform, false);
            var resultRect = resultGo.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultRect.pivot = new Vector2(0.5f, 0.5f);
            resultRect.anchoredPosition = new Vector2(0, -240);
            resultRect.sizeDelta = new Vector2(800, 220);

            var resultBg = resultGo.AddComponent<Image>();
            resultBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            var resultTextGo = new GameObject("Text");
            resultTextGo.transform.SetParent(resultGo.transform, false);
            var resultTextRect = resultTextGo.AddComponent<RectTransform>();
            resultTextRect.anchorMin = Vector2.zero;
            resultTextRect.anchorMax = Vector2.one;
            resultTextRect.offsetMin = new Vector2(15, 15);
            resultTextRect.offsetMax = new Vector2(-15, -15);
            resultText = resultTextGo.AddComponent<Text>();
            resultText.fontSize = 20;
            resultText.color = new Color(0.8f, 0.9f, 0.8f, 1f);
            resultText.alignment = TextAnchor.UpperLeft;
            resultText.text = "点击「生成对话」开始测试...\n\n" +
                "先点击「记录测试选择」添加玩家历史，\n" +
                "再输入情境描述，点击生成。";

            UpdateStatus($"服务器状态: {(generator.IsServerReady() ? "就绪" : "启动中...")}");
        }

        Text CreateLabel(string name, Transform parent, Vector2 pos, string text, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(600, 40);
            var txt = go.AddComponent<Text>();
            txt.fontSize = fontSize;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = text;
            return txt;
        }

        void OnGenerateClicked()
        {
            if (generator == null || styleTrainer == null)
            {
                UpdateStatus("错误：组件未初始化");
                return;
            }

            var context = contextInput != null ? contextInput.text : testContext;
            if (string.IsNullOrWhiteSpace(context))
            {
                UpdateStatus("请输入情境描述");
                return;
            }

            if (!generator.IsServerReady())
            {
                UpdateStatus("服务器未就绪，请等待 llama-server 启动...");
                return;
            }

            UpdateStatus("正在生成...");
            if (resultText != null)
                resultText.text = "生成中...";

            // 构建 prompt
            var history = playerHistory.GetFormattedHistory();
            var systemPrompt = styleTrainer.BuildSystemPrompt(history, context);
            var userPrompt = $"情境：{context}\n请生成1-2句对话：";

            generator.Generate(systemPrompt, userPrompt, result =>
            {
                if (resultText != null)
                {
                    resultText.text = string.IsNullOrEmpty(result)
                        ? "生成失败，请检查 llama-server 日志。"
                        : result;
                }
                UpdateStatus(string.IsNullOrEmpty(result) ? "生成失败" : "生成完成");
            });
        }

        void UpdateStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
            UnityEngine.Debug.Log($"[AI_Test] {msg}");
        }
    }
}
