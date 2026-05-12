using UnityEngine;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// Skip Button 可见性测试脚本
    /// 挂载到场景中的 FungusVNController 所在 GameObject 或独立 GameObject
    /// </summary>
    public class SkipButtonTest : MonoBehaviour
    {
        [Header("目标 VN 控制器")]
        public FungusVNController vnController;

        [Header("按键测试")]
        public KeyCode toggleKey = KeyCode.F1;

        void Start()
        {
            if (vnController == null)
            {
                vnController = FindObjectOfType<FungusVNController>();
                if (vnController == null)
                {
                    Debug.LogError("[SkipButtonTest] 未找到 FungusVNController");
                    return;
                }
            }

            // 延迟显示跳过按钮，确保 UI 已创建
            Invoke(nameof(ShowSkipButton), 1f);
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleSkipButton();
            }
        }

        void ShowSkipButton()
        {
            vnController.SetSkipButtonVisible(true);
            Debug.Log("[SkipButtonTest] 已调用 SetSkipButtonVisible(true)");
        }

        void ToggleSkipButton()
        {
            // 通过反射获取 skipButton 的 active 状态（仅供测试）
            var field = typeof(FungusVNController).GetField("skipButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var btn = field.GetValue(vnController) as Button;
                if (btn != null)
                {
                    bool newState = !btn.gameObject.activeSelf;
                    vnController.SetSkipButtonVisible(newState);
                    Debug.Log($"[SkipButtonTest] 切换跳过按钮状态: {newState}");
                }
                else
                {
                    Debug.LogWarning("[SkipButtonTest] skipButton 为 null");
                }
            }
        }

        [ContextMenu("显示跳过按钮")]
        void ShowSkip() { vnController?.SetSkipButtonVisible(true); }

        [ContextMenu("隐藏跳过按钮")]
        void HideSkip() { vnController?.SetSkipButtonVisible(false); }
    }
}
