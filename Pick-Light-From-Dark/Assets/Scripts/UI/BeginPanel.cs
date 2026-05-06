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
                if (dialogueText == null)
                {
                    Debug.LogError("BeginPanel: dialogueText 未赋值！请在 Inspector 中将对话文本文件拖到 BeginPanel 的 dialogueText 字段");
                    return;
                }

                MusicMgr.Instance.PlaySound("按钮点击音效");
                // UIMgr.Instance.ShowPanel<GalDialoguePanel>(
                //     E_UILayer.Middle,
                //     (panel) =>
                //     {
                //         if (panel == null)
                //         {
                //             Debug.LogError("GalDialoguePanel 加载失败！");
                //             return;
                //         }
                //         DialogueSystem.Instance.BindPanel(panel);
                //         DialogueSystem.Instance.StartDialogue(dialogueText, DialogueSystem.DialogueMode.Gal);
                //     }
                // );
                UIMgr.Instance.ShowPanel<GamePanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
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
