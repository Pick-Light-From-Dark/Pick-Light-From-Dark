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
                UIMgr.Instance.HideAllPanels();
                if (coordinator != null && !string.IsNullOrEmpty(coordinator.CurrentLevelSceneName))
                    SceneMgr.Instance.LoadScene(coordinator.CurrentLevelSceneName);
                else
                    SceneMgr.Instance.LoadScene("GameScene");
                break;

            case "BackBtn":
                UIMgr.Instance.HideAllPanels();
                SceneMgr.Instance.LoadScene("GameScene");
                break;
        }
    }
}
