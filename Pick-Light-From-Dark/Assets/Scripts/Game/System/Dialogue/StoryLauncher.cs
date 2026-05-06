using UnityEngine;

public class StoryLauncher : MonoBehaviour
{
    public TextAsset dialogueText;  // 对话文本
    public DialogueSystem dialogueSystem;  // 对话系统引用

    void Start()
    {

    }

    // 启动对话并选择模式
    public void StartDialogue(DialogueSystem.DialogueMode mode)
    {
        dialogueSystem.StartDialogue(dialogueText, mode);  // 启动对话并指定模式
    }
}