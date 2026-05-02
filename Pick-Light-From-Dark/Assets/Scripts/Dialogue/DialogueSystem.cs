using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : BasePanel
{
    [Header("核心UI")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI contentText;
    public Image backgroundImage; // 背景
    public Image roleImage;       // 【共用一个角色图】

    [Header("选项按钮")]
    public Button btnYes;
    public Button btnNo;

    private List<DialogueLine> allLines;
    private int index = 0;
    private bool isChoiceTime = false;

    public override void ShowMe()
    {
        roleImage.gameObject.SetActive(false); // 一开始隐藏立绘
        StartDialogue("Dialogue1");
    }

    public override void HideMe()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isChoiceTime)
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(string fileName)
    {
        TextAsset txt = Resources.Load<TextAsset>(fileName);
        allLines = DialogueParse.Parse(txt);

        index = 0;
        isChoiceTime = false;
        btnYes.gameObject.SetActive(false);
        btnNo.gameObject.SetActive(false);

        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (index >= allLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = allLines[index];
        index++;

        if (line.type == "选项")
        {
            ShowChoice();
            return;
        }

        ShowNormalLine(line);
        AutoSetRoleImage(line); // 自动换角色图
        AutoSetBackground(line); // 自动换背景
    }

    void ShowNormalLine(DialogueLine line)
    {
        switch (line.type)
        {
            case "旁白":
                speakerText.text = "";
                contentText.text = line.content;
                break;
            case "场景":
                speakerText.text = "";
                contentText.text = $"【场景】{line.content}";
                break;
            case "对话":
                speakerText.text = line.speaker;
                contentText.text = line.content;
                break;
        }
    }

    // ====================== 【核心：共用一个Image，自动换图】 ======================
    void AutoSetRoleImage(DialogueLine line)
    {
        if (line.type == "对话")
        {
            // 从 Resources/Role/ 加载对应名字的图
            Sprite sprite = Resources.Load<Sprite>("Role/" + line.speaker);

            if (sprite != null)
            {
                roleImage.sprite = sprite;
                roleImage.gameObject.SetActive(true);
            }
        }
        else
        {
            // 旁白、场景 → 隐藏立绘
            roleImage.gameObject.SetActive(false);
        }
    }

    // ====================== 自动换背景 ======================
    void AutoSetBackground(DialogueLine line)
    {
        if (line.type == "场景")
        {
            Sprite bg = Resources.Load<Sprite>("Backgrounds/" + line.content);
            if (bg != null)
                backgroundImage.sprite = bg;
        }
    }

    // ====================== 选项 ======================
    void ShowChoice()
    {
        isChoiceTime = true;
        contentText.text = "请选择：吃 / 不吃";
        btnYes.gameObject.SetActive(true);
        btnNo.gameObject.SetActive(true);
    }

    public void OnChooseYes()
    {
        HideChoice();
        UIMgr.Instance.HidePanel<DialogueSystem>();
        UIMgr.Instance.ShowPanel<GamePanel>();
    }

    public void OnChooseNo()
    {
        HideChoice();
        PlayEnding();
    }

    void HideChoice()
    {
        isChoiceTime = false;
        btnYes.gameObject.SetActive(false);
        btnNo.gameObject.SetActive(false);
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

    void EndDialogue()
    {
        HideMe();
    }

    protected override void ClickBtn(string btnName) { }
}