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
                // 游戏中打开 → 返回暂停面板；主菜单打开 → 返回开始界面
                if (GamePanel.Instance != null && GamePanel.Instance.gameObject.activeInHierarchy)
                    UIMgr.Instance.ShowPanel<StopGamePanel>();
                else
                    UIMgr.Instance.ShowPanel<BeginPanel>();
                break;
        }
    }
}
