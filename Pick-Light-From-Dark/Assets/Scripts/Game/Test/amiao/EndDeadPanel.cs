using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 死亡结局面板 — 重新开始按钮从本关游玩部分开始
    /// </summary>
    public class EndDeadPanel : MonoBehaviour
    {
        [Header("UI 组件（运行时自动查找子对象）")]
        [SerializeField] private Image endingImage;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button restartButton;

        void Start()
        {
            AutoFindComponents();
            BindButtons();
        }

        void AutoFindComponents()
        {
            if (endingImage == null)
                endingImage = transform.Find("EndingImage")?.GetComponent<Image>();
            if (returnButton == null)
                returnButton = transform.Find("ReturnButton")?.GetComponent<Button>();
            if (restartButton == null)
                restartButton = transform.Find("RestartButton")?.GetComponent<Button>();
        }

        void BindButtons()
        {
            if (returnButton != null)
                returnButton.onClick.AddListener(OnReturnClick);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClick);
        }

        void OnReturnClick()
        {
            Debug.Log("[EndDeadPanel] 返回主界面");
            SceneManager.LoadScene("GameScene");
        }

        void OnRestartClick()
        {
            Debug.Log("[EndDeadPanel] 重新开始 — 从本关游玩部分开始（预留接口）");
            // TODO: 接入存档系统，读取当前关卡进度并从游玩部分开始
        }

        void OnDestroy()
        {
            if (returnButton != null)
                returnButton.onClick.RemoveListener(OnReturnClick);
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClick);
        }
    }
}
