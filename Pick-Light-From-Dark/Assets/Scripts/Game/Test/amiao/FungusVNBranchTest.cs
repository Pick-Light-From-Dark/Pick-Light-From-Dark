using UnityEngine;
using System.Collections.Generic;

namespace Game.Test
{
    /// <summary>
    /// FungusVN 分支功能测试器
    /// 挂载到任意 GameObject 上，Play 后自动解析指定对话文本并验证 block/action/跳转 语法
    /// </summary>
    public class FungusVNBranchTest : MonoBehaviour
    {
        [Header("待测试对话文本")]
        public TextAsset dialogueText;

        [Header("期望验证的段落名（留空则自动检测）")]
        public List<string> expectedBlocks = new List<string>();

        [Header("测试开关")]
        public bool runOnStart = true;

        void Start()
        {
            if (!runOnStart) return;
            RunTest();
        }

        public void RunTest()
        {
            Debug.Log("=== FungusVNBranchTest 开始 ===");

            if (dialogueText == null)
            {
                dialogueText = Resources.Load<TextAsset>("Dialogue/Dialogue1");
                if (dialogueText == null)
                {
                    Debug.LogError("[FAIL] 找不到对话文本 Resources/Dialogue/Dialogue1.txt");
                    return;
                }
            }

            Debug.Log($"[LOAD] 已加载: {dialogueText.name}");

            var lines = DialogueParser.Parse(dialogueText);
            Debug.Log($"[PARSE] 共解析 {lines.Count} 行");

            // 1. 验证段落索引
            var blockIndex = new Dictionary<string, int>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].type == "段落" && !string.IsNullOrEmpty(lines[i].content))
                {
                    blockIndex[lines[i].content] = i + 1;
                    Debug.Log($"[BLOCK] 段落 '{lines[i].content}' 起始索引: {i + 1}");
                }
            }

            if (expectedBlocks.Count > 0)
            {
                foreach (var blockName in expectedBlocks)
                {
                    if (blockIndex.ContainsKey(blockName))
                        Debug.Log($"[PASS] 期望段落 '{blockName}' 已找到，起始 {blockIndex[blockName]}");
                    else
                        Debug.LogError($"[FAIL] 期望段落 '{blockName}' 未找到");
                }
            }

            // 2. 验证选项绑定
            int choiceCount = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].type == "选项")
                {
                    choiceCount++;
                    var line = lines[i];
                    Debug.Log($"[CHOICE #{choiceCount}] '{line.choice1}' vs '{line.choice2}'");

                    if (!string.IsNullOrEmpty(line.choice1JumpTarget))
                        Debug.Log($"  [CHOICE1-JUMP] → {line.choice1JumpTarget} (索引: {(blockIndex.ContainsKey(line.choice1JumpTarget) ? blockIndex[line.choice1JumpTarget].ToString() : "未找到")})");
                    if (!string.IsNullOrEmpty(line.choice1Action))
                        Debug.Log($"  [CHOICE1-ACTION] → {line.choice1Action}");
                    if (!string.IsNullOrEmpty(line.choice1Result))
                        Debug.Log($"  [CHOICE1-RESULT] 长度 {line.choice1Result.Length}");

                    if (!string.IsNullOrEmpty(line.choice2JumpTarget))
                        Debug.Log($"  [CHOICE2-JUMP] → {line.choice2JumpTarget} (索引: {(blockIndex.ContainsKey(line.choice2JumpTarget) ? blockIndex[line.choice2JumpTarget].ToString() : "未找到")})");
                    if (!string.IsNullOrEmpty(line.choice2Action))
                        Debug.Log($"  [CHOICE2-ACTION] → {line.choice2Action}");
                    if (!string.IsNullOrEmpty(line.choice2Result))
                        Debug.Log($"  [CHOICE2-RESULT] 长度 {line.choice2Result.Length}");

                    // 验证跳转目标有效性
                    if (!string.IsNullOrEmpty(line.choice1JumpTarget) && !blockIndex.ContainsKey(line.choice1JumpTarget))
                        Debug.LogError($"[FAIL] Choice1 跳转目标 '{line.choice1JumpTarget}' 不存在");
                    if (!string.IsNullOrEmpty(line.choice2JumpTarget) && !blockIndex.ContainsKey(line.choice2JumpTarget))
                        Debug.LogError($"[FAIL] Choice2 跳转目标 '{line.choice2JumpTarget}' 不存在");
                }
            }

            // 3. 验证 action 指令
            int actionCount = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                if (!string.IsNullOrEmpty(lines[i].action))
                {
                    actionCount++;
                    Debug.Log($"[ACTION #{actionCount}] 行 {i}: {lines[i].action}");
                }
            }

            // 4. 总结
            bool pass = true;
            if (choiceCount == 0)
            {
                Debug.LogWarning("[WARN] 未检测到选项行");
                pass = false;
            }
            if (actionCount == 0)
            {
                Debug.LogWarning("[WARN] 未检测到 action 指令");
                pass = false;
            }

            if (pass)
                Debug.Log("=== FungusVNBranchTest 全部通过 ===");
            else
                Debug.Log("=== FungusVNBranchTest 完成，存在警告 ===");
        }
    }
}
