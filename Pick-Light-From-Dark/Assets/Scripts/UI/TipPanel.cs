using Game.Flow;

[UIPath("UI/Content")]
public class TipPanel : BasePanel
{
    public override void HideMe() { }
    public override void ShowMe() { }

    protected override void ClickBtn(string btnName)
    {
        var coordinator = LevelFlowCoordinator.Instance;
        switch (btnName)
        {
            case "RetryBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.HideAllPanels();
                if (coordinator != null && !string.IsNullOrEmpty(coordinator.CurrentLevelSceneName))
                    SceneMgr.Instance.LoadScene(coordinator.CurrentLevelSceneName);
                else
                    SceneMgr.Instance.LoadScene("GameScene");
                break;

            case "BackBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.HideAllPanels();
                SceneMgr.Instance.LoadScene("GameScene");
                break;
        }
    }
}
