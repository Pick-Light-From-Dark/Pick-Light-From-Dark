using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Test
{
    /// <summary>
    /// 视觉小说对话测试器 —— 一键启动完整 VN 体验
    /// 挂载到场景任意空物体，Play 后自动创建 Canvas/EventSystem/DialogueSystem
    /// 并启动 Gal 模式对话，展示打字机、背景划入划出、立绘淡入淡出效果
    /// </summary>
    public class DialogueVNTest : MonoBehaviour
    {
        [Header("对话文本 (留空自动加载 Resources/Dialogue1)")")]
        public TextAsset dialogueText;

        [Header("自动运行")]
        public bool runOnStart = true;

        [Header("可选：指定对话模式")]
        public DialogueSystem.DialogueMode mode = DialogueSystem.DialogueMode.Gal;

        void Start()
        {
            if (!runOnStart) return;
            RunTest();
        }

        public void RunTest()
        {
            Debug.Log("=== DialogueVNTest 启动 ===");

            // 加载对话文本
            if (dialogueText == null)
            {
                dialogueText = Resources.Load<TextAsset>("Dialogue1");
                if (dialogueText == null)
                {
                    Debug.LogError("[DialogueVNTest] 找不到对话文本 Resources/Dialogue1.txt");
                    return;
                }
            }
            Debug.Log($"[DialogueVNTest] 已加载对话: {dialogueText.name}");

            // 确保 EventSystem 存在
            if (EventSystem.current == null)
            {
                var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Debug.Log("[DialogueVNTest] 自动创建 EventSystem");
            }

            // 确保主 Canvas 存在（UIMgr 通常依赖它）
            EnsureMainCanvas();

            // 确保 DialogueSystem 存在
            if (DialogueSystem.Instance == null)
            {
                var dsGo = new GameObject("[DialogueSystem]");
                dsGo.AddComponent<DialogueSystem>();
                Debug.Log("[DialogueVNTest] 自动创建 DialogueSystem");
            }

            // 显示 Gal 面板并启动对话
            UIMgr.Instance.ShowPanel<GalDialoguePanel>(
                E_UILayer.Middle,
                (panel) =>
                {
                    if (panel == null)
                    {
                        Debug.LogError("[DialogueVNTest] GalDialoguePanel 加载失败");
                        return;
                    }

                    DialogueSystem.Instance.BindPanel(panel);
                    DialogueSystem.Instance.StartDialogue(dialogueText, mode);

                    Debug.Log("[DialogueVNTest] 视觉小说测试已启动 —— 点击鼠标左键推进对话");
                    Debug.Log("[DialogueVNTest] 功能验证清单:");
                    Debug.Log("  1. 打字机效果: 文本逐字显示");
                    Debug.Log("  2. 点击跳过: 打字中点击鼠标立即显示全文");
                    Debug.Log("  3. 立绘淡入淡出: 角色切换时立绘透明度渐变");
                    Debug.Log("  4. 背景划入划出: 场景切换时背景从左侧滑入滑出");
                    Debug.Log("  5. 选项分支: 遇到选项时显示 A/B 按钮");
                }
            );
        }

        void EnsureMainCanvas()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("MainCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;

                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                Debug.Log("[DialogueVNTest] 自动创建 MainCanvas");
            }
        }
    }
}
