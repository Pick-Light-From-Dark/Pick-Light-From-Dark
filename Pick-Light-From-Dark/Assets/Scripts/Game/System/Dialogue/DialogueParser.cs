using System;
using System.Collections.Generic;
using UnityEngine;

public static class DialogueParser
{
    public static List<DialogueLine> Parse(TextAsset txt)
    {
        var list = new List<DialogueLine>();
        if (txt == null) return list;

        string[] lines = txt.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string s = line.Trim();
            if (string.IsNullOrEmpty(s)) continue;

            var d = new DialogueLine();

            if (s.StartsWith("[旁白]："))
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
            else if (s.StartsWith("【选项-吃】"))
            {
                // 找到最近的“选项”
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].type == "选项")
                    {
                        // 把后面的所有文本拼接成结果
                        string result = "";

                        for (int j = Array.IndexOf(lines, line) + 1; j < lines.Length; j++)
                        {
                            string next = lines[j].Trim();
                            if (string.IsNullOrEmpty(next)) continue;

                            // 遇到新的结构就停止
                            if (next.StartsWith("选项") || next.StartsWith("【卡牌"))
                                break;

                            result += next + "\n";
                        }

                        list[i].choice1Result = result.Trim();
                        break;
                    }
                }
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
}