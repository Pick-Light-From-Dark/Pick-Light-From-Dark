using UnityEngine;

[UIPath("UI/Content")]
public class StopGamePanel : BasePanel
{
    public override void ShowMe() { }

    public override void HideMe() { }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "ContinueGameBtn":
                Game.Flow.GameFlowController.Instance.ResumeGame();
                UIMgr.Instance.HidePanel<StopGamePanel>();
                break;

            case "SettingBtn":
                UIMgr.Instance.ShowPanel<SettingPanel>();
                break;

            case "QuitGameBtn":
                Game.Flow.GameFlowController.Instance.ResumeGame();
                UIMgr.Instance.HideAllPanels();
                UIMgr.Instance.ShowPanel<BeginPanel>();
                break;
        }
    }
}
