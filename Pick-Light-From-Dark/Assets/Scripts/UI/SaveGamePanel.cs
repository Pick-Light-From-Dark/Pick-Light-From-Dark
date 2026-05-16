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

        Debug.Log($"[SaveGamePanel] ShowMe 调用: isSaveMode={isSaveMode}, 激活场景={SceneManager.GetActiveScene().name}");
    }

    void DetermineMode()
    {
        isSaveMode = IsInGameplay();
    }

    /// <summary>判断当前是否处于游戏内（关卡中），兼容多场景加载</summary>
    bool IsInGameplay()
    {
        Debug.Log($"[SaveGamePanel] IsInGameplay 检测开始: 场景数={SceneManager.sceneCount}, 激活场景={SceneManager.GetActiveScene().name}");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string sceneName = SceneManager.GetSceneAt(i).name;
            Debug.Log($"[SaveGamePanel]   场景[{i}]: {sceneName}");
            if (sceneName == "GameScene")
                return false;
        }
        bool result = SceneManager.GetActiveScene().name.StartsWith("Level");
        Debug.Log($"[SaveGamePanel] IsInGameplay 结果: {result}");
        return result;
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

        var text = btn.GetComponentInChildren<Text>(true);
        if (text == null)
        {
            var textObj = new GameObject("SaveText");
            textObj.transform.SetParent(btn.transform, false);
            var rt = textObj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            text = textObj.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.fontSize = 28;
            var font = Resources.Load<Font>("Font/LXGWWenKaiScreen");
            if (font == null)
                font = Resources.Load<Font>("Font/SourceHanSansHWSC-Regular");
            if (font != null)
                text.font = font;
        }

        var saveSystem = CrossLevelSaveSystem.Instance;
        if (saveSystem != null && saveSystem.HasSave())
        {
            var cp = saveSystem.currentSave.checkpoint;
            var time = System.DateTimeOffset.FromUnixTimeMilliseconds(cp.saveTime).LocalDateTime;
            text.text = $"Lv.{cp.currentLevelId} {time:MM-dd HH:mm}";
        }
        else
        {
            text.text = isSaveMode ? "存档" : "无存档";
        }

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

        // 先从当前场景推断关卡ID，确保不保存过期的关卡号
        string sceneName = SceneManager.GetActiveScene().name;
        int inferredLevelId = 0;
        if (sceneName.StartsWith("Level"))
        {
            string levelStr = sceneName.Substring(5);
            int.TryParse(levelStr, out inferredLevelId);
        }

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
        else if (inferredLevelId > 0 && inferredLevelId != cp.currentLevelId)
        {
            // 当前场景推断的关卡号与 checkpoint 不一致，以当前场景为准
            cp.currentLevelId = inferredLevelId;
        }

        MusicMgr.Instance?.PlaySound("按钮点击音效");

        if (cp.isInGameplay)
            saveSystem.SaveGameplayProgress(cp.currentLevelId);
        else
            saveSystem.SaveStoryProgress(cp.currentLevelId, cp.storyFileName, 0);

        Debug.Log($"[SaveGamePanel] 已保存 Lv.{cp.currentLevelId}");
        RefreshButtonState();
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
