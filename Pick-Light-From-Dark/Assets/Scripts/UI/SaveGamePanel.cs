using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Game.Test;

[UIPath("UI/MainFlow")]
public class SaveGamePanel : BasePanel
{
    private bool isSaveMode;

    public override void HideMe() { }

    public override void ShowMe()
    {
        EnsureSaveSystem();
        DetermineMode();
        RefreshButtonState();
    }

    void DetermineMode()
    {
        // 在关卡场景中打开 → 存档模式；其他场景（主菜单/结局面板）→ 读档模式
        string sceneName = SceneManager.GetActiveScene().name;
        isSaveMode = sceneName.StartsWith("Level");
    }

    CrossLevelSaveSystem EnsureSaveSystem()
    {
        if (CrossLevelSaveSystem.Instance == null)
        {
            var go = new GameObject("CrossLevelSaveSystem");
            go.AddComponent<CrossLevelSaveSystem>();
            Debug.Log("[SaveGamePanel] 自动创建 CrossLevelSaveSystem");
        }
        return CrossLevelSaveSystem.Instance;
    }

    void RefreshButtonState()
    {
        var btn = transform.Find("SaveGameBtn1")?.GetComponent<Button>();
        if (btn == null) return;

        var saveSystem = CrossLevelSaveSystem.Instance;
        if (!isSaveMode)
        {
            // 读档模式：无存档时禁用按钮
            btn.interactable = saveSystem != null && saveSystem.HasSave();
        }
        else
        {
            // 存档模式：始终可点击（即使无存档信息也会提示）
            btn.interactable = true;
        }
    }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "SaveGameBtn1":
                if (isSaveMode)
                    DoSave();
                else
                    DoLoad();
                break;

            case "BackBtn":
                MusicMgr.Instance?.PlaySound("按钮点击音效");
                UIMgr.Instance.HidePanel<SaveGamePanel>();
                // 在关卡场景中 → 返回暂停面板；其他场景 → 返回开始界面
                if (SceneManager.GetActiveScene().name.StartsWith("Level"))
                    UIMgr.Instance.ShowPanel<StopGamePanel>();
                else
                    UIMgr.Instance.ShowPanel<BeginPanel>();
                break;
        }
    }

    void DoSave()
    {
        var saveSystem = EnsureSaveSystem();

        var cp = saveSystem.currentSave?.checkpoint;
        if (cp == null || cp.currentLevelId <= 0)
        {
            Debug.LogWarning("[SaveGamePanel] 无存档信息可保存");
            return;
        }

        MusicMgr.Instance?.PlaySound("按钮点击音效");

        if (cp.isInGameplay)
            saveSystem.SaveGameplayProgress(cp.currentLevelId);
        else
            saveSystem.SaveStoryProgress(cp.currentLevelId, cp.storyFileName, 0);

        Debug.Log($"[SaveGamePanel] 已保存 Lv.{cp.currentLevelId}");
    }

    void DoLoad()
    {
        var saveSystem = EnsureSaveSystem();
        if (!saveSystem.HasSave())
        {
            Debug.LogWarning("[SaveGamePanel] 无存档可读取");
            return;
        }

        var cp = saveSystem.LoadCheckpoint();
        MusicMgr.Instance?.PlaySound("按钮点击音效");
        UIMgr.Instance.HidePanel<SaveGamePanel>(true);

        string sceneName = $"Level{cp.currentLevelId}";
        SceneMgr.Instance.LoadScene(sceneName);
        Debug.Log($"[SaveGamePanel] 读档到 {sceneName} | 剧情: {cp.storyFileName}");
    }
}
