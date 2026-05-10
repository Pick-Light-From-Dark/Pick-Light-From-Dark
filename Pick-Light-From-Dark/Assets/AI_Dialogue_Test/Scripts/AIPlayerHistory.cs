using UnityEngine;

namespace Game.Test.AI
{
    /// <summary>
    /// 玩家选项历史记录管理。
    /// 使用 PlayerPrefs 存储玩家在游戏中的选择历史，供 AI 生成时参考。
    /// </summary>
    public class AIPlayerHistory : MonoBehaviour
    {
        [Header("存储键名")]
        public string historyKey = "AI_PlayerChoiceHistory";
        public string choiceCountKey = "AI_PlayerChoiceCount";

        [Header("最大记录条数")]
        [Range(5, 50)]
        public int maxRecords = 20;

        /// <summary>记录一次玩家选择</summary>
        public void RecordChoice(string choiceText)
        {
            if (string.IsNullOrEmpty(choiceText)) return;

            var history = PlayerPrefs.GetString(historyKey, "");
            var count = PlayerPrefs.GetInt(choiceCountKey, 0);

            // 限制记录数量，超出时移除最旧的
            var entries = history.Split(new[] { " → " }, System.StringSplitOptions.None);
            if (count >= maxRecords && !string.IsNullOrEmpty(history))
            {
                // 找到第一个 "→" 之后的内容
                var firstArrow = history.IndexOf(" → ");
                if (firstArrow >= 0)
                {
                    history = history.Substring(firstArrow + 3);
                }
                else
                {
                    history = "";
                }
            }

            if (!string.IsNullOrEmpty(history))
                history += " → ";
            history += choiceText;

            PlayerPrefs.SetString(historyKey, history);
            PlayerPrefs.SetInt(choiceCountKey, Mathf.Min(count + 1, maxRecords));
            PlayerPrefs.Save();

            UnityEngine.Debug.Log($"[AIPlayerHistory] 记录选择: {choiceText}");
        }

        /// <summary>获取格式化的玩家历史</summary>
        public string GetFormattedHistory()
        {
            var history = PlayerPrefs.GetString(historyKey, "");
            return string.IsNullOrEmpty(history)
                ? "玩家还没有做出任何选择。"
                : history;
        }

        /// <summary>获取历史记录数量</summary>
        public int GetChoiceCount()
        {
            return PlayerPrefs.GetInt(choiceCountKey, 0);
        }

        /// <summary>清空历史记录</summary>
        public void ClearHistory()
        {
            PlayerPrefs.DeleteKey(historyKey);
            PlayerPrefs.DeleteKey(choiceCountKey);
            PlayerPrefs.Save();
            UnityEngine.Debug.Log("[AIPlayerHistory] 历史记录已清空");
        }

        /// <summary>获取最近 N 条记录</summary>
        public string GetRecentHistory(int count)
        {
            var history = PlayerPrefs.GetString(historyKey, "");
            if (string.IsNullOrEmpty(history)) return "无";

            var entries = history.Split(new[] { " → " }, System.StringSplitOptions.None);
            if (entries.Length <= count) return history;

            var sb = new System.Text.StringBuilder();
            for (int i = entries.Length - count; i < entries.Length; i++)
            {
                if (sb.Length > 0) sb.Append(" → ");
                sb.Append(entries[i]);
            }
            return sb.ToString();
        }
    }
}
