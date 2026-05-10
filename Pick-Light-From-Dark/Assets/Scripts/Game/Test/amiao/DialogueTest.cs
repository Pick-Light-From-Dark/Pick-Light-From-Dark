using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 对话系统测试脚本
    /// 使用 GalDialoguePanel 以视觉小说模式展示第一段对话
    /// </summary>
    public class DialogueTest : MonoBehaviour
    {
        [Header("测试配置")]
        public bool runOnStart = true;

        [Header("对话文本 (留空自动加载 Resources/Dialogue1)")]
        public TextAsset dialogueText;

        void Start()
        {
            if (!runOnStart) return;

            RunTest();
        }

        public void RunTest()
        {
            Debug.Log("=== 对话系统测试开始 ===");

            // 加载对话文本
            if (dialogueText == null)
            {
                dialogueText = Resources.Load<TextAsset>("Dialogue1");
                if (dialogueText == null)
                {
                    Debug.LogError("[FAIL] 找不到对话文本 Resources/Dialogue1.txt，请在 Inspector 中手动赋值");
                    return;
                }
            }

            Debug.Log($"[LOAD] 已加载对话文本: {dialogueText.name}, 长度: {dialogueText.text.Length} 字符");

            // 显示 Gal 对话面板并启动视觉小说模式
            UIMgr.Instance.ShowPanel<GalDialoguePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    if (panel == null)
                    {
                        Debug.LogError("[FAIL] GalDialoguePanel 加载失败");
                        return;
                    }

                    Debug.Log("[PANEL] GalDialoguePanel 已显示");

                    DialogueSystem.Instance.BindPanel(panel);
                    DialogueSystem.Instance.StartDialogue(dialogueText, DialogueSystem.DialogueMode.Gal);

                    Debug.Log("[START] 视觉小说对话已启动 - 点击鼠标左键推进");
                }
            );
        }
    }
}
