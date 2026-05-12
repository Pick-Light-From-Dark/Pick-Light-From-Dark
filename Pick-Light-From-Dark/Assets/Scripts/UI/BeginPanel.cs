using UnityEngine;

public class BeginPanel : BasePanel
{
    [Header("对话文本")]
    public TextAsset dialogueText;

    public override void HideMe() { }

    public override void ShowMe() { }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "StartBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                // 启动关卡流程协调器（三段式：剧情→游玩→结尾）
                var coordinator = FindObjectOfType<Game.Flow.LevelFlowCoordinator>();
                if (coordinator != null)
                {
                    UIMgr.Instance.HidePanel<BeginPanel>();
                    // LevelFlowCoordinator 在 Start() 中已自动开始流程
                }
                else
                {
                    Debug.LogWarning("[BeginPanel] 场景中未找到 LevelFlowCoordinator，直接进游戏");
                    UIMgr.Instance.ShowPanel<GamePanel>();
                    UIMgr.Instance.HidePanel<BeginPanel>();
                }
                break;

            case "SaveBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.ShowPanel<SaveGamePanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "SettingBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.ShowPanel<SettingPanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "ExperienceBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.ShowPanel<ExperiencePanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "AboutUsBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.ShowPanel<AboutUsPanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "QuitBtn":
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;
        }
    }
}
