using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 死亡结局面板 — 失败时显示，点击回到主界面
    /// </summary>
    public class DeadEndPanel : MonoBehaviour
    {
        void Start()
        {
            var endingImage = GetComponent<Image>();
            if (endingImage == null)
                endingImage = transform.Find("EndingImage")?.GetComponent<Image>();

            if (endingImage != null)
            {
                endingImage.raycastTarget = true;
                var btn = endingImage.gameObject.GetComponent<Button>();
                if (btn == null) btn = endingImage.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(OnAnyClick);
            }
        }

        void OnAnyClick()
        {
            Debug.Log("[DeadEndPanel] 点击回到主界面");
            CrossLevelSaveSystem.Instance?.ResetEndingState();
            UIMgr.Instance.HideAllPanels();
            SceneMgr.Instance.LoadScene("GameScene");
        }
    }
}
