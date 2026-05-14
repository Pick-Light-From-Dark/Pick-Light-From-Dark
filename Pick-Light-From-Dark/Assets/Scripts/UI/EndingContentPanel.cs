using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Game.Config;
using Game.Flow;

[UIPath("UI/Content")]
public class EndingContentPanel : BasePanel
{
    [Header("结局名称文本")]
    [SerializeField] private TextMeshProUGUI endingNameText;

    [Header("结局序号文本")]
    [SerializeField] private TextMeshProUGUI endingCountText;

    [Header("结局描述文本")]
    [SerializeField] private TextMeshProUGUI endingDesText;

    [Header("结局图片")]
    [SerializeField] private Image endingImage;

    private EndingDataSO endingData;
    private int currentEndingId;
    private bool buttonsCreated = false;

    protected override void Awake()
    {
        base.Awake();
        AutoFindUIComponents();
    }

    public override void HideMe() { }

    public override void ShowMe()
    {
        if (!buttonsCreated)
        {
            CreateActionButtons();
            buttonsCreated = true;
        }
    }

    /// <summary>
    /// 自动查找子对象中的UI组件（Prefab未手动赋值时的fallback）
    /// </summary>
    void AutoFindUIComponents()
    {
        if (endingNameText == null)
            endingNameText = FindTextInChildren("EndingNameText");
        if (endingCountText == null)
            endingCountText = FindTextInChildren("EndingCount");
        if (endingDesText == null)
            endingDesText = FindTextInChildren("EndingDesText");
        if (endingImage == null)
            endingImage = FindImageInChildren("EndingImg");
    }

    TextMeshProUGUI FindTextInChildren(string name)
    {
        var t = transform.Find(name);
        if (t != null) return t.GetComponent<TextMeshProUGUI>();
        // 深度搜索
        foreach (var txt in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (txt.gameObject.name == name) return txt;
        }
        return null;
    }

    Image FindImageInChildren(string name)
    {
        var t = transform.Find(name);
        if (t != null) return t.GetComponent<Image>();
        foreach (var img in GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject.name == name) return img;
        }
        return null;
    }

    /// <summary>
    /// 动态创建底部操作按钮
    /// </summary>
    void CreateActionButtons()
    {
        // 查找或创建按钮父节点
        Transform btnParent = transform.Find("EndingImgBk");
        if (btnParent == null) btnParent = transform;

        // 创建返回主界面按钮
        var menuBtn = CreateButton(btnParent, "ReturnToMenuBtn", "返回主界面", new Vector2(-140, -160));
        RegisterControl("ReturnToMenuBtn", menuBtn);
        BindControlEvent(menuBtn, "ReturnToMenuBtn");

        // 创建读取存档按钮
        var saveBtn = CreateButton(btnParent, "LoadSaveBtn", "读取存档", new Vector2(140, -160));
        RegisterControl("LoadSaveBtn", saveBtn);
        BindControlEvent(saveBtn, "LoadSaveBtn");

        Debug.Log("[EndingContentPanel] 操作按钮已动态创建");
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(200, 60);

        go.AddComponent<CanvasRenderer>();

        var img = go.AddComponent<Image>();
        img.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        // 创建文本子对象
        var txtGo = new GameObject("Text");
        txtGo.transform.SetParent(go.transform, false);
        var txtRt = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        txtGo.AddComponent<CanvasRenderer>();
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        return btn;
    }

    void BindControlEvent(Button btn, string controlName)
    {
        btn.onClick.AddListener(() => ClickBtn(controlName));
    }

    /// <summary>
    /// 根据结局ID设置结局面板内容
    /// </summary>
    public void SetupEnding(int endingId, EndingDataSO data)
    {
        endingData = data;
        currentEndingId = endingId;
        AutoFindUIComponents();

        if (endingData == null)
        {
            Debug.LogError("[EndingContentPanel] SetupEnding 失败：endingData 为 null");
            return;
        }

        var entry = endingData.GetEndingById(endingId);
        if (entry == null)
        {
            Debug.LogError($"[EndingContentPanel] 找不到结局ID: {endingId}");
            return;
        }

        if (endingNameText != null)
            endingNameText.text = entry.endingName;

        if (endingCountText != null)
            endingCountText.text = endingData.GetEndingCountText(endingId);

        if (endingDesText != null)
            endingDesText.text = entry.description;

        if (endingImage != null && entry.endingSprite != null)
            endingImage.sprite = entry.endingSprite;

        Debug.Log($"[EndingContentPanel] 显示结局 [{endingId}] {entry.endingName}");
    }

    protected override void ClickBtn(string btnName)
    {
        switch (btnName)
        {
            case "ContinueBtn":
                MusicMgr.Instance?.PlaySound("按钮点击音效");
                ContinueGame();
                break;

            case "BackBtn":
                MusicMgr.Instance?.PlaySound("按钮点击音效");
                BackToMenu();
                break;

            case "ReturnToMenuBtn":
                MusicMgr.Instance?.PlaySound("按钮点击音效");
                ReturnToMainMenu();
                break;

            case "LoadSaveBtn":
                MusicMgr.Instance?.PlaySound("按钮点击音效");
                GoToLoadSave();
                break;
        }
    }

    private void ContinueGame()
    {
        UIMgr.Instance.HideAllPanels();
        var coordinator = LevelFlowCoordinator.Instance;
        if (coordinator != null && !string.IsNullOrEmpty(coordinator.NextLevelSceneName))
            SceneMgr.Instance.LoadScene(coordinator.NextLevelSceneName);
        else
            SceneMgr.Instance.LoadScene("GameScene");
    }

    private void BackToMenu()
    {
        UIMgr.Instance.HideAllPanels();
        SceneMgr.Instance.LoadScene("GameScene");
    }

    private void ReturnToMainMenu()
    {
        if (EndingManager.Instance != null)
            EndingManager.Instance.ReturnToMainMenu();
        else
        {
            UIMgr.Instance.HidePanel<EndingContentPanel>(true);
            EventCenter.Instance.EventTrigger(E_EventType.ShowMainMenu);
        }
    }

    private void GoToLoadSave()
    {
        if (EndingManager.Instance != null)
            EndingManager.Instance.GoToLoadSave();
        else
        {
            UIMgr.Instance.HidePanel<EndingContentPanel>(true);
            EventCenter.Instance.EventTrigger(E_EventType.ShowMainMenu);
            UIMgr.Instance.ShowPanel<SaveGamePanel>();
        }
    }
}
