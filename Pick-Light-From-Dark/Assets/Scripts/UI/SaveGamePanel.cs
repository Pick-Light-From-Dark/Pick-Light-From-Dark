using UnityEngine;
using Game.Test;

[UIPath("UI/MainFlow")]
public class SaveGamePanel : BasePanel
{
    private bool isSaveMode;

    public override void HideMe() { }

    public override void ShowMe()
    {
        DetermineMode();
    }

    void DetermineMode()
    {
        // 游戏中打开 → 存档模式；主菜单/结局面板打开 → 读档模式
        isSaveMode = GamePanel.Instance != null && GamePanel.Instance.gameObject.activeInHierarchy;
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
                // 游戏中打开 → 返回暂停面板；主菜单打开 → 返回开始界面
                if (GamePanel.Instance != null && GamePanel.Instance.gameObject.activeInHierarchy)
                    UIMgr.Instance.ShowPanel<StopGamePanel>();
                else
                    UIMgr.Instance.ShowPanel<BeginPanel>();
                break;
        }
    }

    void DoSave()
    {
        var saveSystem = CrossLevelSaveSystem.Instance;
        if (saveSystem == null)
        {
            Debug.LogWarning("[SaveGamePanel] CrossLevelSaveSystem 未初始化");
            return;
        }

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
        var saveSystem = CrossLevelSaveSystem.Instance;
        if (saveSystem == null || !saveSystem.HasSave())
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
