using UnityEngine;

namespace Game.Test
{
    /// <summary>
    /// 开发者功能基类 — 所有开发中辅助功能继承此类。
    /// 完全独立，不影响游戏正式逻辑，可随时一键移除。
    /// </summary>
    public abstract class DevModeBase : MonoBehaviour
    {
        [Header("开发者模式开关")]
        [Tooltip("是否启用此功能")]
        public bool enabled = true;

        protected virtual void Awake()
        {
            // 开发模式标记，方便运行时识别
            gameObject.name = $"[DEV] {GetType().Name}";
        }

        protected virtual void Update()
        {
            if (!enabled) return;
            OnDevUpdate();
        }

        /// <summary>
        /// 子类实现具体的开发功能逻辑
        /// </summary>
        protected abstract void OnDevUpdate();

        /// <summary>
        /// 一键禁用所有开发模式功能
        /// </summary>
        public static void DisableAllDevModes()
        {
            var modes = FindObjectsOfType<DevModeBase>();
            foreach (var mode in modes)
            {
                mode.enabled = false;
                Debug.Log($"[DevMode] 已禁用: {mode.GetType().Name}");
            }
        }

        [ContextMenu("禁用所有开发模式")]
        void ContextDisableAll()
        {
            DisableAllDevModes();
        }
    }
}
