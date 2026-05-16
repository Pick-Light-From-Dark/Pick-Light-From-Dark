using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BeginPanel : BasePanel
{
    public override void HideMe() { }

    public override void ShowMe()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        MusicMgr.Instance.PlayBKMusic("BGM/Blind Spot");
        SetupAllButtonHover();
    }

    private void SetupAllButtonHover()
    {
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            SetupButtonHover(btn);
        }
    }

    private void SetupButtonHover(Button btn)
    {
        if (btn == null) return;
        var hoverImage = btn.transform.Find("Image");
        if (hoverImage == null) return;
        hoverImage.gameObject.SetActive(false);

        var trigger = btn.gameObject.GetComponent<HoverImageTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<HoverImageTrigger>();
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
