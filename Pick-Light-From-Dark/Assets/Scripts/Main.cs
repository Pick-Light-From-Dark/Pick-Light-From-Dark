using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 主菜单入口：仅在 GameScene 显示开始界面。
/// 关卡场景（Level1~5 等）也挂了本组件时，不得再次弹出 BeginPanel，否则切场景会闪主菜单。
/// </summary>
public class Main : MonoBehaviour
{
    const string MainMenuSceneName = "GameScene";

    static bool sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterSceneHook()
    {
        if (sceneHookRegistered) return;
        sceneHookRegistered = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MainMenuSceneName) return;
        UIMgr.Instance?.HidePanel<BeginPanel>(true);
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != MainMenuSceneName)
            return;

        UIMgr.Instance.ShowPanel<BeginPanel>();
    }
}
