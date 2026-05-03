using UnityEngine;
using Game.Emotion;
using Game.Config;

namespace Game.Testing.Runners
{
    /// <summary>
    /// 情绪系统手动测试器
    /// 开发者在 Inspector 调参，运行后通过 OnGUI 按钮或快捷键触发
    /// </summary>
    public class EmotionSystemTester : MonoBehaviour
    {
        [Header("测试关卡配置")]
        [Tooltip("拖入 TestLevelConfig 或自定义 LevelConfigSO")]
        public LevelConfigSO testConfig;

        [Header("测试参数")]
        [Tooltip("ChangePanic 测试用 delta")]
        public int panicDelta = 10;
        [Tooltip("ChangeExcite 测试用 delta")]
        public int exciteDelta = 10;
        [Tooltip("DecreaseEmotionWhileEyeClose 测试用模拟时间(秒)")]
        public float simulateEyeCloseSeconds = 1f;
        [Tooltip("DecreaseEmotionWhileEyeClose 测试用降率")]
        public float decreaseRate = 5f;

        [Header("UI 显示")]
        public bool showOnGUI = true;
        public KeyCode initializeKey = KeyCode.F1;
        public KeyCode applyPanicKey = KeyCode.F2;
        public KeyCode applyExciteKey = KeyCode.F3;
        public KeyCode runAllKey = KeyCode.F4;

        private EmotionSystem emotionSystem;

        void Start()
        {
            emotionSystem = EmotionSystem.Instance;
            Debug.Log("[EmotionSystemTester] 已就绪。F1=Initialize / F2=ChangePanic / F3=ChangeExcite / F4=RunAll");
        }

        void Update()
        {
            if (Input.GetKeyDown(initializeKey)) DoInitialize();
            if (Input.GetKeyDown(applyPanicKey)) DoChangePanic();
            if (Input.GetKeyDown(applyExciteKey)) DoChangeExcite();
            if (Input.GetKeyDown(runAllKey)) RunAllTests();
        }

        void OnGUI()
        {
            if (!showOnGUI) return;

            const int w = 320, h = 24;
            int x = 10, y = 10;
            GUI.Box(new Rect(x - 5, y - 5, w + 10, 280), "EmotionSystem 测试器");
            y += 25;

            if (emotionSystem != null)
            {
                var info = emotionSystem.GetEmotionInfo();
                GUI.Label(new Rect(x, y, w, h), $"状态: {info}"); y += h;
                GUI.Label(new Rect(x, y, w, h), $"超临界: {emotionSystem.IsCaughtByCriticalValue()}"); y += h + 4;
            }
            else
            {
                GUI.Label(new Rect(x, y, w, h), "EmotionSystem 实例未就绪"); y += h + 4;
            }

            GUI.Label(new Rect(x, y, 100, h), "Panic Δ:");
            int.TryParse(GUI.TextField(new Rect(x + 100, y, 60, h), panicDelta.ToString()), out panicDelta); y += h;

            GUI.Label(new Rect(x, y, 100, h), "Excite Δ:");
            int.TryParse(GUI.TextField(new Rect(x + 100, y, 60, h), exciteDelta.ToString()), out exciteDelta); y += h + 4;

            if (GUI.Button(new Rect(x, y, w, h), "[F1] Initialize 关卡配置")) DoInitialize(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "[F2] ChangePanic")) DoChangePanic(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "[F3] ChangeExcite")) DoChangeExcite(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), $"模拟闭眼{simulateEyeCloseSeconds}秒(降率{decreaseRate})")) DoDecreaseWhileEyeClose(); y += h;
            if (GUI.Button(new Rect(x, y, w, h), "[F4] 一键跑全部用例")) RunAllTests(); y += h;
        }

        [ContextMenu("Initialize")]
        public void DoInitialize()
        {
            if (testConfig == null)
            {
                Debug.LogError("[EmotionSystemTester] testConfig 未指定");
                return;
            }
            emotionSystem.Initialize(testConfig);
            Debug.Log($"[EmotionSystemTester] Initialize 完成 → {emotionSystem.GetEmotionInfo()}");
        }

        [ContextMenu("ChangePanic")]
        public void DoChangePanic()
        {
            int before = emotionSystem.GetEmotionInfo().panicValue;
            emotionSystem.ChangePanic(panicDelta);
            int after = emotionSystem.GetEmotionInfo().panicValue;
            Debug.Log($"[EmotionSystemTester] ChangePanic({panicDelta}) {before}→{after}");
        }

        [ContextMenu("ChangeExcite")]
        public void DoChangeExcite()
        {
            int before = emotionSystem.GetEmotionInfo().exciteValue;
            emotionSystem.ChangeExcite(exciteDelta);
            int after = emotionSystem.GetEmotionInfo().exciteValue;
            Debug.Log($"[EmotionSystemTester] ChangeExcite({exciteDelta}) {before}→{after}");
        }

        [ContextMenu("DecreaseWhileEyeClose")]
        public void DoDecreaseWhileEyeClose()
        {
            int before = emotionSystem.GetTotalEmotion();
            emotionSystem.DecreaseEmotionWhileEyeClose(simulateEyeCloseSeconds, decreaseRate);
            int after = emotionSystem.GetTotalEmotion();
            Debug.Log($"[EmotionSystemTester] DecreaseEmotionWhileEyeClose({simulateEyeCloseSeconds}s @ {decreaseRate}/s) total {before}→{after}");
        }

        [ContextMenu("RunAllTests")]
        public void RunAllTests()
        {
            Debug.Log("=== EmotionSystem 一键测试 ===");
            DoInitialize();
            DoChangePanic();
            DoChangeExcite();
            // 边界测试
            emotionSystem.ChangePanic(999);
            int high = emotionSystem.GetEmotionInfo().panicValue;
            emotionSystem.ChangePanic(-999);
            int low = emotionSystem.GetEmotionInfo().panicValue;
            Debug.Log($"[EmotionSystemTester] Clamp 测试: 上限={high} 下限={low}");
            DoDecreaseWhileEyeClose();
            Debug.Log("=== 完成 ===");
        }
    }
}
