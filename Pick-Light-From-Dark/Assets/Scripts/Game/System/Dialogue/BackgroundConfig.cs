using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BackgroundConfig", menuName = "Dialogue/BackgroundConfig")]
public class BackgroundConfig : ScriptableObject
{
    [System.Serializable]
    public class BgData
    {
        public string name;
        public Sprite sprite;
    }

    public List<BgData> backgrounds;

    private Dictionary<string, Sprite> bgDict;

    public void Init()
    {
        bgDict = new Dictionary<string, Sprite>();
        foreach (var b in backgrounds)
        {
            if (!bgDict.ContainsKey(b.name))
                bgDict.Add(b.name, b.sprite);
        }
    }

    public Sprite GetBg(string bgName)
    {
        if (bgDict.ContainsKey(bgName))
        {
            return bgDict[bgName];  // 返回找到的背景图
        }
        else
        {
            Debug.LogWarning($"背景图片未找到：{bgName}");  // 输出未找到的背景名称
            return null;  // 如果找不到，返回 null
        }
    }
}