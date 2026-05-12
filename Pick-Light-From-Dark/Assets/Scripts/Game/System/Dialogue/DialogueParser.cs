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
            if (s.StartsWith("#")) continue;

            var d = new DialogueLine();

            if (s.StartsWith("[bg:"))
            {
                d.type = "指令";
                string bgContent = s.Substring(4).TrimEnd(']');
                string[] parts = bgContent.Split(',');
                d.bg = parts[0].Trim();
                if (parts.Length > 1)
                    ParseTransition(bgContent.Substring(bgContent.IndexOf(',') + 1), d);
            }
            else if (s.StartsWith("[se:"))
            {
                d.type = "指令";
                d.se = s.Substring(4).TrimEnd(']').Trim();
            }
            else if (s.StartsWith("[bgm:"))
            {
                d.type = "指令";
                d.bgm = s.Substring(5).TrimEnd(']').Trim();
            }
            else if (s.StartsWith("[fg:"))
            {
                d.type = "指令";
                string fgContent = s.Substring(4).TrimEnd(']');
                string[] parts = fgContent.Split(',');
                d.fg = parts[0].Trim();
                if (parts.Length > 1)
                    ParseTransition(fgContent.Substring(fgContent.IndexOf(',') + 1), d);
            }
            else if (s.StartsWith("[mg:"))
            {
                d.type = "指令";
                string mgContent = s.Substring(4).TrimEnd(']');
                string[] parts = mgContent.Split(',');
                d.mg = parts[0].Trim();
                if (parts.Length > 1)
                    ParseTransition(mgContent.Substring(mgContent.IndexOf(',') + 1), d);
            }
            else if (s.StartsWith("[layer:"))
            {
                d.type = "指令";
                d.isLayerCommand = true;
                string layerContent = s.Substring(7).TrimEnd(']');
                if (!string.IsNullOrEmpty(layerContent))
                {
                    string[] parts = layerContent.Split(',');
                    foreach (string part in parts)
                    {
                        string p = part.Trim();
                        if (string.IsNullOrEmpty(p)) continue;

                        if (p.StartsWith("bg="))
                        {
                            d.bg = p.Substring(3).Trim();
                        }
                        else if (p.StartsWith("mg="))
                        {
                            d.mg = p.Substring(3).Trim();
                        }
                        else if (p.StartsWith("fg="))
                        {
                            d.fg = p.Substring(3).Trim();
                        }
                        else
                        {
                            // 最后一个非键值对参数作为转场类型
                            ParseTransition(p, d);
                        }
                    }
                }
            }
            else if (s.StartsWith("[solid:"))
            {
                d.type = "指令";
                string solidContent = s.Substring(7).TrimEnd(']');
                string[] parts = solidContent.Split(',');
                d.solid = parts[0].Trim();
                if (parts.Length > 1)
                    ParseTransition(solidContent.Substring(solidContent.IndexOf(',') + 1), d);
            }
            else if (s.StartsWith("[wait:"))
            {
                d.type = "指令";
                if (float.TryParse(s.Substring(6).TrimEnd(']').Trim(), out float waitSec))
                    d.wait = waitSec;
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
            else if (s.StartsWith("【选项-"))
            {
                // 解析选项结果文本，同时提取选项标识
                // 格式：【选项-xxx】，xxx 为选项标识（如 吃1、吃2）
                int idStart = 4; // 跳过 "【选项-"
                int idEnd = s.IndexOf('】');
                string choiceId = idEnd > idStart ? s.Substring(idStart, idEnd - idStart).Trim() : "";

                string result = ReadUntilBreak(rawLines, i + 1);
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    if (list[j].type == "选项")
                    {
                        // 根据选项标识末尾数字分配 choice1/choice2
                        if (choiceId.EndsWith("2"))
                        {
                            list[j].choice2Result = result;
                            list[j].choice2Id = choiceId;
                        }
                        else
                        {
                            list[j].choice1Result = result;
                            list[j].choice1Id = choiceId;
                        }
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
    /// 解析转场指令，支持 fade,slide,crossfade 以及带停留时间的 fade,1.5
    /// </summary>
    private static void ParseTransition(string rawTransition, DialogueLine line)
    {
        if (string.IsNullOrEmpty(rawTransition)) return;
        string[] parts = rawTransition.Split(',');
        line.transition = parts[0].Trim().ToLower();
        if (parts.Length > 1 && float.TryParse(parts[1].Trim(), out float hold))
            line.transitionHold = hold;
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
