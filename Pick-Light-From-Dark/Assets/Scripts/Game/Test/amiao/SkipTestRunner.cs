using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Fungus;

namespace Game.Test
{
    /// <summary>
    /// 跳过功能测试器 — 验证跳过行为：跳到选项时是否显示选项前对话，且不会跳过选项
    /// </summary>    public class SkipTestRunner : MonoBehaviour
    {
        FungusVNController vn;
        SayDialog sayDialog;

        [Header("测试配置")]
        [Tooltip("运行后自动执行一次跳过测试")]
        public bool autoTestOnStart = false;
        [Tooltip("自动测试延迟（秒）")]
        public float autoTestDelay = 1f;

        // 测试状态
        string beforeSkipName = "";
        string beforeSkipStory = "";
        string afterSkipName = "";
        string afterSkipStory = "";
        string expectedStoryBeforeChoice = "";
        int testPhase = 0; // 0=空闲 1=跳过前记录 2=跳过后记录
        float autoTestTimer = 0;
        bool autoTestTriggered = false;

        Rect windowRect = new Rect(10, 10, 420, 520);
        Vector2 scrollPos;
        List<string> logLines = new List<string>();

        void Start()
        {
            FindVN();
            Log("[SkipTest] 测试器启动，按 F4 显隐窗口");
            if (autoTestOnStart)
            {
                autoTestTimer = autoTestDelay;
                Log($"[SkipTest] 将在 {autoTestDelay} 秒后自动执行跳过测试");
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F4))
                enabled = !enabled;

            if (autoTestOnStart && !autoTestTriggered && autoTestTimer > 0)
            {
                autoTestTimer -= Time.deltaTime;
                if (autoTestTimer <= 0)
                {
                    autoTestTriggered = true;
                    RunSkipTest();
                }
            }
        }

        void FindVN()
        {
            vn = FindObjectOfType<FungusVNController>();
            if (vn != null)
            {
                var field = typeof(FungusVNController).GetField("sayDialog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    sayDialog = field.GetValue(vn) as SayDialog;
            }
        }

        void OnGUI()
        {
            windowRect = GUILayout.Window(0, windowRect, DrawWindow, "跳过功能测试器", GUILayout.Width(420));
        }

        void DrawWindow(int id)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(460));

            GUILayout.Label("=== VN 状态 ===", GUI.skin.box);
            if (vn == null)
            {
                GUILayout.Label("未找到 FungusVNController");
                if (GUILayout.Button("重新查找"))
                    FindVN();
            }
            else
            {
                var linesField = typeof(FungusVNController).GetField("lines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var lineIndexField = typeof(FungusVNController).GetField("lineIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var isChoosingField = typeof(FungusVNController).GetField("isChoosing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var lines = linesField?.GetValue(vn) as System.Collections.IList;
                int lineIndex = lineIndexField != null ? (int)lineIndexField.GetValue(vn) : -1;
                bool isChoosing = isChoosingField != null && (bool)isChoosingField.GetValue(vn);

                GUILayout.Label($"总行数: {(lines != null ? lines.Count : 0)}");
                GUILayout.Label($"当前行: {lineIndex}");
                GUILayout.Label($"是否在选择中: {isChoosing}");

                // 查找下一选项位置
                int nextChoiceIdx = -1;
                if (lines != null && lineIndex >= 0)
                {
                    for (int i = lineIndex; i < lines.Count; i++)
                    {
                        var line = lines[i];
                        var typeProp = line?.GetType().GetField("type");
                        string type = typeProp?.GetValue(line) as string;
                        if (type == "选项")
                        {
                            nextChoiceIdx = i;
                            break;
                        }
                    }
                }
                GUILayout.Label($"下一选项位置: {(nextChoiceIdx >= 0 ? nextChoiceIdx.ToString() : "无")}");

                GUILayout.Space(5);
                GUILayout.Label("=== 对话框当前文本 ===", GUI.skin.box);
                if (sayDialog != null)
                {
                    GUILayout.Label($"姓名: {sayDialog.NameText}");
                    GUILayout.Label($"内容: {sayDialog.StoryText}");
                }
                else
                {
                    GUILayout.Label("sayDialog 未找到");
                }

                GUILayout.Space(5);
                GUILayout.Label("=== 跳过测试 ===", GUI.skin.box);

                if (GUILayout.Button("执行跳过测试", GUILayout.Height(30)))
                {
                    RunSkipTest();
                }

                GUILayout.Label("跳过前:");
                GUILayout.Label($"  姓名: {beforeSkipName}");
                GUILayout.Label($"  内容: {beforeSkipStory}");

                GUILayout.Label("跳过后:");
                GUILayout.Label($"  姓名: {afterSkipName}");
                GUILayout.Label($"  内容: {afterSkipStory}");

                GUILayout.Label("预期选项前对话:");
                GUILayout.Label($"  {expectedStoryBeforeChoice}");

                // 验证结果
                GUILayout.Space(5);
                bool passChoice = isChoosing || nextChoiceIdx < 0;
                bool passText = string.IsNullOrEmpty(expectedStoryBeforeChoice) ||
                                afterSkipStory.Contains(expectedStoryBeforeChoice) ||
                                expectedStoryBeforeChoice.Contains(afterSkipStory);

                GUI.color = passChoice ? Color.green : Color.red;
                GUILayout.Label(passChoice ? "[通过] 已停在选项处" : "[失败] 未停在选项处");
                GUI.color = passText ? Color.green : Color.red;
                GUILayout.Label(passText ? "[通过] 文本显示正确" : "[失败] 文本显示不正确");
                GUI.color = Color.white;
            }

            GUILayout.Space(10);
            GUILayout.Label("=== 测试日志 ===", GUI.skin.box);
            foreach (var line in logLines)
            {
                GUILayout.Label(line);
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        void RunSkipTest()
        {
            FindVN();
            if (vn == null)
            {
                Log("[SkipTest] 错误：未找到 FungusVNController");
                return;
            }

            // 记录跳过前状态
            if (sayDialog != null)
            {
                beforeSkipName = sayDialog.NameText;
                beforeSkipStory = sayDialog.StoryText;
            }

            // 计算预期选项前对话
            expectedStoryBeforeChoice = GetExpectedTextBeforeChoice();
            Log($"[SkipTest] 预期选项前对话: {expectedStoryBeforeChoice}");

            // 触发跳过（通过反射调用私有方法 OnSkipStory）
            var onSkipMethod = typeof(FungusVNController).GetMethod("OnSkipStory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (onSkipMethod != null)
            {
                Log("[SkipTest] 触发跳过...");
                onSkipMethod.Invoke(vn, null);

                // 跳过是同步执行的，立即记录结果
                if (sayDialog != null)
                {
                    afterSkipName = sayDialog.NameText;
                    afterSkipStory = sayDialog.StoryText;
                }

                var isChoosingField = typeof(FungusVNController).GetField("isChoosing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                bool isChoosing = isChoosingField != null && (bool)isChoosingField.GetValue(vn);

                Log($"[SkipTest] 跳过后 isChoosing={isChoosing}");
                Log($"[SkipTest] 跳过后 Name={afterSkipName}");
                Log($"[SkipTest] 跳过后 Story={afterSkipStory}");

                if (isChoosing)
                    Log("[SkipTest] 结果：跳过成功停在选项处");
                else
                    Log("[SkipTest] 结果：无选项或已结束");
            }
            else
            {
                Log("[SkipTest] 错误：无法找到 OnSkipStory 方法");
            }
        }

        string GetExpectedTextBeforeChoice()
        {
            var linesField = typeof(FungusVNController).GetField("lines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var lineIndexField = typeof(FungusVNController).GetField("lineIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var lines = linesField?.GetValue(vn) as System.Collections.IList;
            int lineIndex = lineIndexField != null ? (int)lineIndexField.GetValue(vn) : 0;

            if (lines == null) return "";

            // 找到下一个选项
            int choiceIdx = -1;
            for (int i = lineIndex; i < lines.Count; i++)
            {
                var line = lines[i];
                var typeProp = line?.GetType().GetField("type");
                string type = typeProp?.GetValue(line) as string;
                if (type == "选项")
                {
                    choiceIdx = i;
                    break;
                }
            }
            if (choiceIdx < 0) return "(无后续选项)";

            // 向前查找最近一句有内容的对话/旁白/场景
            for (int i = choiceIdx - 1; i >= 0; i--)
            {
                var line = lines[i];
                var typeProp = line?.GetType().GetField("type");
                string type = typeProp?.GetValue(line) as string;
                var contentProp = line?.GetType().GetField("content");
                string content = contentProp?.GetValue(line) as string;

                if ((type == "对话" || type == "旁白" || type == "场景") && !string.IsNullOrEmpty(content))
                    return content;
            }
            return "(未找到选项前对话)";
        }

        void Log(string msg)
        {
            logLines.Add(msg);
            if (logLines.Count > 50) logLines.RemoveAt(0);
            Debug.Log(msg);
        }

        [ContextMenu("执行跳过测试")]
        void ContextMenuRunTest()
        {
            RunSkipTest();
        }
    }
}
