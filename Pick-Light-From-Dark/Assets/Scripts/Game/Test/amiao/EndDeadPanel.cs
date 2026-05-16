using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
<<<<<<< HEAD
    /// 死亡结局面板 — 读档按钮打开存档面板，返回按钮回到主界面
=======
    /// 死亡结局面板 — 失败时显示
    /// ReturnButton: 重新开始本关
    /// ReturnGamePanel: 返回主界面
>>>>>>> origin/develop
    /// </summary>
    public class EndDeadPanel : MonoBehaviour
    {
        [Header("UI 组件（运行时自动查找子对象）")]
        [SerializeField] private Image endingImage;
        [SerializeField] private Button returnButton;
<<<<<<< HEAD
        [SerializeField] private Button loadSaveButton;
=======
        [SerializeField] private Button returnGamePanelBtn;
>>>>>>> origin/develop

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
<<<<<<< HEAD
            if (loadSaveButton == null)
                loadSaveButton = transform.Find("LoadSaveButton")?.GetComponent<Button>();
=======
            if (returnGamePanelBtn == null)
                returnGamePanelBtn = transform.Find("ReturnGamePanel")?.GetComponent<Button>();
>>>>>>> origin/develop
        }

        void BindButtons()
        {
            if (returnButton != null)
                returnButton.onClick.AddListener(OnRestartClick);

<<<<<<< HEAD
            if (loadSaveButton != null)
                loadSaveButton.onClick.AddListener(OnLoadSaveClick);
=======
            if (returnGamePanelBtn != null)
                returnGamePanelBtn.onClick.AddListener(OnReturnClick);
        }

        void OnRestartClick()
        {
            Debug.Log("[EndDeadPanel] 重新开始本关");
            gameObject.SetActive(false);
            string currentScene = SceneManager.GetActiveScene().name;
            SceneMgr.Instance.LoadScene(currentScene);
>>>>>>> origin/develop
        }

        void OnReturnClick()
        {
            Debug.Log("[EndDeadPanel] 返回主界面");
<<<<<<< HEAD
            SceneManager.LoadScene("GameScene");
        }

        void OnLoadSaveClick()
        {
            Debug.Log("[EndDeadPanel] 打开存档面板");
            gameObject.SetActive(false);
            UIMgr.Instance.ShowPanel<SaveGamePanel>();
=======
            gameObject.SetActive(false);
            UIMgr.Instance.HideAllPanels();
            UIMgr.Instance.ShowPanel<BeginPanel>();
>>>>>>> origin/develop
        }

        void OnDestroy()
        {
            if (returnButton != null)
<<<<<<< HEAD
                returnButton.onClick.RemoveListener(OnReturnClick);
            if (loadSaveButton != null)
                loadSaveButton.onClick.RemoveListener(OnLoadSaveClick);
=======
                returnButton.onClick.RemoveListener(OnRestartClick);
            if (returnGamePanelBtn != null)
                returnGamePanelBtn.onClick.RemoveListener(OnReturnClick);
>>>>>>> origin/develop
        }
    }
}
