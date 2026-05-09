using System;
using System.Collections.Generic;
using UnityEngine;

public static class DialogueParser
{
    public static List<DialogueLine> Parse(TextAsset txt)
    {
        var list = new List<DialogueLine>();
        if (txt == null) return list;

        string[] rawLines = txt.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < rawLines.Length; i++)
        {
            string s = rawLines[i].Trim();
            if (string.IsNullOrEmpty(s)) continue;

            var d = new DialogueLine();

            if (s.StartsWith("[bg:"))
            {
                d.type = "指令";
                d.bg = s.Substring(4).TrimEnd(']');
            }
            else if (s.StartsWith("[se:"))
            {
                d.type = "指令";
                d.se = s.Substring(4).TrimEnd(']');
            }
            else if (s.StartsWith("[bgm:"))
            {
                d.type = "指令";
                d.bgm = s.Substring(5).TrimEnd(']');
            }
            else if (s.StartsWith("[旁白]："))
            {
                d.type = "旁白";
                d.speaker = "";
                d.content = s.Substring(5).Trim();
            }
            else if (s.StartsWith("[场景]："))
            {
                d.type = "场景";
                d.speaker = "";
                d.content = s.Substring(5).Trim();
            }
            else if (s.StartsWith("[场景/旁白]："))
            {
                d.type = "场景";
                d.speaker = "";
                d.content = s.Substring(8).Trim();
            }
            else if (s.StartsWith("[文本场景/画面]："))
            {
                d.type = "场景";
                d.speaker = "";
                d.content = s.Substring(9).Trim();
            }
            else if (s.StartsWith("选项"))
            {
                d.type = "选项";
                d.speaker = "";

                int start = s.IndexOf("-");
                int orIndex = s.IndexOf("or");

                if (start != -1 && orIndex != -1)
                {
                    d.choice1 = s.Substring(start + 1, orIndex - start - 1).Trim();
                    d.choice2 = s.Substring(orIndex + 2).Replace("。", "").Trim();
                }

                list.Add(d);
                continue;
            }
            else if (s.StartsWith("【选项-吃1】"))
            {
                // 解析选项1的结果文本
                string result = ReadUntilBreak(rawLines, i + 1);
                // 找到最近的"选项"
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    if (list[j].type == "选项")
                    {
                        list[j].choice1Result = result;
                        break;
                    }
                }
                continue;
            }
            else if (s.StartsWith("【选项-吃2】"))
            {
                // 解析选项2的结果文本
                string result = ReadUntilBreak(rawLines, i + 1);
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    if (list[j].type == "选项")
                    {
                        list[j].choice2Result = result;
                        break;
                    }
                }
                continue;
            }
            else if (s.StartsWith("【选项-吃】"))
            {
                // 兼容旧格式，默认当作 choice1Result
                string result = ReadUntilBreak(rawLines, i + 1);
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    if (list[j].type == "选项")
                    {
                        list[j].choice1Result = result;
                        break;
                    }
                }
                continue;
            }
            else if (s.Contains("："))
            {
                int idx = s.IndexOf("：");
                d.type = "对话";
                d.speaker = s.Substring(0, idx).Trim();
                d.content = s.Substring(idx + 1).Trim();
            }
            else
            {
                continue;
            }

            list.Add(d);
        }

        return list;
    }

    /// <summary>
    /// 从 startIndex 开始读取文本，直到遇到新的标记行（选项/【卡牌等）
    /// </summary>
    private static string ReadUntilBreak(string[] rawLines, int startIndex)
    {
        string result = "";
        for (int k = startIndex; k < rawLines.Length; k++)
        {
            string next = rawLines[k].Trim();
            if (string.IsNullOrEmpty(next)) continue;

            if (next.StartsWith("选项") || next.StartsWith("【卡牌") || next.StartsWith("【选项"))
                break;

            result += next + "\n";
        }
        return result.Trim();
    }
}
