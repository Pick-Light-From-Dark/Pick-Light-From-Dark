using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Flow;

[UIPath("UI/Content")]
public class EndingContentPanel : BasePanel
{
    public override void HideMe() { }
    public override void ShowMe() { }

    protected override void ClickBtn(string btnName)
    {
        var coordinator = LevelFlowCoordinator.Instance;
        switch (btnName)
        {
            case "ContinueBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.HideAllPanels();
                if (coordinator != null && !string.IsNullOrEmpty(coordinator.NextLevelSceneName))
                    SceneManager.LoadScene(coordinator.NextLevelSceneName);
                else
                    SceneManager.LoadScene("GameScene");
                break;

            case "BackBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.HideAllPanels();
                SceneManager.LoadScene("GameScene");
                break;
        }
    }
}