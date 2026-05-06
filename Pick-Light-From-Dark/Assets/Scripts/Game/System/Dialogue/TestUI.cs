using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    public Button startButton;  // 启动对话的按钮
    public StoryLauncher storyLauncher;  // StoryLauncher 用来启动对话

    void Start()
    {
        // 按钮点击事件绑定
        startButton.onClick.AddListener(StartGameDialogue);  // 点击按钮时启动 Game 对话
    }

    // 启动对话并显示对话框
    public void StartGameDialogue()
    {
        // 直接启动对话，选择 Game 模式
        storyLauncher.StartDialogue(DialogueSystem.DialogueMode.Game);
    }

}
