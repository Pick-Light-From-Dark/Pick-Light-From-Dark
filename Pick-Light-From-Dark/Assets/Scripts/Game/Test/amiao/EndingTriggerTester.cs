using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Test
{
    /// <summary>
    /// 结局触发测试器 — 按数字键直接 Instantiate 对应结局预制体
    /// 不依赖 EndingManager / EndingContentPanel 的动态创建老路
    /// </summary>
    public class EndingTriggerTester : MonoBehaviour
    {
        [Header("结局预制体（Resources 路径或拖拽引用）")]
        public GameObject ending1Prefab;
        public GameObject ending2Prefab;
        public GameObject ending4Prefab;
        public GameObject ending5Prefab;
        public GameObject deathEndingPrefab;

        [Header("运行时按键")]
        public bool enableHotkeys = true;

        private GameObject currentEndingPanel;

        void Update()
        {
            if (!enableHotkeys) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                ShowEnding(ending1Prefab, 6001);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                ShowEnding(ending2Prefab, 6002);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                ShowEnding(ending4Prefab, 6004);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                ShowEnding(ending5Prefab, 6005);
            else if (Input.GetKeyDown(KeyCode.Alpha0))
                ShowEnding(deathEndingPrefab, -1);
            else if (Input.GetKeyDown(KeyCode.Escape) && currentEndingPanel != null)
                Destroy(currentEndingPanel);
        }

        public void ShowEnding(GameObject prefab, int endingId)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[EndingTriggerTester] 结局预制体为 null (ID: {endingId})");
                return;
            }

            // 销毁已显示的结局面板
            if (currentEndingPanel != null)
                Destroy(currentEndingPanel);

            // 查找或创建 Canvas
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("EndingCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // 确保 EventSystem 存在
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            currentEndingPanel = Instantiate(prefab, canvas.transform);
            Debug.Log($"[EndingTriggerTester] 显示结局面板 ID: {endingId}");
        }
    }
}
