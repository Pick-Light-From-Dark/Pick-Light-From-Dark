using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : BasePanel
{
    public static DialogueSystem Instance;

    [Header("配置")]
    public RoleConfig roleConfig;
    public BackgroundConfig backgroundConfig;
    [Header("核心UI")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI contentText;
    public Image backgroundImage;
    public Image roleImage;
    public Image speakerNameBg;

    [Header("选项")]
    public Button btnYes;
    public Button btnNo;

    private List<DialogueLine> lines;
    private int index = 0;
    private bool isChoosing = false;


    private DialogueLine currentChoiceLine;
    void Awake()
    {
        Instance = this;
        if (roleImage != null) roleImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isChoosing)
        {
            NextLine();
        }
    }

    
    public void StartDialogue(string txtFileName)
    {
        gameObject.SetActive(true);

        // 强制显示背景，不会被隐藏
        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(true);

        // 加载文本
        TextAsset txt = Resources.Load<TextAsset>(txtFileName);

        // 加载失败 → 显示错误，不隐藏面板
        if (txt == null)
        {
            contentText.text = $" 找不到文件：{txtFileName}";
            Debug.LogError($"对话文件加载失败：{txtFileName}");
            return;
        }

        // 解析
        lines = DialogueParser.Parse(txt);
        if (lines == null || lines.Count == 0)
        {
            contentText.text = " 文本解析为空";
            return;
        }

        // 正常开始
        index = 0;
        isChoosing = false;
        btnYes.gameObject.SetActive(false);
        btnNo.gameObject.SetActive(false);
        NextLine();
    }

    void NextLine()
    {
        if (lines == null || index >= lines.Count)
        {
            contentText.text = " 对话结束";
            return;
        }

        DialogueLine line = lines[index];
        index++;

        if (line.type == "选项")
        {
            currentChoiceLine = line;
            ShowChoice(line);
            return;
        }

        ShowLine(line);
        AutoSetRole(line);
        AutoSetBackground(line);
    }

    void ShowLine(DialogueLine line)
    {
        switch (line.type)
        {
            case "旁白":
                speakerText.text = "";
                contentText.text = line.content;

                if (speakerNameBg != null)
                    speakerNameBg.gameObject.SetActive(false);
                break;

            case "场景":
                speakerText.text = "";

                string content = line.content;
                if (content.StartsWith("场景："))
                {
                    content = content.Substring("场景：".Length);
                }

                contentText.text = content;

                if (speakerNameBg != null)
                    speakerNameBg.gameObject.SetActive(false);
                break;

            case "对话":
                speakerText.text = line.speaker;
                contentText.text = line.content;

                if (speakerNameBg != null)
                    speakerNameBg.gameObject.SetActive(true);
                break;
        }
    }

    void AutoSetRole(DialogueLine line)
    {
        if (line.type == "对话")
        {
            Sprite sprite = roleConfig.GetRoleSprite(line.speaker);
            if (sprite != null)
            {
                roleImage.sprite = sprite;
                roleImage.gameObject.SetActive(true);
            }
        }
        else
        {
            roleImage.gameObject.SetActive(false);
        }
    }

    void AutoSetBackground(DialogueLine line)
    {
        if (line.type == "场景")
        {
            Sprite bg = backgroundConfig.GetBg(line.content);
            if (bg != null)
                backgroundImage.sprite = bg;
        }
    }

    void ShowChoice(DialogueLine line)
    {
        isChoosing = true;
        currentChoiceLine = line;

        btnYes.GetComponentInChildren<TextMeshProUGUI>().text = line.choice1;
        btnNo.GetComponentInChildren<TextMeshProUGUI>().text = line.choice2;

        btnYes.gameObject.SetActive(true);
        btnNo.gameObject.SetActive(true);

        contentText.text = "请选择：";
    }

    public void OnChooseYes()
    {
        isChoosing = false;

        btnYes.gameObject.SetActive(false);
        btnNo.gameObject.SetActive(false);

        
        if (currentChoiceLine != null)
        {
            speakerText.text = "";
            contentText.text = currentChoiceLine.choice1Result;
        }
    }

    public void OnChooseNo()
    {
        isChoosing = false;

        btnYes.gameObject.SetActive(false);
        btnNo.gameObject.SetActive(false);

        
        UIMgr.Instance.HidePanel<DialogueSystem>();
    }

    void PlayEnding()
    {
        speakerText.text = "【结局】";
        contentText.text = "你拒绝了零食，游戏结束。";
        Invoke(nameof(BackToMain), 2f);
    }

    void BackToMain()
    {
        UIMgr.Instance.HidePanel<DialogueSystem>();
        UIMgr.Instance.ShowPanel<BeginPanel>();
    }

    public override void ShowMe() { }
    public override void HideMe() { }

    protected override void ClickBtn(string btnName) { }
}