using UnityEngine;
using TMPro;
using UnityEngine.UI;

[UIPath("UI/Dialogue")]
public class GameDialoguePanel : BasePanel, IDialoguePanel
{
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI speakerText;

    public Image backgroundImage;

    public void SetContent(string speaker, string content)
    {
        if (speakerText != null)
            speakerText.text = speaker;

        if (contentText != null)
            contentText.text = content;
    }

    public void SetRole(Sprite sprite)
    {
        // Game模式不显示角色
    }

    public void SetBackground(Sprite sprite)
    {
        if (backgroundImage != null && sprite != null)
        {
            backgroundImage.sprite = sprite;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public override void ShowMe() { }

    public override void HideMe() { }
}
