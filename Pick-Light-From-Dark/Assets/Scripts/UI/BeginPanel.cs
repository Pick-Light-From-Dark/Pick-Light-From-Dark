using UnityEngine;

public class BeginPanel : BasePanel
{
    public override void HideMe() { }

    public override void ShowMe()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        MusicMgr.Instance.PlayBKMusic("BGM/Blind Spot");
    }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "StartBtn":
                MusicMgr.Instance.StopBKMusic();
                UIMgr.Instance.HidePanel<BeginPanel>(true);
                SceneMgr.Instance.LoadScene("Level1");
                break;

            case "SaveBtn":
                UIMgr.Instance.ShowPanel<SaveGamePanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "SettingBtn":
                UIMgr.Instance.ShowPanel<SettingPanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "ExperienceBtn":
                UIMgr.Instance.ShowPanel<ExperiencePanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "AboutUsBtn":
                UIMgr.Instance.ShowPanel<AboutUsPanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;

            case "QuitBtn":
                UIMgr.Instance.HidePanel<BeginPanel>();
                break;
        }
    }
}
