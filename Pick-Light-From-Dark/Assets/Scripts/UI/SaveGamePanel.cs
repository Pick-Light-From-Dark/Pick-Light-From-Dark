using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
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
        RefreshTitle();
    }

    void DetermineMode()
    {
        isSaveMode = IsInGameplay();
    }

    /// <summary>判断当前是否处于游戏内（关卡中），兼容多场景加载</summary>
    bool IsInGameplay()
    {
        // 如果有主菜单场景在加载中，一定不是游戏内
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == "GameScene")
                return false;
        }
        // 否则根据当前激活场景判断
        return SceneManager.GetActiveScene().name.StartsWith("Level");
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

    void RefreshTitle()
    {
        var title = transform.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
        if (title != null)
            title.text = isSaveMode ? "存档" : "读档";
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
                // 在游戏内 → 返回暂停面板；在主菜单/结局面板 → 返回开始界面
                if (IsInGameplay())
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
            // checkpoint 为空，尝试从场景名自动推断关卡并创建初始存档
            saveSystem.SaveCurrentProgress();
            cp = saveSystem.currentSave?.checkpoint;
            if (cp == null || cp.currentLevelId <= 0)
            {
                Debug.LogWarning("[SaveGamePanel] 无存档信息可保存");
                return;
            }
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
