using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 结局画面控制器 — 独立测试用结局面板
    /// 支持重新开始（死亡结局从本关游玩部分开始，其他结局从头开始）和返回主界面
    /// </summary>
    public class EndingScreenController : MonoBehaviour
    {
        [Header("结局显示")]
        public Text endingNameText;
        public Text endingDescriptionText;

        [Header("按钮")]
        public Button restartButton;
        public Button returnToMenuButton;

        [Header("按钮图片 (Sprite Swap)")]
        public Sprite normalButtonSprite;
        public Sprite hoverButtonSprite;

        [Header("结局配置")]
        public int currentEndingId;
        public bool isDeathEnding = false;

        [Header("场景名称")]
        public string mainMenuSceneName = "GameScene";
        public string currentLevelSceneName = "Level1";

        /// <summary>存档系统预留接口：点击重新开始时触发</summary>
        public System.Action OnRestartRequested;

        /// <summary>存档系统预留接口：点击返回主界面时触发</summary>
        public System.Action OnReturnToMainMenuRequested;

        void Awake()
        {
            SetupButtons();
        }

        void SetupButtons()
        {
            if (restartButton != null)
            {
                SetupSpriteSwap(restartButton);
                restartButton.onClick.AddListener(OnRestartClick);
            }

            if (returnToMenuButton != null)
            {
                SetupSpriteSwap(returnToMenuButton);
                returnToMenuButton.onClick.AddListener(OnReturnToMenuClick);
            }
        }

        /// <summary>配置按钮 SpriteSwap 悬停效果</summary>
        void SetupSpriteSwap(Button btn)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img == null) return;

            if (normalButtonSprite != null)
                img.sprite = normalButtonSprite;

            if (hoverButtonSprite != null)
            {
                var spriteState = btn.spriteState;
                spriteState.highlightedSprite = hoverButtonSprite;
                spriteState.pressedSprite = hoverButtonSprite;
                spriteState.selectedSprite = hoverButtonSprite;
                btn.spriteState = spriteState;
                btn.transition = Selectable.Transition.SpriteSwap;
            }
        }

        /// <summary>显示指定结局</summary>
        public void ShowEnding(int endingId, string endingName, string description, bool deathEnding)
        {
            currentEndingId = endingId;
            isDeathEnding = deathEnding;

            if (endingNameText != null)
                endingNameText.text = endingName;

            if (endingDescriptionText != null)
                endingDescriptionText.text = description;

            // 死亡结局按钮文本区分
            var restartTxt = restartButton?.GetComponentInChildren<Text>();
            if (restartTxt != null)
                restartTxt.text = deathEnding ? "重新开始" : "从头开始";

            gameObject.SetActive(true);

            Debug.Log($"[EndingScreenController] 显示结局 [{endingId}] {endingName} 死亡={deathEnding}");
        }

        void OnRestartClick()
        {
            Debug.Log($"[EndingScreenController] 重新开始 结局={currentEndingId} 死亡={isDeathEnding}");

            // 触发预留接口（供存档系统接入）
            OnRestartRequested?.Invoke();

            if (isDeathEnding)
            {
                // 死亡结局：从本关游玩部分开始
                // MVP：直接重载当前关卡场景
                // TODO：接入存档系统后，恢复本关中间状态
                if (!string.IsNullOrEmpty(currentLevelSceneName))
                    SceneManager.LoadScene(currentLevelSceneName);
            }
            else
            {
                // 其他结局：从头开始（重载当前场景）
                if (!string.IsNullOrEmpty(currentLevelSceneName))
                    SceneManager.LoadScene(currentLevelSceneName);
            }
        }

        void OnReturnToMenuClick()
        {
            Debug.Log("[EndingScreenController] 返回主界面");

            OnReturnToMainMenuRequested?.Invoke();

            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>通过结局数据直接显示</summary>
        public void ShowEndingFromData(Game.Config.EndingEntry entry, bool deathEnding = false)
        {
            if (entry == null) return;
            ShowEnding(entry.id, entry.endingName, entry.description, deathEnding);
        }
    }
}
