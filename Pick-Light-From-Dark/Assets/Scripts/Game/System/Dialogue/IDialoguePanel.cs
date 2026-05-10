using UnityEngine;

public interface IDialoguePanel
{
    void SetContent(string speaker, string content);
    void SetRole(Sprite sprite);
    void SetBackground(Sprite sprite, string transition = null);

    void Show();
    void Hide();
}
