using UnityEngine;
using UnityEngine.SceneManagement;

public class BeginPanel : BasePanel
{
    public override void HideMe() { }

    public override void ShowMe() { }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "StartBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.HidePanel<BeginPanel>(true);
                SceneManager.LoadScene("Level1");
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
