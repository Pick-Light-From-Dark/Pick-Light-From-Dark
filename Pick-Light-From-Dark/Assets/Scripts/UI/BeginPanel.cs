using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginPanel : BasePanel
{
    public override void HideMe()
    {
        
    }

    public override void ShowMe()
    {
        
    }
    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "StartBtn":
                UIMgr.Instance.ShowPanel<GamePanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
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
