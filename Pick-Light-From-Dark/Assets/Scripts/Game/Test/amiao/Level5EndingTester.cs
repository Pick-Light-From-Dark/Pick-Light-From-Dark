using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// Level 5 结局五（北极星 / Dialogue5-2e）剧情预览测试器
    /// 挂载到场景空物体，Play 后自动加载 5-2e 文本并以 Gal 模式播放
    /// 用于验证 [pan] 平移指令及背景图移动效果
    /// </summary>
    public class Level5EndingTester : MonoBehaviour
    {
        [Header("对话文本（留空自动加载 Dialogue5-2e）")]
        public TextAsset dialogueText;

        [Header("自动运行")]
        public bool runOnStart = true;

        [Header("对话模式")]
        public DialogueSystem.DialogueMode mode = DialogueSystem.DialogueMode.Gal;

        void Start()
        {
            if (!runOnStart) return;
            RunTest();
        }

        public void RunTest()
        {
            Debug.Log("=== Level5EndingTester 启动 ===");

            if (dialogueText == null)
            {
                dialogueText = Resources.Load<TextAsset>("Dialogue/Dialogue5-2e");
                if (dialogueText == null)
                {
                    Debug.LogError("[Level5EndingTester] 找不到对话文本 Resources/Dialogue/Dialogue5-2e.txt");
                    return;
                }
            }

            Debug.Log($"[Level5EndingTester] 已加载对话: {dialogueText.name}");

            UIMgr.Instance.ShowPanel<GalDialoguePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    if (panel == null)
                    {
                        Debug.LogError("[Level5EndingTester] GalDialoguePanel 加载失败");
                        return;
                    }

                    DialogueSystem.Instance.BindPanel(panel);
                    DialogueSystem.Instance.SetOnComplete(() =>
                    {
                        Debug.Log("[Level5EndingTester] 剧情播放完毕");
                    });
                    DialogueSystem.Instance.StartDialogue(dialogueText, mode);

                    Debug.Log("[Level5EndingTester] Level 5 结局五已启动 —— 点击鼠标左键推进对话");
                    Debug.Log("[Level5EndingTester] 验证清单:");
                    Debug.Log("  1. [bg:星星图] 背景切换");
                    Debug.Log("  2. [pan:bg,up,x] 大图向上平移扫描效果");
                    Debug.Log("  3. 宋明月立绘淡入淡出");
                }
            );
        }
    }
}
