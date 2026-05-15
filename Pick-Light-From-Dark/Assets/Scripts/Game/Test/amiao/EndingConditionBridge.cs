using UnityEngine;
using System.Collections.Generic;

namespace Game.Test
{
    /// <summary>
    /// 结局条件桥接器 — 供 Fungus 剧情调用的后端接口
    ///
    /// Fungus 使用方式：
    /// 1. 在场景中创建一个 GameObject，挂载本组件
    /// 2. 使用 'Call Method' 或 'Invoke Method' 命令调用以下 public 方法
    /// 3. 查询方法（返回 bool）可通过 Invoke Method 的 Save Return Value 存入 Fungus Variable
    /// </summary>
    public class EndingConditionBridge : MonoBehaviour
    {
        [Header("调试")]
        [Tooltip("是否在控制台输出操作日志")]
        public bool logOperations = true;

        // 内存 fallback：当 CrossLevelSaveSystem 不可用时使用
        private HashSet<int> fallbackCardsUsed = new HashSet<int>();

        // ========== 记录接口（Fungus Call Method / Invoke Method 无参调用）==========

        /// <summary>记录使用了分享泡面卡 (2017)</summary>
        public void RecordCard2017()
        {
            RecordCard(2017);
        }

        /// <summary>记录使用了寻求宋明月帮助 (2026)</summary>
        public void RecordCard2026()
        {
            RecordCard(2026);
        }

        /// <summary>通用记录接口（Fungus Invoke Method 可传入 int 参数）</summary>
        public void RecordCard(int cardId)
        {
            if (CrossLevelSaveSystem.Instance != null)
            {
                CrossLevelSaveSystem.Instance.RecordCardUsed(cardId);
            }
            else
            {
                fallbackCardsUsed.Add(cardId);
            }

            Log($"[EndingConditionBridge] 记录卡牌使用: {cardId}");
        }

        // ========== 查询接口（Fungus Invoke Method 可捕获返回值）==========

        /// <summary>是否使用了分享泡面卡 (2017)</summary>
        public bool HasUsedCard2017()
        {
            return HasUsedCard(2017);
        }

        /// <summary>是否使用了寻求宋明月帮助 (2026)</summary>
        public bool HasUsedCard2026()
        {
            return HasUsedCard(2026);
        }

        /// <summary>
        /// 是否可以显示天台选项 — 两卡全部使用时返回 true
        /// Fungus 可将返回值存入 Boolean Variable，绑定到 Menu 的 hideThisOption
        /// </summary>
        public bool CanShowRooftopChoice()
        {
            bool result = HasUsedCard(2017) && HasUsedCard(2026);
            Log($"[EndingConditionBridge] 天台选项检查: {result} (2017={HasUsedCard(2017)}, 2026={HasUsedCard(2026)})");
            return result;
        }

        /// <summary>通用查询接口</summary>
        public bool HasUsedCard(int cardId)
        {
            if (CrossLevelSaveSystem.Instance != null)
            {
                var data = CrossLevelSaveSystem.Instance.LoadEndingData();
                return data != null && data.cardsUsed.Contains(cardId);
            }

            return fallbackCardsUsed.Contains(cardId);
        }

        // ========== 管理接口 ==========

        /// <summary>重置卡牌使用记录（测试用）</summary>
        public void ResetCardUsage()
        {
            if (CrossLevelSaveSystem.Instance != null)
            {
                CrossLevelSaveSystem.Instance.ClearAll();
            }

            fallbackCardsUsed.Clear();
            Log("[EndingConditionBridge] 卡牌使用记录已重置");
        }

        // ========== 内部 ==========

        void Log(string msg)
        {
            if (logOperations)
                Debug.Log(msg);
        }
    }
}
