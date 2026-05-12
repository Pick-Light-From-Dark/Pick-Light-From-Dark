using UnityEngine;
using Game.Config;

namespace Game.Flow
{
    /// <summary>
    /// 结局管理器 — 负责触发结局、显示结局面板
    /// </summary>
    public class EndingManager : SingletonAutoMono<EndingManager>
    {
        [Header("结局数据配置")]
        [SerializeField] private EndingDataSO endingData;

        [Header("结局面板路径（Resources下）")]
        [SerializeField] private string endingPanelPath = "UI/Content/EndingContentPanel";

        /// <summary>
        /// 当前触发的结局ID
        /// </summary>
        public int CurrentEndingId { get; private set; }

        /// <summary>
        /// 是否正在显示结局
        /// </summary>
        public bool IsShowingEnding { get; private set; }

        /// <summary>
        /// 初始化结局管理器
        /// </summary>
        public void Initialize(EndingDataSO data)
        {
            endingData = data;
            IsShowingEnding = false;
            CurrentEndingId = 0;
            Debug.Log("[EndingManager] 初始化完成");
        }

        /// <summary>
        /// 触发指定ID的结局
        /// </summary>
        public void TriggerEnding(int endingId)
        {
            if (endingData == null)
            {
                Debug.LogError("[EndingManager] 触发结局失败：endingData 为 null");
                return;
            }

            var entry = endingData.GetEndingById(endingId);
            if (entry == null)
            {
                Debug.LogError($"[EndingManager] 找不到结局ID: {endingId}");
                return;
            }

            CurrentEndingId = endingId;
            IsShowingEnding = true;

            Debug.Log($"[EndingManager] 触发结局 [{endingId}] {entry.endingName}");

            // 触发结局显示事件
            EventCenter.Instance.EventTrigger(E_EventType.ShowResultPanel);

            // 显示结局面板
            ShowEndingPanel(endingId);
        }

        /// <summary>
        /// 显示结局面板
        /// </summary>
        private void ShowEndingPanel(int endingId)
        {
            UIMgr.Instance.ShowPanel<EndingContentPanel>(
                E_UILayer.System,
                (panel) =>
                {
                    if (panel != null)
                    {
                        panel.SetupEnding(endingId, endingData);
                    }
                }
            );
        }

        /// <summary>
        /// 返回主界面
        /// </summary>
        public void ReturnToMainMenu()
        {
            IsShowingEnding = false;
            CurrentEndingId = 0;
            UIMgr.Instance.HidePanel<EndingContentPanel>(true);
            EventCenter.Instance.EventTrigger(E_EventType.ShowMainMenu);
            Debug.Log("[EndingManager] 返回主界面");
        }

        /// <summary>
        /// 跳转至读取存档界面
        /// </summary>
        public void GoToLoadSave()
        {
            IsShowingEnding = false;
            UIMgr.Instance.HidePanel<EndingContentPanel>(true);
            EventCenter.Instance.EventTrigger(E_EventType.ShowMainMenu);
            // 显示存档面板
            UIMgr.Instance.ShowPanel<SaveGamePanel>();
            Debug.Log("[EndingManager] 跳转至读取存档界面");
        }

        /// <summary>
        /// 获取结局数据配置
        /// </summary>
        public EndingDataSO GetEndingData()
        {
            return endingData;
        }
    }
}
