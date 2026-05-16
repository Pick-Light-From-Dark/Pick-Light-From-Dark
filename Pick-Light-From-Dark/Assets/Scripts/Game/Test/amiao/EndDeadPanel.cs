using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 死亡结局面板 — 失败时显示
    /// ReturnButton: 重新开始本关
    /// ReturnGamePanel: 返回主界面
    /// LoadSaveButton: 打开存档面板
    /// </summary>
    public class EndDeadPanel : MonoBehaviour
    {
        [Header("UI 组件（运行时自动查找子对象）")]
        [SerializeField] private Image endingImage;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button returnGamePanelBtn;
        [SerializeField] private Button loadSaveButton;

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
            if (returnGamePanelBtn == null)
                returnGamePanelBtn = transform.Find("ReturnGamePanel")?.GetComponent<Button>();
            if (loadSaveButton == null)
                loadSaveButton = transform.Find("LoadSaveButton")?.GetComponent<Button>();
        }

        void BindButtons()
        {
            if (returnButton != null)
                returnButton.onClick.AddListener(OnRestartClick);

            if (returnGamePanelBtn != null)
                returnGamePanelBtn.onClick.AddListener(OnReturnClick);

            if (loadSaveButton != null)
                loadSaveButton.onClick.AddListener(OnLoadSaveClick);
        }

        void OnRestartClick()
        {
            Debug.Log("[EndDeadPanel] 重新开始本关");
            gameObject.SetActive(false);
            string currentScene = SceneManager.GetActiveScene().name;
            SceneMgr.Instance.LoadScene(currentScene);
        }

        void OnReturnClick()
        {
            Debug.Log("[EndDeadPanel] 返回主界面");
            gameObject.SetActive(false);
            UIMgr.Instance.HideAllPanels();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        }

        void OnLoadSaveClick()
        {
            Debug.Log("[EndDeadPanel] 打开存档面板");
            gameObject.SetActive(false);
            UIMgr.Instance.ShowPanel<SaveGamePanel>();
        }

        void OnDestroy()
        {
            if (returnButton != null)
                returnButton.onClick.RemoveListener(OnRestartClick);
            if (returnGamePanelBtn != null)
                returnGamePanelBtn.onClick.RemoveListener(OnReturnClick);
            if (loadSaveButton != null)
                loadSaveButton.onClick.RemoveListener(OnLoadSaveClick);
        }
    }
}
