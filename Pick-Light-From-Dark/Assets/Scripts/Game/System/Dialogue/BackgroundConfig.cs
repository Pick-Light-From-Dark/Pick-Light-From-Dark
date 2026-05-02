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

    public Sprite GetBg(string name)
    {
        if (bgDict == null) Init();

        return bgDict.ContainsKey(name) ? bgDict[name] : null;
    }
}