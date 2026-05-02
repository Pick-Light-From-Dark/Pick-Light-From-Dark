using System;
using System.Collections.Generic;
using UnityEngine;

public static class DialogueParse
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
                d.content = s.Trim();
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