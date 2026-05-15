using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 结局二（莫比乌斯环）前端面板 — 纯展示，按钮提供基础导航
    /// </summary>
    public class Ending2Panel : MonoBehaviour
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
            Debug.Log("[Ending2Panel] 返回主界面");
            SceneManager.LoadScene("GameScene");
        }

        void OnLoadSaveClick()
        {
            Debug.Log("[Ending2Panel] 读取存档 - 待接入存档面板");
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
