using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// 任务系统调试器 — 按键触发事件，验证 TaskManager/LevelRecordManager/PlayerDataStore
    /// </summary>
    public class TaskSystemDebugger : MonoBehaviour
    {
        [Header("调试配置")]
        [Tooltip("当前选中的卡牌 ID")]
        public int currentCardID = 1001;

        void Update()
        {
            // Space: 触发 CardReadComplete 事件
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EventCenter.Instance.EventTrigger(E_EventType.CardReadComplete, currentCardID);
                Debug.Log($"[TaskSystemDebugger] 触发 CardReadComplete, cardID={currentCardID}");
            }

            // S: 触发 GameWin 事件，验证 LevelRecordManager 存盘
            if (Input.GetKeyDown(KeyCode.S))
            {
                EventCenter.Instance.EventTrigger(E_EventType.GameWin);
                Debug.Log("[TaskSystemDebugger] 触发 GameWin, 验证 LevelRecordManager 存盘");
            }

            // 1: 切换 currentCardID 为 1001
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentCardID = 1001;
                Debug.Log($"[TaskSystemDebugger] 切换 currentCardID → {currentCardID}");
            }

            // 2: 切换 currentCardID 为 1002
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentCardID = 1002;
                Debug.Log($"[TaskSystemDebugger] 切换 currentCardID → {currentCardID}");
            }
        }
    }
}