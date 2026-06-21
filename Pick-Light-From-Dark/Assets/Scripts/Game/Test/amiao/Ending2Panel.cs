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
            BindImageClick();
        }

        void AutoFindComponents()
        {
            if (endingImage == null)
                endingImage = GetComponent<Image>();
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

        /// <summary>结局图片本身也支持点击返回</summary>
        void BindImageClick()
        {
            if (endingImage == null) return;
            endingImage.raycastTarget = true;
            var existingBtn = endingImage.GetComponent<Button>();
            if (existingBtn == null)
            {
                var btn = endingImage.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(OnReturnClick);
            }
            else
            {
                existingBtn.onClick.AddListener(OnReturnClick);
            }
        }

        void OnReturnClick()
        {
            Debug.Log("[Ending2Panel] 返回主界面");
            CrossLevelSaveSystem.Instance?.MarkGameCompleted();
            MusicMgr.Instance?.StopBKMusic();
            SceneManager.LoadScene("GameScene");
        }

        void OnLoadSaveClick()
        {
            Debug.Log("[Ending2Panel] 打开存档面板");
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
