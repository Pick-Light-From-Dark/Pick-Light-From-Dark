using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[UIPath("UI/Content")]
public class StopGamePanel : BasePanel
{
    public override void ShowMe()
    {
        SetupButtonHover("ContinueGameBtn");
        SetupButtonHover("SaveGameBtn");
        SetupButtonHover("SettingBtn");
        SetupButtonHover("QuitGameBtn");
    }

    public override void HideMe() { }

    private void SetupButtonHover(string btnName)
    {
        var btnT = transform.Find($"StopImgBk/{btnName}");
        if (btnT == null) return;

        var hoverImage = btnT.Find("Image");
        if (hoverImage == null) return;
        hoverImage.gameObject.SetActive(false);

        var trigger = btnT.gameObject.GetComponent<HoverImageTrigger>();
        if (trigger == null)
            trigger = btnT.gameObject.AddComponent<HoverImageTrigger>();
        trigger.hoverImage = hoverImage.gameObject;
    }

    private class HoverImageTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject hoverImage;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverImage != null) hoverImage.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverImage != null) hoverImage.SetActive(false);
        }
    }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "ContinueGameBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                Game.Flow.GameFlowController.Instance.ResumeGame();
                UIMgr.Instance.HidePanel<StopGamePanel>();
                break;

            case "SaveGameBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.HidePanel<StopGamePanel>();
                UIMgr.Instance.ShowPanel<SaveGamePanel>();
                break;

            case "SettingBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                UIMgr.Instance.HidePanel<StopGamePanel>();
                UIMgr.Instance.ShowPanel<SettingPanel>();
                break;

            case "QuitGameBtn":
                MusicMgr.Instance.PlaySound("按钮点击音效");
                Game.Flow.GameFlowController.Instance.GameLose("主动退出");
                UIMgr.Instance.HideAllPanels();
                UIMgr.Instance.ShowPanel<BeginPanel>();
                break;
        }
    }
}
