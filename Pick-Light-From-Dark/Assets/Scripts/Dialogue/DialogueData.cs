using System;
using System.Collections.Generic;
using UnityEngine;

// 对话数据结构
public class DialogueData
{
    public string type; // "旁白"/"场景"/"对话"
    public string speaker; // 说话人（如"陆萤""宋明月"）
    public string content; // 对话内容
}

public class DialogueParser
{
    // 解析TXT文件，返回对话列表
    public static List<DialogueData> ParseDialogue(string fileName)
    {
        List<DialogueData> dialogueList = new List<DialogueData>();
        TextAsset txt = Resources.Load<TextAsset>("Dialogue/" + fileName);

        if (txt == null)
        {
            Debug.LogError("找不到对话文件：" + fileName);
            return dialogueList;
        }

        string[] lines = txt.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            DialogueData data = new DialogueData();

            // 识别 [旁白] 格式
            if (line.StartsWith("[旁白]"))
            {
                data.type = "旁白";
                data.speaker = "";
                data.content = line.Substring(4).Trim();
            }
            // 识别 [场景] 格式
            else if (line.StartsWith("[场景]"))
            {
                data.type = "场景";
                data.speaker = "";
                data.content = line.Substring(4).Trim();
            }
            // 识别角色对话（如 "陆萤：xxx"）
            else if (line.Contains("："))
            {
                int colonIndex = line.IndexOf("：");
                data.type = "对话";
                data.speaker = line.Substring(0, colonIndex).Trim();
                data.content = line.Substring(colonIndex + 1).Trim();
            }
            else
            {
                // 忽略空行或无法识别的行
                continue;
            }

            dialogueList.Add(data);
        }

        return dialogueList;
    }
}