using UnityEngine;
using Game.Config;
using Game.Data;

namespace Game.Data
{
    /// <summary>
    /// 玩家状态
    /// 管理玩家的游戏内状态（卧床、闭眼等）
    /// </summary>
    public class PlayerState : SingletonAutoMono<PlayerState>
    {
        [Header("玩家状态")]
        [SerializeField] private bool isInBed;
        [SerializeField] private bool isEyesClosed;

        /// <summary>
        /// 关卡初始化：根据配置重置玩家状态，避免单例跨关残留
        /// </summary>
        public void Initialize(LevelConfigSO config)
        {
            // 直接赋值（不走 SetInBed/SetEyesClosed），避免初始化阶段误触发事件
            isInBed = config != null ? config.initialInBed : true;
            isEyesClosed = false;
            Debug.Log($"[PlayerState] 初始化 - 床上:{isInBed} 闭眼:{isEyesClosed}");
        }

        /// <summary>
        /// 玩家是否在床上
        /// </summary>
        public bool IsInBed()
        {
            return isInBed;
        }

        /// <summary>
        /// 设置是否在床上
        /// </summary>
        public void SetInBed(bool inBed)
        {
            if (isInBed != inBed)
            {
                isInBed = inBed;
                Debug.Log($"[PlayerState] 玩家{(inBed ? "上床" : "下床")}");
            }
        }

        /// <summary>
        /// 玩家是否闭眼
        /// </summary>
        public bool IsEyesClosed()
        {
            return isEyesClosed;
        }

        /// <summary>
        /// 设置是否闭眼
        /// </summary>
        public void SetEyesClosed(bool closed)
        {
            if (isEyesClosed != closed)
            {
                isEyesClosed = closed;
                Debug.Log($"[PlayerState] 玩家{(closed ? "闭眼" : "睁眼")}");

                // 触发闭眼状态变化事件
                EventCenter.Instance.EventTrigger(E_EventType.PlayerEyeCloseChanged, closed);

                if (closed)
                {
                    EventCenter.Instance.EventTrigger(E_EventType.EyeCloseStart);
                }
                else
                {
                    EventCenter.Instance.EventTrigger(E_EventType.EyeCloseEnd);
                }
            }
        }

        /// <summary>
        /// 切换闭眼状态
        /// </summary>
        public void ToggleEyesClosed()
        {
            SetEyesClosed(!isEyesClosed);
        }

        void Update()
        {
            // C键切换闭眼
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleEyesClosed();
            }
        }
    }
}
