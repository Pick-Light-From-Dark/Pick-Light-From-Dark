using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 死亡结局面板 — 读档按钮打开存档面板，返回按钮回到主界面
    /// </summary>
    public class EndDeadPanel : MonoBehaviour
    {
        [Header("UI 组件（运行时自动查找子对象）")]
        [SerializeField] private Image endingImage;
        [SerializeField] private Button returnButton;
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
            if (loadSaveButton == null)
                loadSaveButton = transform.Find("LoadSaveButton")?.GetComponent<Button>();
        }

        void BindButtons()
        {
            if (returnButton != null)
                returnButton.onClick.AddListener(OnReturnClick);

            if (loadSaveButton != null)
                loadSaveButton.onClick.AddListener(OnLoadSaveClick);
        }

        void OnReturnClick()
        {
            Debug.Log("[EndDeadPanel] 返回主界面");
            SceneManager.LoadScene("GameScene");
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
                returnButton.onClick.RemoveListener(OnReturnClick);
            if (loadSaveButton != null)
                loadSaveButton.onClick.RemoveListener(OnLoadSaveClick);
        }
    }
}
