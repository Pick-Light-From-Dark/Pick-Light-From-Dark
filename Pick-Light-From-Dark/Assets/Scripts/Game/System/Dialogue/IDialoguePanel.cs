using UnityEngine;

public interface IDialoguePanel
{
    void SetContent(string speaker, string content);
    void SetRole(Sprite sprite);
    void SetBackground(Sprite sprite, string transition = null);
    void SetLayerBackground(string layer, Sprite sprite, string transition = null);
    void SetSolidBackground(Color color);
    void PanLayer(string layer, string direction, float duration, float distance);

    void Show();
    void Hide();
}
