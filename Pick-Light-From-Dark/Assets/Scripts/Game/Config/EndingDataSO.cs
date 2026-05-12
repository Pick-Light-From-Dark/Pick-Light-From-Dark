using System.Collections.Generic;
using UnityEngine;

namespace Game.Config
{
    /// <summary>
    /// 单条结局数据
    /// </summary>
    [System.Serializable]
    public class EndingEntry
    {
        [Tooltip("结局ID")]
        public int id;

        [Tooltip("结局名称")]
        public string endingName;

        [Tooltip("结局描述")]
        [TextArea(3, 5)]
        public string description;

        [Tooltip("结局展示图片（可选）")]
        public Sprite endingSprite;
    }

    /// <summary>
    /// 结局数据配置表 — ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "EndingData", menuName = "Game/Ending Data")]
    public class EndingDataSO : ScriptableObject
    {
        [Header("结局列表")]
        public List<EndingEntry> endings = new List<EndingEntry>();

        /// <summary>
        /// 根据ID获取结局数据
        /// </summary>
        public EndingEntry GetEndingById(int id)
        {
            if (endings == null) return null;
            foreach (var entry in endings)
            {
                if (entry != null && entry.id == id)
                    return entry;
            }
            return null;
        }

        /// <summary>
        /// 获取结局序号文本（如"结局一"）
        /// </summary>
        public string GetEndingCountText(int id)
        {
            if (endings == null) return "";
            for (int i = 0; i < endings.Count; i++)
            {
                if (endings[i] != null && endings[i].id == id)
                {
                    string[] cnNums = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
                    if (i < cnNums.Length)
                        return $"结局{cnNums[i]}";
                    return $"结局{i + 1}";
                }
            }
            return "";
        }
    }
}
