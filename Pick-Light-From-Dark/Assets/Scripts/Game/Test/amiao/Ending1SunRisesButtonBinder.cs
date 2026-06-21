using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 结局一（太阳照常升起）按钮绑定 — 点击回到主界面
    /// </summary>
    public class Ending1SunRisesButtonBinder : MonoBehaviour
    {
        void Start()
        {
            var img = GetComponent<Image>();
            if (img == null) img = transform.Find("EndingImage")?.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                var btn = img.gameObject.GetComponent<Button>();
                if (btn == null) btn = img.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    CrossLevelSaveSystem.Instance?.MarkGameCompleted();
                    MusicMgr.Instance?.StopBKMusic();
                    SceneManager.LoadScene("GameScene");
                });
            }
        }
    }
}
