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
            else if (s.StartsWith("[block:"))
            {
                d.type = "段落";
                d.content = s.Substring(7).TrimEnd(']').Trim();
            }
            else if (s.StartsWith("[action:"))
            {
                d.type = "指令";
                d.action = s.Substring(8).TrimEnd(']').Trim().ToLower();
            }
            else if (s.StartsWith("[hide_dialog"))
            {
                d.type = "指令";
                d.dialogAction = "hide";
            }
            else if (s.StartsWith("[show_dialog"))
            {
                d.type = "指令";
                d.dialogAction = "show";
            }
            else if (s.StartsWith("[center_text:"))
            {
                d.type = "指令";
                d.centerText = s.Substring(13).TrimEnd(']').Trim();
            }
            else if (s.StartsWith("[pan:"))
            {
                d.type = "指令";
                string panContent = s.Substring(5).TrimEnd(']');
                string[] parts = panContent.Split(',');
                if (parts.Length >= 3)
                {
                    d.panLayer = parts[0].Trim().ToLower();
                    d.panDirection = parts[1].Trim().ToLower();
                    if (float.TryParse(parts[2].Trim(), out float dur))
                        d.panDuration = dur;
                    if (parts.Length >= 4 && float.TryParse(parts[3].Trim(), out float dist))
                        d.panDistance = dist;
                    else
                        d.panDistance = -1f;
                }
            }
            else if (s.StartsWith("[旁白]："))
            {
                d.type = "旁白";
                d.speaker = "旁白";
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
                // 继承上一行对话的说话人（若上一行是对话/旁白/场景）
                string prevSpeaker = "";
                if (list.Count > 0)
                {
                    var prev = list[list.Count - 1];
                    if (prev.type == "对话" || prev.type == "旁白" || prev.type == "场景")
                        prevSpeaker = prev.speaker;
                }
                d.speaker = prevSpeaker;

                // 兼容格式：选项 -A or -B。 / 选项 - A or - B。 / 选项 -A or B。
                int start = s.IndexOf('-');
                int orIndex = s.IndexOf("or");

                if (start != -1 && orIndex != -1 && orIndex > start)
                {
                    d.choice1 = s.Substring(start + 1, orIndex - start - 1).Trim();
                    d.choice1 = d.choice1.TrimStart('-').Trim();

                    string afterOr = s.Substring(orIndex + 2).Replace("。", "").Trim();
                    d.choice2 = afterOr.TrimStart('-').Trim();
                }

                list.Add(d);
                continue;
            }
            else if (s.StartsWith("【选项"))
            {
                // 解析选项结果文本，同时提取选项标识和跳转目标
                // 兼容格式：【选项-xxx】、【选项 - xxx】、【选项-x】等
                int idStart = s.IndexOf('-');
                if (idStart == -1) idStart = s.IndexOf('一'); // 备用分隔符
                if (idStart == -1) idStart = 3; // 兜底
                int idEnd = s.IndexOf('】');
                string choiceId = (idEnd > idStart + 1) ? s.Substring(idStart + 1, idEnd - idStart - 1).Trim() : "";

                // 提取跳转目标：跳转【段落名】
                string jumpTarget = "";
                int jumpIdx = s.IndexOf("跳转【");
                if (jumpIdx != -1)
                {
                    int targetStart = jumpIdx + 3; // 跳过 "跳转【"
                    int targetEnd = s.IndexOf('】', targetStart);
                    if (targetEnd != -1)
                        jumpTarget = s.Substring(targetStart, targetEnd - targetStart).Trim();
                }

                // 提取动作：动作【xxx】
                string choiceAction = "";
                int actionIdx = s.IndexOf("动作【");
                if (actionIdx != -1)
                {
                    int actionStart = actionIdx + 3; // 跳过 "动作【"
                    int actionEnd = s.IndexOf('】', actionStart);
                    if (actionEnd != -1)
                        choiceAction = s.Substring(actionStart, actionEnd - actionStart).Trim().ToLower();
                }

                string result = ReadUntilBreak(rawLines, i + 1);
                // 结果正文已并入选项行，避免主循环再次解析相同行（否则会选「吃」后仍顺序播放「不吃」等后续文本）
                int afterResultBlock = IndexAfterChoiceResultBlock(rawLines, i + 1);
                i = afterResultBlock - 1;

                for (int j = list.Count - 1; j >= 0; j--)
                {
                    if (list[j].type == "选项")
                    {
                        // 兼容格式：按顺序绑定，第一个结果→choice1，第二个→choice2
                        // 同时也兼容带数字后缀的格式（如 吃1/吃2）
                        // 注意：仅「跳转」无内联正文时 choice1Result 为空，但 choice1Id 已占用，第二条必须落到 choice2，否则会覆盖导致选项结果反了
                        bool bindToChoice2 = choiceId.EndsWith("2");
                        if (!bindToChoice2 && !string.IsNullOrEmpty(list[j].choice1Id) && list[j].choice1Id != choiceId)
                        {
                            bindToChoice2 = true;
                        }
                        if (!bindToChoice2 && !string.IsNullOrEmpty(list[j].choice1Result) && list[j].choice1Id != choiceId)
                        {
                            bindToChoice2 = true;
                        }

                        if (bindToChoice2)
                        {
                            list[j].choice2Result = result;
                            list[j].choice2Id = choiceId;
                            list[j].choice2JumpTarget = jumpTarget;
                            list[j].choice2Action = choiceAction;
                        }
                        else
                        {
                            list[j].choice1Result = result;
                            list[j].choice1Id = choiceId;
                            list[j].choice1JumpTarget = jumpTarget;
                            list[j].choice1Action = choiceAction;
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

            // 策划备忘行（# 开头）不并入对白，且与主循环一致地结束结果块
            if (next.StartsWith("#")) break;

            if (next.StartsWith("选项") || next.StartsWith("【卡牌") || next.StartsWith("【选项") || next.StartsWith("[block:") || next.StartsWith("[action:"))
                break;

            result += next + "\n";
        }
        return result.Trim();
    }

    /// <summary>与 ReadUntilBreak 相同分界：返回第一个不并入选项结果的 raw 行下标（或数组长度）。</summary>
    private static int IndexAfterChoiceResultBlock(string[] rawLines, int startIndex)
    {
        for (int k = startIndex; k < rawLines.Length; k++)
        {
            string next = rawLines[k].Trim();
            if (string.IsNullOrEmpty(next)) continue;

            if (next.StartsWith("#")) return k;

            if (next.StartsWith("选项") || next.StartsWith("【卡牌") || next.StartsWith("【选项") || next.StartsWith("[block:") || next.StartsWith("[action:"))
                return k;
        }
        return rawLines.Length;
    }
}
