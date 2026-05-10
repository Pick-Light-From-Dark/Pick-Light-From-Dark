using UnityEngine;
using System.Text;

namespace Game.Test.AI
{
    /// <summary>
    /// AI 风格训练器。
    /// 读取已有文案，构建 few-shot system prompt，让模型模仿项目语言风格。
    /// </summary>
    public class AIStyleTrainer : MonoBehaviour
    {
        [Header("风格样本（拖入 Dialogue1-1.txt 等文案）")]
        public TextAsset styleSampleText;

        [Header("提取示例数量")]
        [Range(3, 10)]
        public int maxExamples = 5;

        [Header("System Prompt 模板")]
        [TextArea(8, 15)]
        public string systemPromptTemplate = @"你是一位视觉小说对话写手，擅长写细腻、有氛围感的中文对话。

请严格遵循以下风格：
1. 心理活动用括号（）包裹
2. 对话简洁自然，符合角色性格
3. 场景描述富有画面感

参考示例：
{STYLE_EXAMPLES}

根据下面的情境和玩家历史，生成1-2句符合风格的对话。只输出对话内容，不要解释。";

        private StringBuilder examplesBuilder = new StringBuilder();
        private bool isBuilt;

        void Awake()
        {
            BuildStyleExamples();
        }

        /// <summary>从文案中提取对话示例</summary>
        public void BuildStyleExamples()
        {
            if (styleSampleText == null)
            {
                UnityEngine.Debug.LogWarning("[AIStyleTrainer] 未分配风格样本文本，使用默认风格");
                examplesBuilder.Clear();
                examplesBuilder.AppendLine("陆萤：（还有十五分钟熄灯。）");
                examplesBuilder.AppendLine("宋明月：你还好吗？");
                examplesBuilder.AppendLine("宿管：别让我逮到有人说小话！");
                isBuilt = true;
                return;
            }

            var lines = styleSampleText.text.Split('\n');
            int exampleCount = 0;

            examplesBuilder.Clear();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // 跳过指令行和标记行
                if (trimmed.StartsWith("[") && trimmed.Contains("]")) continue;
                if (trimmed.StartsWith("选项")) continue;
                if (trimmed.StartsWith("【")) continue;
                if (trimmed.StartsWith("→")) continue;
                if (trimmed.Contains("跳转")) continue;

                // 收集对话行和场景描述
                if (trimmed.Contains("：") || trimmed.StartsWith("（"))
                {
                    examplesBuilder.AppendLine(trimmed);
                    exampleCount++;
                    if (exampleCount >= maxExamples) break;
                }
            }

            isBuilt = true;
            UnityEngine.Debug.Log($"[AIStyleTrainer] 已构建 {exampleCount} 个风格示例");
        }

        /// <summary>构建完整的 System Prompt</summary>
        public string BuildSystemPrompt(string playerHistory, string context)
        {
            if (!isBuilt) BuildStyleExamples();

            var examples = examplesBuilder.ToString();
            if (string.IsNullOrEmpty(examples))
            {
                examples = "陆萤：（还有十五分钟熄灯。）\n宋明月：你还好吗？";
            }

            var prompt = systemPromptTemplate
                .Replace("{STYLE_EXAMPLES}", examples)
                .Replace("{PLAYER_HISTORY}", string.IsNullOrEmpty(playerHistory) ? "无" : playerHistory)
                .Replace("{CONTEXT}", string.IsNullOrEmpty(context) ? "无" : context);

            return prompt;
        }

        /// <summary>仅构建风格示例部分（用于复用）</summary>
        public string GetStyleExamples()
        {
            if (!isBuilt) BuildStyleExamples();
            return examplesBuilder.ToString();
        }
    }
}
