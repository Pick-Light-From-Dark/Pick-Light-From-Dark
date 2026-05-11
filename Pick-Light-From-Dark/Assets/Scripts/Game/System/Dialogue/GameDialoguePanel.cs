using UnityEngine;
using TMPro;
using UnityEngine.UI;

[UIPath("UI/Dialogue")]
public class GameDialoguePanel : BasePanel, IDialoguePanel
{
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI speakerText;

    public Image backgroundImage;
    public Image midgroundImage;
    public Image foregroundImage;

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

    public void SetBackground(Sprite sprite, string transition = null)
    {
        if (backgroundImage != null && sprite != null)
        {
            backgroundImage.sprite = sprite;
        }
    }

    public void SetLayerBackground(string layer, Sprite sprite, string transition = null)
    {
        Image target = layer switch
        {
            "bg" => backgroundImage,
            "mg" => midgroundImage,
            "fg" => foregroundImage,
            _ => null
        };

        if (target != null)
        {
            if (sprite == null)
            {
                target.sprite = null;
                target.color = Color.clear;
            }
            else
            {
                target.sprite = sprite;
                target.color = Color.white;
            }
        }
    }

    public void SetSolidBackground(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = null;
            backgroundImage.color = color;
        }
        if (midgroundImage != null) { midgroundImage.sprite = null; midgroundImage.color = Color.clear; }
        if (foregroundImage != null) { foregroundImage.sprite = null; foregroundImage.color = Color.clear; }
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
