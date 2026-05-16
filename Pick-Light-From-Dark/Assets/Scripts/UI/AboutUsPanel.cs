using UnityEngine;
using UnityEngine.UI;

public class AboutUsPanel : BasePanel
{
    private bool scrollSetupDone;

    public override void HideMe()
    {
        scrollSetupDone = false;
    }

    public override void ShowMe()
    {
        if (!scrollSetupDone)
            SetupScrollView();
        scrollSetupDone = true;
    }

    private void SetupScrollView()
    {
        var scrollView = transform.Find("Scroll View");
        if (scrollView == null) return;

        var viewport = scrollView.Find("Viewport");
        if (viewport == null) return;

        var content = viewport.Find("Content");
        if (content == null) return;

        var scrollRect = scrollView.GetComponent<ScrollRect>();
        if (scrollRect != null) scrollRect.content = content as RectTransform;

        var fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
            layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // 将 AboutImg 移入 Content 中
        var aboutImg = transform.Find("AboutImg");
        if (aboutImg == null)
        {
            // 未找到则动态创建
            aboutImg = new GameObject("AboutImg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).transform;
        }
        aboutImg.SetParent(content, false);
        aboutImg.SetAsFirstSibling();

        var imgRt = aboutImg as RectTransform;
        imgRt.pivot = new Vector2(0.5f, 1f);
        imgRt.anchorMin = new Vector2(0, 1f);
        imgRt.anchorMax = new Vector2(1, 1f);

        var img = aboutImg.GetComponent<Image>();
        img.preserveAspect = true;

        // 根据图片宽高比和 Content 宽度设置尺寸
        if (img.sprite != null)
        {
            float contentWidth = ((RectTransform)content).rect.width;
            if (contentWidth <= 0) contentWidth = 800f;
            float ratio = (float)img.sprite.texture.width / img.sprite.texture.height;
            imgRt.sizeDelta = new Vector2(0, contentWidth / ratio);
        }
        else
        {
            imgRt.sizeDelta = new Vector2(0, 600f);
        }
    }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "BackBtn":
                UIMgr.Instance.HidePanel<AboutUsPanel>();
                UIMgr.Instance.ShowPanel<BeginPanel>();
                break;
        }
    }

    void Start() { }
    void Update() { }
}
