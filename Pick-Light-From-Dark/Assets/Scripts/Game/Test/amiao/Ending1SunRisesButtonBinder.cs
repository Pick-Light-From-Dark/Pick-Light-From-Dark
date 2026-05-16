using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Test
{
    /// <summary>
    /// 结局一预制体（Ending1_SunRises）按钮绑定：Prefab 上 Button 的 OnClick 为空时使用。
    /// LoadSaveButton → 从头开始（重载关卡）；ReturnButton → 返回主界面。
    /// </summary>
    public class Ending1SunRisesButtonBinder : MonoBehaviour
    {
        [SerializeField] string restartSceneName = "Level1";
        [SerializeField] string mainMenuSceneName = "GameScene";

        void Awake()
        {
            WireButton("LoadSaveButton", () =>
            {
                if (!string.IsNullOrEmpty(restartSceneName))
                    SceneManager.LoadScene(restartSceneName);
            });

            WireButton("ReturnButton", () =>
            {
                if (!string.IsNullOrEmpty(mainMenuSceneName))
                    SceneManager.LoadScene(mainMenuSceneName);
            });
        }

        void WireButton(string childName, UnityEngine.Events.UnityAction onClick)
        {
            var t = transform.Find(childName);
            if (t == null)
            {
                Debug.LogWarning($"[Ending1SunRisesButtonBinder] 未找到子物体: {childName}");
                return;
            }

            var btn = t.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning($"[Ending1SunRisesButtonBinder] {childName} 无 Button 组件");
                return;
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(onClick);
        }
    }
}
