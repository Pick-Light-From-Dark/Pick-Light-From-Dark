using UnityEngine;

[UIPath("UI/MainFlow")]
public class SaveGamePanel : BasePanel
{
    public override void HideMe() { }

    public override void ShowMe() { }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "BackBtn":
                UIMgr.Instance.HidePanel<SaveGamePanel>();
                break;
        }
    }
}
