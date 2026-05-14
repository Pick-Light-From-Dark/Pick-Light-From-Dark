using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 快进开发模式：按住指定按键时快进剧情，松开即停。
    /// 挂载到场景即可生效，完全独立不影响其他系统。
    /// </summary>
    public class FastForwardDevMode : DevModeBase
    {
        [Header("快进按键")]
        [Tooltip("按住此键快进，松开停止")]
        public KeyCode fastForwardKey = KeyCode.Space;

        [Header("目标 VN 控制器（为空则自动查找）")]
        public FungusVNController targetVN;

        private bool wasHolding = false;

        protected override void Awake()
        {
            base.Awake();
            FindTargetVN();
        }

        void FindTargetVN()
        {
            if (targetVN == null)
            {
                targetVN = FindObjectOfType<FungusVNController>();
                if (targetVN != null)
                    Debug.Log($"[FastForwardDevMode] 自动找到 VN 控制器: {targetVN.name}");
            }
        }

        protected override void OnDevUpdate()
        {
            if (targetVN == null)
            {
                FindTargetVN();
                return;
            }

            bool isHolding = Input.GetKey(fastForwardKey);

            if (isHolding && !wasHolding)
            {
                // 按下瞬间：开启快进
                targetVN.ToggleFastForward();
                Debug.Log("[FastForwardDevMode] 快进开启");
            }
            else if (!isHolding && wasHolding)
            {
                // 松开瞬间：停止快进
                targetVN.ToggleFastForward();
                Debug.Log("[FastForwardDevMode] 快进停止");
            }

            wasHolding = isHolding;
        }

        [ContextMenu("测试：切换快进")]
        void TestToggleFastForward()
        {
            if (targetVN != null)
                targetVN.ToggleFastForward();
        }
    }
}
